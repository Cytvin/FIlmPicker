using FIlmPicker.Services.DatabaseServices;
using System.Threading.Channels;

namespace FIlmPicker.Services
{
    public class BackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, DatabaseService, Task<bool>>> _queue;

        public BackgroundTaskQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait //read about
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, DatabaseService, Task<bool>>>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, DatabaseService, Task<bool>> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _queue.Writer.WriteAsync(workItem);
        }

        public async Task<Func<CancellationToken, DatabaseService, Task<bool>>> DequeueAsync(CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
    }
}
