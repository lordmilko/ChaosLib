using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_MethodReturnsDifferentEnum
{
    namespace Duck
    {
        public enum SomeEnum
        {
            First,
            Second
        }

        public interface IDuck
        {
            SomeEnum Method();
        }
    }

    namespace Normal
    {
        public enum SomeEnum
        {
            foo,
            bar
        }

        class Normal : INormal
        {
            public SomeEnum Method() => SomeEnum.bar;
        }

        interface INormal
        {
            SomeEnum Method();
        }
    }

    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void Duck_As_MethodReturnsDifferentEnum()
        {
            object normal = new Normal.Normal();

            var duck = normal.As<Duck.IDuck>();

            var result = duck.Method();

            Assert.AreEqual(Duck.SomeEnum.Second, result);
        }
    }
}
