using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OG.MessageProcessing.Processing
{
    public class Message
    {
        protected Message(object data, DateTime timestamp)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Data = data;
            Timestamp = timestamp;
        }
        public object Data { get; }
        public DateTime Timestamp { get; }
    }
    public class Message<T> : Message where T : class
    {
        public Message(T data, DateTime timestamp) : base(data, timestamp) { }
        public new T Data => (T)base.Data;
    }
}
