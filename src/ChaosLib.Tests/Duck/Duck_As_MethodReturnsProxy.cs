using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_MethodReturnsProxy
{
    namespace Duck
    {
        public interface IOuter
        {
            IPropertyValue Method();
        }

        public interface IPropertyValue
        {
            string Method();
        }
    }

    namespace Normal
    {
        class Normal : INormal
        {
            public IPropertyValue Method() => new PropertyValue();
        }

        interface INormal
        {
            IPropertyValue Method();
        }

        interface IPropertyValue
        {
            string Method();
        }

        class PropertyValue : IPropertyValue
        {
            public string Method() => "hello";
        }
    }

    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void Duck_As_MethodReturnsProxy()
        {
            object normal = new Normal.Normal();

            var duck = normal.As<Duck.IOuter>();

            var value = duck.Method();
            var result = value.Method();

            Assert.AreEqual("hello", result);
        }
    }
}
