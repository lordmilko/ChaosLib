using System.Collections.Generic;
using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_MethodReturnsProxyIList
{
    namespace Duck
    {
        public interface IOuter
        {
            IList<IPropertyValue> Method();
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
            public IList<IPropertyValue> Method() => new List<IPropertyValue> { new PropertyValue() };
        }

        interface INormal
        {
            IList<IPropertyValue> Method();
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
        public void Duck_As_MethodReturnsProxyIList()
        {
            object normal = new Normal.Normal();

            var duck = normal.As<Duck.IOuter>();

            var value = duck.Method();
            var result = value[0].Method();

            Assert.AreEqual("hello", result);
        }
    }
}
