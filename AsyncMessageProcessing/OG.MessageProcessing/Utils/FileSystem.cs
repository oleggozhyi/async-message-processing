using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OG.MessageProcessing.Utils
{
    public interface IFileSystem
    {
        void EnsurePathAndSave(string dir, string fileName, string content);
    }
    public class FileSystem : IFileSystem
    {
        public void EnsurePathAndSave(string dir, string fileName, string content)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, fileName), content);
        }
    }
}
