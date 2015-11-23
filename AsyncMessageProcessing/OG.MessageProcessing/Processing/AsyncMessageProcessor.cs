using OG.MessageProcessing.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OG.MessageProcessing.Processing
{
    public class AsyncMessageProcessor
    {
        private readonly BlockingCollection<Message> messageQueue;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly IDateTimeProvider dateTimeProvider;

        public AsyncMessageProcessor(IMessageHandler messageHanlder) : this(messageHanlder, AsyncMessageProcessorOptions.Default, new DateTimeProvider()) { }
        public AsyncMessageProcessor(IMessageHandler messageHanlder, AsyncMessageProcessorOptions options) : this(messageHanlder, options, new DateTimeProvider()) { }
        public AsyncMessageProcessor(IMessageHandler messageHanlder, AsyncMessageProcessorOptions options, IDateTimeProvider dateTimeProvider)
        {
            this.dateTimeProvider = dateTimeProvider;
            messageQueue = new BlockingCollection<Message>(options.UnderlyingCollection);
            new Thread(() =>
            {
                try
                {
                    options.LoopStrategy.Loop(messageQueue.GetConsumingEnumerable(cts.Token), cts.Token, messageHanlder.Handle);
                }
                catch (OperationCanceledException) {/*occurs when stopped*/}
            })
            { IsBackground = true }.Start();
        }

        public void Add<T>(T dataItem) where T : class
        {
            if (IsStopped || IsAddingStopped)
                throw new InvalidOperationException("Agent has been already stopped.");

            var msg = new Message<T>(dataItem, dateTimeProvider.GetNow());
            messageQueue.Add(msg);
        }

        public void StopAdding() => messageQueue.CompleteAdding();
        public void Stop() => cts.Cancel();

        public bool IsAddingStopped => messageQueue.IsAddingCompleted;
        public bool IsStopped => cts.IsCancellationRequested;
        public bool IsQueueEmpty => messageQueue.Count == 0 || cts.IsCancellationRequested;
    }
}
