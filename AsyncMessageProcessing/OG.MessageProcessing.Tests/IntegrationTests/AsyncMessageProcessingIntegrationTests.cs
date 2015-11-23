using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework.Internal;
using NUnit.Framework;
using OG.MessageProcessing.Persisting;
using OG.MessageProcessing.Processing;
using OG.MessageProcessing.Utils;

namespace OG.MessageProcessing.Tests.IntegrationTests
{
    [TestFixture]
    public class AsyncMessageProcessingIntegrationTests : TestBase
    {
        public IDateTimeProvider DateTimeProvider { get; set; }
        private AsyncMessageProcessor CreateSut(string rootDir)
        {
            var handler = new FileMessagePersister(new FileMessagePersisterSettings
            {
                RootDirectory = rootDir,
                DirectoryFormat = "{Timestamp:yyyyMMdd}",
                FileNameFormat = "{Data}.log"
            }, new FileSystem());
            return new AsyncMessageProcessor(handler, AsyncMessageProcessorOptions.Default, DateTimeProvider);
        }


        [Test]
        public async Task Verifies_that_processing_saves_files_to_disk()
        {
            using (var tmpFolder = new TempFolder())
            {
                //Arragne
                DateTimeProvider.GetNow().Returns(23.June(2013).At(23, 0, 0));
                var sut = CreateSut(tmpFolder.Path);

                //Act
                sut.Add("msg1");
                await sut.WaitFor(s => s.IsQueueEmpty);

                //Assert
                var expectedFileName = Path.Combine(tmpFolder.Path, @"20130623\msg1.log");
                File.Exists(expectedFileName).Should().BeTrue();
            }
        }

        [Test]
        public async Task Verifies_that_next_folder_is_created_after_midnight_is_crossed()
        {
            using (var tmpFolder = new TempFolder())
            {
                //Arragne
                DateTimeProvider.GetNow().Returns(23.June(2013).At(23, 59, 59),
                                                  24.June(2013).At(0, 0, 1));
                var sut = CreateSut(tmpFolder.Path);

                //Act
                sut.Add("msg1");
                sut.Add("msg2");
                await sut.WaitFor(s => s.IsQueueEmpty);

                //Assert
                var expectedFileName1 = Path.Combine(tmpFolder.Path, @"20130623\msg1.log");
                var expectedFileName2 = Path.Combine(tmpFolder.Path, @"20130624\msg2.log");
                File.Exists(expectedFileName1).Should().BeTrue();
                File.Exists(expectedFileName2).Should().BeTrue();
            }
        }
    }
}
