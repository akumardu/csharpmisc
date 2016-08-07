using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using csharpmisc.AsyncProg;

namespace csharpmisctest.AsyncProg
{
    [TestClass]
    public class ParallelForTest
    {
        [TestMethod]
        public void BasicParallelForTest()
        {
            var parallelForResult = Parallel.For(1, 10, i =>
            {
                Debug.WriteLine("Loop Called with value " + i);
            });

            Assert.IsTrue(parallelForResult.IsCompleted);
        }

        [TestMethod]
        public void ParallelForWithStopTest()
        {
            var started = new List<int>();
            var completed = new List<int>();
            bool stopStarted = false;
            var parallelForResult = Parallel.For(1, 10, (i, loopState) =>
            {
                // Adding only loops that started before Stop was called
                if (!stopStarted)
                {
                    started.Add(i);
                }
                
                Debug.WriteLine("Started Loop " + i);
                Thread.Sleep(100);

                if (i == 5)
                {
                    loopState.Stop();
                    stopStarted = true;
                }

                Debug.WriteLine("Completed Loop " + i + ":" + loopState.IsStopped);

                // Record all completed loops
                completed.Add(i);
            });

            // When Stop is called all the threads which were already executing 
            // will continue to execute, but the system won't spawn new threads
            Assert.IsTrue(started.Count == completed.Count);

            // IsCompleted is false when we stop
            Assert.IsFalse(parallelForResult.IsCompleted);

            started.Sort();
            completed.Sort();
            for (int i = 0; i < started.Count; i++)
            {
                Assert.IsTrue(started[i] == completed[i]);
            }
        }

        [TestMethod]
        public void ParallelForWithSimpleBreakTest()
        {
            var started = new List<int>();
            var completed = new List<int>();
            bool stopStarted = false;
            var parallelForResult = Parallel.For(1, 10, (i, loopState) =>
            {
                // Adding only loops that started before Stop was called
                if (!stopStarted)
                {
                    started.Add(i);
                }

                Debug.WriteLine("Started Loop " + i);
                Thread.Sleep(100);

                if (i == 5)
                {
                    stopStarted = true;
                    loopState.Break();
                }

                Debug.WriteLine("Completed Loop " + i + " : " + loopState.LowestBreakIteration);
                completed.Add(i);
            });

            // When Break is called all the threads which were already executing 
            // will continue to execute,
            // Iterations prior to the current one still need to be run
            // Break implies ordering
            Assert.IsTrue(started.Count <= completed.Count);

            // IsCompleted is false when we break
            Assert.IsFalse(parallelForResult.IsCompleted);

            completed.Sort();
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(completed[i] == (i+1));
            }
        }

        [TestMethod]
        public void BasicParallelForEachTest()
        {
            IEnumerable<int> list = new List<int> {1, 2, 3, 4, 5, 6};
            var parallelResult = Parallel.ForEach(list, element =>
            {
                Debug.WriteLine("Loop Executed: " + element);
            });

            Assert.IsTrue(parallelResult.IsCompleted);
        }

        [TestMethod]
        public void ParallelForEachWithExceptionTest()
        {
            IEnumerable<int> list = new List<int> { 1, 2, 3, 4, 5, 6 };
            ParallelLoopResult parallelResult;
            try
            {
                parallelResult = Parallel.ForEach(list, (element, loopState) =>
                {
                    Debug.WriteLine("Loop Executed: " + element + " ExceptionState: " + loopState.IsExceptional);
                    if(element == 3)
                    {
                        throw new ArgumentException("Invalid Argument Exception");
                    }
                });
            }
            catch(Exception ex)
            {
                // Exception thrown within Parallel Foreach is grouped together
                // in Aggregate Exception
                Assert.IsTrue(ex.GetType() == typeof(AggregateException));

                var listOfExceptions = ((AggregateException)ex).Flatten();
                Assert.IsTrue(listOfExceptions.InnerExceptions.First().GetType() == typeof(ArgumentException));
            }
        }

        [TestMethod]
        public void BasicPLinqAsParallelTest()
        {
            List<int> inputData = new List<int>() { 1, 2, 3, 4, 5 };

            // AsParallel enables parallel processing of LINQ to objects query
            foreach(var o in inputData.AsParallel().Select(i =>
            {
                // This will run in parallel
                Debug.WriteLine("Select: {0}", i);
                return i * 10;
            }))
            {
                // This will start running as soon as inputs become available
                // but this is serial
                Debug.WriteLine("Inside Loop: {0}", o);
            }
        }

        [TestMethod]
        public void BasicPLinqForAllTest()
        {
            List<int> inputData = new List<int>() { 1, 2, 3, 4, 5 };

            // ForAll operator avoids the merge and executes a delegate 
            // for each output element
            inputData.AsParallel().Select(i =>
            {
                // This will run in parallel
                Debug.WriteLine("Select: {0}", i);
                return i * 10;
            }).ForAll(o =>
            {
                // This will run in parallel to one above
                Debug.WriteLine("Inside Loop: {0}", o);
            });
        }

        private static IEnumerable<int> Iterate(int from, int to, int step)
        {
            for(int i = from; i < to; i += step)
            {
                Debug.WriteLine("Thread Id: {0} for {1}", Thread.CurrentThread.ManagedThreadId, i);
                yield return i;
            }
        }

        [TestMethod]
        public void ParallelForEachWithStepsTest()
        {
            // Parallel takes locks on enumerator
            // If the work isn't that big, taking locks isn't worth it
            // Other way is to do these steps manually
            Parallel.ForEach(Iterate(0, 20, 3), i =>
              {
                  Debug.WriteLine("Loop: {0}", i);
              });
        }

        [TestMethod]
        public void ParallelForEachWithPartitionerTest()
        {
            // Partitioner.Create creates tuples of ranges from the values supplied
            Parallel.ForEach(Partitioner.Create(0, 200), range =>
             {
                 Debug.WriteLine("{0} - {1}", range.Item1, range.Item2);
             });
        }

        [TestMethod]
        public void DegreeOfParallelismTest()
        {
            Debug.WriteLine("Processor Count: {0}", Environment.ProcessorCount);
            int processorCount = Environment.ProcessorCount;

            var addrs = new[] {"10.120.177.84", "10.120.177.85", "10.120.177.86", "10.120.177.87",
            "10.120.177.88", "10.120.177.89", "10.120.177.90", "10.120.177.91", "10.120.177.92",
            "10.120.177.93", "10.120.177.94", "10.120.177.95", "10.120.177.96", "10.120.177.97",
            "10.120.177.98", "10.120.177.99", "10.120.177.100", "10.120.177.101", "10.120.177.102",
            "10.120.177.98", "10.120.177.99", "10.120.177.100", "10.120.177.101", "10.120.177.102"};

            Stopwatch clock = new Stopwatch();
            clock.Start();
            var pings = from addr in addrs.AsParallel()
                        select new Ping().Send(addr);
            clock.Stop();
            Debug.WriteLine("Time: {0}", clock.Elapsed);
            foreach(var ping in pings)
            {
                Debug.WriteLine("{0} : {1}", ping.Status, ping.Address);
            }

            clock.Reset();
            clock.Start();
            var pings2 = from addr in addrs.AsParallel().WithDegreeOfParallelism(20)
                        select new Ping().Send(addr);
            clock.Stop();
            Debug.WriteLine("Time: {0}", clock.Elapsed);
            foreach (var ping in pings2)
            {
                Debug.WriteLine("{0} : {1}", ping.Status, ping.Address);
            }
        }

        [TestMethod]
        public void FalseSharingTest()
        {
            Stopwatch clock = new Stopwatch();
            clock.Start();

            {
                // False Sharing of rand1 and rand2 (on the same cache line in memory)
                Random rand1 = new Random(), rand2 = new Random();
                int[] results1 = new int[20000000], results2 = new int[20000000];
                Parallel.Invoke(
                () =>
                {
                    for (int i = 0; i < results1.Length; i++)
                        results1[i] = rand1.Next();
                },
                () =>
                {
                    for (int i = 0; i < results2.Length; i++)
                        results2[i] = rand2.Next();
                });
            }

            clock.Stop();
            Debug.WriteLine("False Sharing time: {0}", clock.Elapsed);

            clock.Reset();
            clock.Start();
            {
                int[] results1, results2;
                Parallel.Invoke(
                () => {
                    Random rand1 = new Random();
                    results1 = new int[20000000];
                    for (int i = 0; i < results1.Length; i++)
                        results1[i] = rand1.Next();
                },
                () => {
                    Random rand2 = new Random();
                    results2 = new int[20000000];
                    for (int i = 0; i < results2.Length; i++)
                        results2[i] = rand2.Next();
                });
            }

            clock.Stop();
            Debug.WriteLine("Without False Sharing time: {0}", clock.Elapsed);
        }

        [TestMethod]
        public void ParallelForCompleteApiWithAggregationTest()
        {
            // We are aggregating the result of Pi here
            long NUM_STEPS = 100;
            double sum = 0.0;
            double step = 1.0 / (double)NUM_STEPS;
            object obj = new object();
            Parallel.For(0, NUM_STEPS,
            () => 0.0, // Initial - Gets called only once at first
            (i, state, partial) => // Body of the main method
            {
                Debug.WriteLine("Thread: {0} - Iteration: {1} - Partial: {2}", Thread.CurrentThread.ManagedThreadId, i, partial);
                double x = (i + 0.5) * step;
                return partial + 4.0 / (1.0 + x * x);
            },
            partial => // Local Finally runs at the end of each tasks iterations
            {
                Debug.WriteLine("Thread: {0} - Partial: {1}", Thread.CurrentThread.ManagedThreadId, partial);
                lock (obj) sum += partial;
            });

            Debug.WriteLine("PI: {0}", step * sum);
        }

        [TestMethod]
        public void PLinqAggregationsTest()
        {
            long NUM_STEPS = 100;
            double step = 1.0 / (double)NUM_STEPS;

            // Partitioner.Create to create partitions
            // AsParallel to parallelize it
            var pi = Partitioner.Create(0, NUM_STEPS).AsParallel().Select(range =>
            {
                double partial = 0.0;
                for (long i = range.Item1; i < range.Item2; i++)
                {
                    double x = (i + 0.5) * step;
                    partial += 4.0 / (1.0 + x * x);
                }
                return partial;
            }).Sum() * step; // Sum() functions sums over all iterations

            Debug.WriteLine("PI: {0}", pi);
        }

        [TestMethod]
        public void ThreadStaticTest()
        {
            var tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(() =>
            {
                Task.Delay(1000).Wait();
                Debug.WriteLine("Random Number: " + Utils.GetRandomNumber());
            }));

            tasks.Add(Task.Factory.StartNew(() =>
            {
                Task.Delay(1000).Wait();
                Debug.WriteLine("Random Number: " + Utils.GetRandomNumber());
            }));

            tasks.Add(Task.Factory.StartNew(() =>
            {
                Task.Delay(1000).Wait();
                Debug.WriteLine("Random Number: " + Utils.GetRandomNumber());
            }));

            Task.WaitAll(tasks.ToArray());
        }

        [TestMethod]
        public void ThreadLocalTest()
        {
            Utils test = new Utils();
            var tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("Random Number: " + test.GetThreadLocalRandomValue());
                Task.Delay(1000).Wait();
                Debug.WriteLine("Random Number: " + test.GetThreadLocalRandomValue());
            }));

            tasks.Add(Task.Factory.StartNew(() =>
            {
                Task.Delay(900).Wait();
                Debug.WriteLine("Random Number: " + test.GetThreadLocalRandomValue());
            }));

            tasks.Add(Task.Factory.StartNew(() =>
            {
                Task.Delay(800).Wait();
                Debug.WriteLine("Random Number: " + test.GetThreadLocalRandomValue());
            }));

            Task.WaitAll(tasks.ToArray());
        }


    }
}
