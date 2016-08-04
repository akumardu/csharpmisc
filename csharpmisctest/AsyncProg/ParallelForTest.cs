using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

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

        
    }
}
