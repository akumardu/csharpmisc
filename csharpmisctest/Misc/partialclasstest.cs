using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharpmisc.Misc;

namespace csharpmisctest.Misc
{
    [TestClass]
    public class partialclasstest
    {
        [TestMethod]
        public void TestPartialClass()
        {
            var partClass = new partialclass();
            Assert.IsTrue(partClass.FirstFunc().Equals("FirstFunc"));
            Assert.IsTrue(partClass.SecondFunc().Equals("SecondFunc"));
        }

        [TestMethod]
        public void TestPartialMethod()
        {
            var partClass = new partialclass();
            int val = 0;
            partClass.CallPartialMethod(ref val);
            Assert.IsTrue(val == 6);
        }
    }
}
