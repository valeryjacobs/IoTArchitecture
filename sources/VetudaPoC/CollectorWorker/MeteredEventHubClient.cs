using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CollectorWorker
{
    class MeteredEventHubClient
    {
        private EventHubClient Client { get; }

        private LinkedList<MeterItem<long>> LookBack { get; }

        private TimeSpan LookBackWindow { get; }

        private TimeSpan DelayWindow { get; }

        public double AverageBytesPerSecond { get; }

        public long BytesSent { get; private set; }

        public int EventCount { get; private set; }

        public long TotalEventCount { get; private set; }

        public MeteredEventHubClient(EventHubClient client, long maxBytes, TimeSpan timePeriod)
        {
            Client = client;
            LookBack = new LinkedList<MeterItem<long>>();
            LookBackWindow = TimeSpan.FromMilliseconds(timePeriod.Milliseconds/2);
            DelayWindow = TimeSpan.FromMilliseconds(LookBackWindow.TotalMilliseconds / 2);
            AverageBytesPerSecond = maxBytes / 2;
        }

        public async Task SendBatchAsync(IEnumerable<EventData> data, int maxBatchSizeInBytes, CancellationToken cancellationToken)
        {
            var batchEnum = data.GetEnumerator();
            var batch = new List<EventData>();
            long batchSize = 0;

            while (!cancellationToken.IsCancellationRequested && batchEnum.MoveNext() && batchEnum.Current != null)
            {
                var current = batchEnum.Current;
                var size = current.SerializedSizeInBytes;
                await CheckMeter(size, cancellationToken);

                if ((batchSize + size) > maxBatchSizeInBytes)
                {
                    await SendBatchAsync(batch, cancellationToken);
                    batch.Clear();
                    batchSize = 0;
                }

                batch.Add(current);
                batchSize += size;
            }

            await SendBatchAsync(batch, cancellationToken);
        }

        private async Task SendBatchAsync(List<EventData> batch, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                await Client.SendBatchAsync(batch);
                TotalEventCount += batch.Count;
            }
        }

        private async Task CheckMeter(long size, CancellationToken cancellationToken)
        {
            if (BytesSent <= AverageBytesPerSecond)
            {
                DateTime meterTime = DateTime.Now.Subtract(LookBackWindow);

                var current = LookBack.First;

                while (!cancellationToken.IsCancellationRequested && current != null && current.Value.Time <= meterTime)
                {
                    BytesSent -= current.Value.Value;
                    EventCount--;
                    current = current.Next;
                    LookBack.RemoveFirst();
                }
            }

            LookBack.AddLast(new MeterItem<long>(size));
            BytesSent += size;
            EventCount++;

            var time = (DateTime.UtcNow - LookBack.First.Value.Time).TotalMilliseconds;

            if (BytesSent / time >= AverageBytesPerSecond)
            {
                await Backoff(cancellationToken);
            }
        }

        private Task Backoff(CancellationToken cancellationToken)
        {
            // Put in in some exponential back off data
            return Task.Delay(DelayWindow, cancellationToken);
        }

        private class MeterItem<T>
        {
            public MeterItem(T value)
            {
                this.Value = value;
                this.Time = DateTime.UtcNow;
            }

            public DateTime Time { get; }

            public T Value { get; }
        }
    }
}
