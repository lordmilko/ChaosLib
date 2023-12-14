using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_PropertyReturnsProxy
{
    namespace Duck
    {
        public interface IOuter
        {
            IPropertyValue Property { get; }
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
            public IPropertyValue Property => new PropertyValue();
        }

        interface INormal
        {
            IPropertyValue Property { get; }
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
        public void Duck_As_PropertyReturnsProxy()
        {
            object normal = new Normal.Normal();

            var duck = normal.As<Duck.IOuter>();

            var value = duck.Property;
            var result = value.Method();

            Assert.AreEqual("hello", result);
        }
    }
}
