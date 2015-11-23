using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OG.MessageProcessing.Processing
{
    public interface IMessageHandler
    {
        void Handle(Message msg);
    }
}
