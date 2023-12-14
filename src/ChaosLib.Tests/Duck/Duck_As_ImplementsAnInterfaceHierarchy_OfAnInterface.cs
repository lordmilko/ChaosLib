using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_ImplementsAnInterfaceHierarchy_OfAnInterface
{
    [TestClass]
    public class TestClass
    {
        public interface IDuck : IDuckParent
        {
            string Method();
        }

        public interface IDuckParent
        {
            string ParentMethod();
        }

        public class Normal : INormal
        {
            public string Method() => "hello";

            public string ParentMethod() => "goodbye";
        }

        interface INormal : INormalParent
        {
            string Method();
        }

        interface INormalParent
        {
            string ParentMethod();
        }

        [TestMethod]
        public void Duck_As_ImplementsAnInterfaceHierarchy_OfAnInterface()
        {
            object normal = new Normal();

            var duck = normal.As<IDuck>();

            var result1 = duck.Method();
            var result2 = duck.ParentMethod();

            Assert.AreEqual("hello", result1);
            Assert.AreEqual("goodbye", result2);
        }
    }
}
