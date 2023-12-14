using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_WrapsAProperty_OfAnInterface
{
    class Normal : INormal
    {
        public string Property { get; } = "hello";
    }

    interface INormal
    {
        string Property { get; }
    }

    interface IDuck
    {
        string Property { get; }
    }

    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void Duck_As_WrapsAProperty_OfAnInterface()
        {
            object normal = new Normal();

            var duck = normal.As<IDuck>();

            var result = duck.Property;

            Assert.AreEqual("hello", result);
        }
    }
}
