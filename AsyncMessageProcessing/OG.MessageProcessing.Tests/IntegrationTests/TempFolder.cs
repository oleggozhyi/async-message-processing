using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace OG.MessageProcessing.Tests.IntegrationTests
{
    public class TempFolder: IDisposable
    {
        public string Path { get; }
        public TempFolder()
        {
            Path = IOPath.Combine(IOPath.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }

        ~TempFolder()
        {
            Dispose();
        }
    }
}
