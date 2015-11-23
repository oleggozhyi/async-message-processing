using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OG.MessageProcessing.Persisting
{
    public class FileMessagePersisterSettings
    {
        public static readonly FileMessagePersisterSettings Default = new FileMessagePersisterSettings();
        public string DirectoryFormat { get; set; } = "{Timestamp:yyyyMMdd}";
        public string RootDirectory { get; set; } = ".\\Messages";
        public string ContentFormat { get; set; } = "Received message at [Timestamp:default:yyyy-MM-dd HH:mm:ss.ffff] with body '[Data]'";
        public string FileNameFormat { get; set; } = "{Data} - {Timestamp:yyyyMMdd HHmmss fff}.log";
    }
}
