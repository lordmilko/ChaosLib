using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_MethodWithProxyOutParameters
{
    namespace Duck
    {
        public interface IOuter
        {
            void Method(out IPropertyValue value);
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
            public void Method(out IPropertyValue value)
            {
                value = new PropertyValue();
            }
        }

        interface INormal
        {
            void Method(out IPropertyValue value);
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
        public void Duck_As_MethodWithProxyOutParameters()
        {
            object normal = new Normal.Normal();

            var duck = normal.As<Duck.IOuter>();

            Duck.IPropertyValue value;
            duck.Method(out value);

            var result = value.Method();

            Assert.AreEqual("hello", result);
        }
    }
}
