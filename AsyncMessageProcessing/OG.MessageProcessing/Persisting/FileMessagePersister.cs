using OG.MessageProcessing.Processing;
using OG.MessageProcessing.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFormat;

namespace OG.MessageProcessing.Persisting
{
    public class FileMessagePersister : IMessageHandler
    {
        private readonly FileMessagePersisterSettings settings;
        private readonly IFileSystem fileSystem;

        public FileMessagePersister(FileMessagePersisterSettings settings) : this(settings, new FileSystem()) { }
        public FileMessagePersister(FileMessagePersisterSettings settings, IFileSystem fileSystem)
        {
            this.settings = settings;
            this.fileSystem = fileSystem;
        }
        public void Handle(Message msg)
        {
            var m = msg as Message<string>;
            if (m == null)
                throw new InvalidOperationException("I can handle only strings for now. Refactor me to use a message dispatcher");
            try
            {
                fileSystem.EnsurePathAndSave(GetDir(m), GetFileName(m), GetContent(m));
            }
            catch (Exception ex)
            {
                //swallowing as an individual failure mustn't shut down the processor
                Console.WriteLine($"Persisting failed with {ex.GetType().Name} - {ex.Message}");
            }
        }

        private string GetDir(Message<string> msg) => 
            Path.Combine(settings.RootDirectory,Smart.Format(settings.DirectoryFormat, msg));

        private string GetFileName(Message<string> msg) =>
            Smart.Format(settings.FileNameFormat, msg);

        private string GetContent(Message<string> msg) =>
            Smart.Format(settings.ContentFormat, msg);
    }
}
