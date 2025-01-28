using System.Threading.Channels;

namespace FIlmPicker.Services
{
    public class BackgroundTaskQueue
    {
        private readonly Channel<Action<CancellationToken>> _queue;

        public BackgroundTaskQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait //read about
            };
            _queue = Channel.CreateBounded<Action<CancellationToken>>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(Action<CancellationToken> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _queue.Writer.WriteAsync(workItem);
        }

        public async Task<Action<CancellationToken>> DequeueAsync(CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
    }
}
