using System.Collections.Generic;
using System.Linq;
using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_MethodReturnsProxyIEnumerable
{
    namespace Duck
    {
        public interface IOuter
        {
            IEnumerable<IPropertyValue> Method();
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
            public IEnumerable<IPropertyValue> Method() => new IPropertyValue[] { new PropertyValue() };
        }

        interface INormal
        {
            IEnumerable<IPropertyValue> Method();
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
        public void Duck_As_MethodReturnsProxyIEnumerable()
        {
            object normal = new Normal.Normal();

            var duck = normal.As<Duck.IOuter>();

            var value = duck.Method();
            var result = value.First().Method();

            Assert.AreEqual("hello", result);
        }
    }
}
