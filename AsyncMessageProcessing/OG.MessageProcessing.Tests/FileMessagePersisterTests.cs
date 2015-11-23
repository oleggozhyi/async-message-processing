using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OG.MessageProcessing.Persisting;
using OG.MessageProcessing.Processing;
using OG.MessageProcessing.Utils;

namespace OG.MessageProcessing.Tests
{
    [TestFixture]
    public class FileMessagePersisterTests: TestBase
    {
        public IFileSystem FileSystem { get; set; }
        private FileMessagePersisterSettings settings;
        private FileMessagePersister CreateSut() => new FileMessagePersister(settings, FileSystem);

        [SetUp]
        public void TestSetUp()
        {
            settings = new FileMessagePersisterSettings
            {
                RootDirectory = "root",
                DirectoryFormat = "{Timestamp:yyyyMMdd}",
                ContentFormat = "{Data} {Timestamp:yyyy-MM-dd-HH-mm-ss-fff}",
                FileNameFormat = "{Data} {Timestamp:yyyy-MM-dd-HH-mm-ss-fff}.log"
            };
        }

        [Test]
        public void Formats_content_correctly()
        {
            //Arrange
            var sut = CreateSut();
            var message = new Message<string>("msg123", 21.November(2015).At(23, 45, 11, 456));
            //Act
            sut.Handle(message);
            //Assert
            FileSystem.Received().EnsurePathAndSave(dir: Arg.Any<string>(), fileName: Arg.Any<string>(),
                content: "msg123 2015-11-21-23-45-11-456");
        }
        [Test]
        public void Formats_filename_correctly()
        {
            //Arrange
            var sut = CreateSut();
            var message = new Message<string>("msg123", 21.November(2015).At(23, 45, 11, 456));
            //Act
            sut.Handle(message);
            //Assert
            FileSystem.Received().EnsurePathAndSave(dir: Arg.Any<string>(),
                fileName: "msg123 2015-11-21-23-45-11-456.log",
                content: Arg.Any<string>());
        }
        [Test]
        public void Formats_irectory_correctly()
        {
            //Arrange
            var sut = CreateSut();
            var message = new Message<string>("msg123", 21.November(2015).At(23, 45, 11, 456));
            //Act
            sut.Handle(message);
            //Assert
            FileSystem.Received().EnsurePathAndSave(
                dir: @"root\20151121",
                fileName: Arg.Any<string>(),
                content: Arg.Any<string>());
        }
    }
}
