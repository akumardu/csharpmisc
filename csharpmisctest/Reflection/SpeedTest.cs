using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csharpmisc.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csharpmisctest.Reflection
{
    [TestClass]
    public class SpeedTest
    {
        [TestMethod]
        public void TestCreationTimeForReflection()
        {
            SpeedTestForReflection sp = new SpeedTestForReflection();
            var timeTakenWithoutReflection = sp.CreateNormalList(1000000);
            var timeTakenWithReflection = sp.CreateReflectedList(1000000);
            Console.WriteLine("Without " + timeTakenWithoutReflection);
            Console.WriteLine("With " + timeTakenWithReflection);
            Assert.IsTrue(timeTakenWithReflection > timeTakenWithoutReflection);
        }

        [TestMethod]
        public void TestAdditionTimeForReflection()
        {
            SpeedTestForReflection sp = new SpeedTestForReflection();
            var timeTakenWithoutReflection = sp.AddToNormalList(1000000);
            var timeTakenWithReflection = sp.AddToReflectedList(1000000);
            Console.WriteLine("Without " + timeTakenWithoutReflection);
            Console.WriteLine("With " + timeTakenWithReflection);
            Assert.IsTrue(timeTakenWithReflection > timeTakenWithoutReflection);
        }
    }
}
