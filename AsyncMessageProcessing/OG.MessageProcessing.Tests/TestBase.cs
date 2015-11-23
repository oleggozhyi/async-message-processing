using System.Linq;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using OG.MessageProcessing.Utils;

namespace OG.MessageProcessing.Tests
{
    public class TestBase
    {
        [SetUp]
        public void SetUp()
        {
            GetType().GetTypeInfo()
                .GetRuntimeProperties()
                .Where(p => p.CanWrite)
                .ForEach(p =>
                {
                    p.SetValue(this, Substitute.For(new[] { p.PropertyType }, new object[0]));
                });
            OnSetUp();
        }

        public virtual void OnSetUp() { }
    }
}