using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using NSubstitute;
using NUnit.Framework.Internal;
using NUnit.Framework;
using OG.MessageProcessing.Processing;
using OG.MessageProcessing.Utils;

namespace OG.MessageProcessing.Tests
{
    [TestFixture]
    public class AsyncMessageProcessorTests : TestBase
    {
        private readonly DateTime aTimestamp = 10.March(2014).At(13, 31, 45);
        public IMessageHandler Hander { get; set; }
        public IDateTimeProvider DateTimeProvider { get; set; }
        public AsyncMessageProcessor CreateSut() => new AsyncMessageProcessor(Hander, AsyncMessageProcessorOptions.Default, DateTimeProvider);

        [TestFixture]
        public class Add : AsyncMessageProcessorTests
        {
            [Test]
            public async Task Calls_hanlder_for_each_message()
            {
                //Arrange
                var sut = CreateSut();
                //Act
                Enumerable.Range(1, 30).ForEach(i=> sut.Add($"msg{i}"));
                sut.StopAdding();
                await sut.WaitFor(s => s.IsQueueEmpty);
                //Assert
                Hander.Received(30).Handle(Arg.Any<Message<string>>());
            }

            [Test]
            public async Task Wrappes_data_with_message_object()
            {
                //Arrange
                DateTimeProvider.GetNow().Returns(aTimestamp);
                var sut = CreateSut();
                //Act
                sut.Add("msg1");
                sut.StopAdding();
                await sut.WaitFor(s => s.IsQueueEmpty);
                //Assert
                Hander.Received(1).Handle(Arg.Is<Message<string>>(msg => msg.Data == "msg1" && msg.Timestamp == aTimestamp ));
            }

            [Test]
            public async Task Processes_item_asyncronously()
            {
                //Arrange
                int handlerThreadId = -1, processorThreadId = Thread.CurrentThread.ManagedThreadId;
                Hander.When(h => h.Handle(Arg.Any<Message<string>>())).Do(_ => handlerThreadId = Thread.CurrentThread.ManagedThreadId);
                var sut = CreateSut();
                //Act
                sut.Add("Dummy");
                sut.StopAdding();
                await sut.WaitFor(s => s.IsQueueEmpty);
                //Assert
                handlerThreadId.Should().NotBe(processorThreadId).And.BePositive();
            }
        }

        [TestFixture]
        public class StopAdding : AsyncMessageProcessorTests
        {
            [Test]
            public void Forbids_adding_new_items()
            {
                //Arrange
                var sut = CreateSut();
                sut.Add("Dummy");
                //Act
                sut.StopAdding();
                Action addingAnotherItem = () => sut.Add("Dummy1");
                //Assert
                addingAnotherItem.ShouldThrow<InvalidOperationException>();
            }

            [Test]
            public async Task Processes_outstanding_items()
            {
                //Arrange
                var waitHandle = new ManualResetEvent(false);
                Hander.When(h => h.Handle(Arg.Any<Message<string>>())).Do(_ => waitHandle.WaitOne(500));
                var sut = CreateSut();
                Enumerable.Range(1, 30).ForEach(i => sut.Add($"msg{i}"));
                //Act
                sut.StopAdding();
                waitHandle.Set();//let the pipeline continue
                await sut.WaitFor(s => s.IsQueueEmpty);
                //Assert
                Hander.Received(30).Handle(Arg.Any<Message<string>>());
            }
        }

        [TestFixture]
        public class Stop: AsyncMessageProcessorTests
        {
            [Test]
            public void Forbids_adding_new_items()
            {
                //Arrange
                var sut = CreateSut();
                sut.Add("Dummy");
                //Act
                sut.Stop();
                Action addingAnotherItem = () => sut.Add("Dummy1");
                //Assert
                addingAnotherItem.ShouldThrow<InvalidOperationException>();
            }

            [Test]
            public async Task Cancels_processing_outstanding_items()
            {
                //Arrange
                var waitHandle = new ManualResetEvent(false);
                Hander.When(h => h.Handle(Arg.Any<Message<string>>())).Do(_ => waitHandle.WaitOne(500));
                var sut = CreateSut();
                Enumerable.Range(1, 30).ForEach(i => sut.Add($"msg{i}"));
                //Act
                sut.Stop();
                waitHandle.Set();//let the pipeline continue
                await sut.WaitFor(s => s.IsQueueEmpty);
                //Assert
                Hander.Received(1).Handle(Arg.Any<Message<string>>());
            }
        }
    }
}
