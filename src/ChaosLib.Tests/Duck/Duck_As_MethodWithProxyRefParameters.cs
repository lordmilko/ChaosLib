using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_MethodWithProxyRefParameters
{
    namespace Duck
    {
        public interface IOuter
        {
            void Method(ref IPropertyValue value);
        }

        public interface IPropertyValue
        {
            string Property { get; set; }
        }
    }

    namespace Normal
    {
        class Normal : INormal
        {
            public string Result { get; set; }

            public void Method(ref IPropertyValue value)
            {
                Result = value.Property;

                value = new PropertyValue
                {
                    Property = "goodbye"
                };
            }
        }

        interface INormal
        {
            void Method(ref IPropertyValue value);
        }

        interface IPropertyValue
        {
            string Property { get; set; }
        }

        class PropertyValue : IPropertyValue
        {
            public string Property { get; set; }
        }
    }

    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void Duck_As_MethodWithProxyRefParameters()
        {
            var normal = new Normal.Normal();

            var duck = normal.As<Duck.IOuter>();

            var value = new Normal.PropertyValue
            {
                Property = "hello"
            }.As<Duck.IPropertyValue>();

            duck.Method(ref value);

            var result1 = normal.Result;
            var result2 = value.Property;

            Assert.AreEqual("hello", result1);
            Assert.AreEqual("goodbye", result2);
        }
    }
}
