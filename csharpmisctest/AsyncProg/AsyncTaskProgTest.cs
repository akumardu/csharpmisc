using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csharpmisctest.AsyncProg
{
    using System.Diagnostics;
    using System.Threading;

    using csharpmisc.AsyncProg;

    [TestClass]
    public class AsyncTaskProgTest
    {
        [TestMethod]
        public void TestLongRunningUtilSimulation()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = Utils.ComputeIntensiveSimulation();
            stopWatch.Stop();
            var duration = stopWatch.Elapsed;

            Assert.IsTrue(duration.Seconds > 1);

        }

        [TestMethod]
        public void TestStockDataDownloaderTask()
        {
            AsyncTaskProg.FindStockInfo("msft");
            Console.WriteLine("Test");
        }
    }
}
