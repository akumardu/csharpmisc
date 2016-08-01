using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

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
    }
}
