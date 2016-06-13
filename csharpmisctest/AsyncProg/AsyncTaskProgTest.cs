using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csharpmisctest.AsyncProg
{
    using System.Diagnostics;
    using System.Threading;

    using csharpmisc.AsyncProg;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
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
            var stockData = StockDataDownloader.GetHistoricalData("msft", 10);

            decimal max = stockData.Prices.Max();
            decimal min = stockData.Prices.Min();

            decimal avg = 0;
            int count = 0;
            foreach (var price in stockData.Prices)
            {
                avg = (avg * count + price) / (count + 1);
                count++;
            }

            decimal stddev = 0;
            foreach (var price in stockData.Prices)
            {
                stddev += Math.Abs(avg - price) / count;
            }

            Assert.IsTrue(avg > 0);

            System.Diagnostics.Debug.WriteLine("Max: {0}", max);
            System.Diagnostics.Debug.WriteLine("Min: {0}", min);
            System.Diagnostics.Debug.WriteLine("Avg: {0}", avg);
            System.Diagnostics.Debug.WriteLine("Std Dev: {0}", stddev);
        }

        [TestMethod]
        public void TestTaskWaitAll()
        {
            var stockData = StockDataDownloader.GetHistoricalData("msft", 10);

            decimal max = 0, min = 0, avg = 0, stddev = 0;
            var taskList = new List<Task>();

            var tMax = new Task(() => { max = stockData.Prices.Max(); });
            var tMin = new Task(() => { min = stockData.Prices.Min(); });

            int count = 0;
            var tAvg = new Task(() =>
            {
                foreach (var price in stockData.Prices)
                {
                    avg = (avg * count + price) / (count + 1);
                    count++;
                }
            });

            // Dependency on tAvg not resolved
            // Should give different results in different runs
            var tStddev = new Task(() =>
            {
                foreach (var price in stockData.Prices)
                {
                    stddev += Math.Abs(avg - price) / count;
                }
            });

            taskList.Add(tMax);
            taskList.Add(tMin);
            taskList.Add(tAvg);
            taskList.Add(tStddev);

            Task.WaitAll(taskList.ToArray());

            System.Diagnostics.Debug.WriteLine("Max: {0}", max);
            System.Diagnostics.Debug.WriteLine("Min: {0}", min);
            System.Diagnostics.Debug.WriteLine("Avg: {0}", avg);
            System.Diagnostics.Debug.WriteLine("Std Dev: {0}", stddev);
        }

        [TestMethod]
        public void TestWaitCallsStart()
        {
            int result = 0;
            var t = new Task<bool>(() =>
            {
                Task.Delay(5000);
                System.Diagnostics.Debug.WriteLine("Task Called");
                result = 1;
                return true;
            });

            var taskList = new List<Task>();
            taskList.Add(t);
            t.Start();
            // Doesn't work if the task isn't started
            Task.WaitAll(taskList.ToArray());
            
            Assert.IsTrue(result == 1);

        }

        private class HelperTestClass
        {
            public int result;
        }

        [TestMethod]
        public void TestParameterPassingToTask()
        {
            var param = new HelperTestClass() { result = 0 };
            var t = new Task<bool>((r) =>
            {
                Task.Delay(5000);
                System.Diagnostics.Debug.WriteLine("Task Called");
                ((HelperTestClass)r).result = 1;
                return true;
            }, param);

            var taskList = new List<Task>();
            taskList.Add(t);
            t.Start();
            // Doesn't work if the task isn't started
            Task.WaitAll(taskList.ToArray());

            Assert.IsTrue(param.result == 1);

        }

        [TestMethod]
        public void TestContinueWith()
        {
            var param = new HelperTestClass() { result = 0 };
            var t = new Task<bool>((r) =>
            {
                Task.Delay(1000);
                System.Diagnostics.Debug.WriteLine("First Task Called");
                ((HelperTestClass)r).result = 1;
                return true;
            }, param);

            var t2 = t.ContinueWith((antecedent, r) => 
            {
                System.Diagnostics.Debug.WriteLine("Second Task Called. First task returned {0}", antecedent.Result);
                ((HelperTestClass)r).result = 2;
            }, param);

            t.Start();

            // Start cannot be called on a continuation task
            //t2.Start();
            // Doesn't work if the task isn't started
            Task.WaitAll(t, t2);

            Assert.IsTrue(param.result == 2);

        }

        private static int HelperTestClassUpdate(HelperTestClass tp)
        {
            Random r = new Random((int)DateTime.Now.Ticks);
            Task.Delay(r.Next(2000));
            System.Diagnostics.Debug.WriteLine("Task Called {0}", tp.result);
            tp.result++;
            return tp.result;
        }

        [TestMethod]
        public void TestWaitAllOneByOnePattern()
        {
            var param = new HelperTestClass() { result = 0 };
            var t1 = new Task<int>((r) =>
            {
                return HelperTestClassUpdate((HelperTestClass)r);
            }, param);

            var t2 = new Task<int>((r) =>
            {
                return HelperTestClassUpdate((HelperTestClass)r);
            }, param);

            var t3 = new Task<int>((r) =>
            {
                return HelperTestClassUpdate((HelperTestClass)r);
            }, param);

            var taskList = new List<Task<int>>();
            taskList.Add(t1);
            taskList.Add(t2);
            taskList.Add(t3);
            taskList.ForEach(t => t.Start());
            while(taskList.Count > 0)
            {
                int index = Task.WaitAny(taskList.ToArray());
                Assert.IsTrue(taskList[index].Exception == null);
                Assert.IsTrue(taskList[index].Result > 0);
                taskList.RemoveAt(index);
            }

            Assert.IsTrue(param.result > 0);
        }

    }
}
