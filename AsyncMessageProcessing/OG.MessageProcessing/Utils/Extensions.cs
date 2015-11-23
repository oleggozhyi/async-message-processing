using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OG.MessageProcessing.Utils
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> seq, Action<T> f)
        {
            foreach (var item in seq)
            {
                f(item);
            }
        }
        
        public static async Task WaitFor<T>(this T obj, Func<T, bool> exitWaitCondition, int msTimeout = 500, int msPollInterval=5, bool noLogs = false)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(msTimeout);
            await Task.Run(async () =>
            {
                for(bool canExit = false; !canExit; canExit = exitWaitCondition(obj))
                {
                    if(!noLogs)
                        Console.WriteLine($"Waiting another {msPollInterval}ms");
                    await Task.Delay(msPollInterval, cts.Token);
                }
            }, cts.Token);
        }
    }
}
