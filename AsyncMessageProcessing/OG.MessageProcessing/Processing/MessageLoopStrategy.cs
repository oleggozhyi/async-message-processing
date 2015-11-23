using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OG.MessageProcessing.Processing
{
    public abstract class MessageLoopStrategy
    {
        public static MessageLoopStrategy SequentialStrategy => new SequentialMessageLoopStrategy();
        public static MessageLoopStrategy ParallelStrategy => new ParallelMessageLoopStrategy();
        public static MessageLoopStrategy ThreadPoolStrategy => new ThreadPoolMessageLoopStrategy();
        public static MessageLoopStrategy TaskPoolStrategy => new TaskMessageLoopStrategy();
        public abstract void Loop(IEnumerable<Message> consumingMessages, CancellationToken ct, Action<Message> processMessage);

        private class SequentialMessageLoopStrategy : MessageLoopStrategy
        {
            public override void Loop(IEnumerable<Message> consumingMessages, CancellationToken ct, Action<Message> processMessage)
            {
                foreach (var m in consumingMessages)
                    processMessage(m);
            }
        }
        private class ParallelMessageLoopStrategy : MessageLoopStrategy
        {
            public override void Loop(IEnumerable<Message> consumingMessages, CancellationToken ct, Action<Message> processMessage)
            {
                var options = new ParallelOptions
                {
                    CancellationToken = ct,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
                Parallel.ForEach(consumingMessages, options, processMessage);
            }
        }
        private class TaskMessageLoopStrategy : MessageLoopStrategy
        {
            public override void Loop(IEnumerable<Message> consumingMessages, CancellationToken ct, Action<Message> processMessage)
            {
                foreach (var m in consumingMessages)
                    Task.Run(() => processMessage(m), ct);
            }
        }
        private class ThreadPoolMessageLoopStrategy : MessageLoopStrategy
        {
            public override void Loop(IEnumerable<Message> consumingMessages, CancellationToken ct, Action<Message> processMessage)
            {
                foreach (var m in consumingMessages)
                    ThreadPool.QueueUserWorkItem(_ => processMessage(m));
            }
        }
    }
}
