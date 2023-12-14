using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_ImplementsIProxyT_OfAnInterface
{
    public interface IDuck
    {
        string Method();
    }

    class Normal : INormal
    {
        public string Method() => "hello";
    }

    interface INormal
    {
        string Method();
    }

    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void Duck_As_ImplementsIProxyT_OfAnInterface()
        {
            object normal = new Normal();

            var duck = normal.As<IDuck>();

            var result = ((IDuckProxy<Normal>)duck).GetInner();

            Assert.AreEqual(normal, result);
        }
    }
}
