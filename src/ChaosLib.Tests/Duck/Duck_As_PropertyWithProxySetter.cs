using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_PropertyWithProxySetter
{
    namespace Duck
    {
        public interface IOuter
        {
            IPropertyValue First { get; set; }
            IPropertyValue Second { get; set; }
        }

        public interface IPropertyValue
        {
            string Value { get; set; }
        }
    }

    namespace Normal
    {
        class Normal : INormal
        {
            public IPropertyValue First { get; set; }
            public IPropertyValue Second { get; set; }
        }

        interface INormal
        {
            IPropertyValue First { get; set; }
            IPropertyValue Second { get; set; }
        }

        interface IPropertyValue
        {
            string Value { get; set; }
        }

        class PropertyValue : IPropertyValue
        {
            public string Value { get; set; }
        }
    }

    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void Duck_As_PropertyWithProxySetter()
        {
            var normal = new Normal.Normal
            {
                First = new Normal.PropertyValue
                {
                    Value = "first"
                }
            };

            var duck = normal.As<Duck.IOuter>();

            var value1 = duck.First;
            Assert.IsNotNull(value1);
            Assert.IsNull(duck.Second);
            value1.Value = "second";

            duck.Second = value1;
            var value2 = duck.Second;

            Assert.AreEqual("second", value2.Value);
        }
    }
}
