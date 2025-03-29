using FIlmPicker.Services.DatabaseServices;

namespace FIlmPicker.Services
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger<QueuedHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public QueuedHostedService(BackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger, IServiceProvider serviceProvider)
        {
            TaskQueue = taskQueue;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public BackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Queued Hosted Service is running.{Environment.NewLine}");

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                MovieListUpdater movieListUpdater = scope.ServiceProvider.GetRequiredService<MovieListUpdater>();
                await movieListUpdater.InitializeQueue();
            }
            
            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem =
                    await TaskQueue.DequeueAsync(stoppingToken);

                try
                {
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        DatabaseService databaseService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
                        bool result = await workItem(stoppingToken, databaseService);

                        if (result == false)
                        {
                            await TaskQueue.QueueBackgroundWorkItemAsync(workItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));

                    await TaskQueue.QueueBackgroundWorkItemAsync(workItem);
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
