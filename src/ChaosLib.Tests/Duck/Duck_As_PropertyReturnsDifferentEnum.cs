using ChaosLib.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duck_As_PropertyReturnsDifferentEnum
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
            SomeEnum Property { get; }
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
            public SomeEnum Property { get; } = SomeEnum.bar;
        }

        interface INormal
        {
            SomeEnum Property { get; }
        }
    }

    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void Duck_As_PropertyReturnsDifferentEnum()
        {
            object normal = new Normal.Normal();

            var duck = normal.As<Duck.IDuck>();

            var result = duck.Property;

            Assert.AreEqual(Duck.SomeEnum.Second, result);
        }
    }
}
