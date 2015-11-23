using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OG.MessageProcessing.Processing
{
    public class AsyncMessageProcessorOptions
    {
        public static readonly AsyncMessageProcessorOptions Default = new AsyncMessageProcessorOptions();

        public MessageLoopStrategy LoopStrategy { get; set; } = MessageLoopStrategy.SequentialStrategy;
        public IProducerConsumerCollection<Message> UnderlyingCollection { get; set; } = new ConcurrentQueue<Message>();
    }
}
