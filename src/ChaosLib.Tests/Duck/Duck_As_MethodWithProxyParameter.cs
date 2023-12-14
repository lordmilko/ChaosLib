using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_MethodWithProxyParameter
{
    namespace Duck
    {
        public interface IOuter
        {
            void Method(IPropertyValue value);
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
            public string Result { get; set; }

            public void Method(IPropertyValue value)
            {
                Result = value.Method();
            }
        }

        interface INormal
        {
            void Method(IPropertyValue value);
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
        public void Duck_As_MethodWithProxyParameter()
        {
            var normal = new Normal.Normal();
            var value = new Normal.PropertyValue().As<Duck.IPropertyValue>();

            var duck = normal.As<Duck.IOuter>();

            duck.Method(value);

            var result = normal.Result;

            Assert.AreEqual("hello", result);
        }
    }
}
