using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csharpmisctest.AsyncProg
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using csharpmisc.AsyncProg;
    using csharpmisc.Misc;

    [TestClass]
    public class AsyncPatternsTest
    {
        [TestMethod]
        public async Task TestInterleaved()
        {
            int counter = 0;

            // Create some sample tasks
            IEnumerable<Task<int>> tasks = (from _ in Enumerable.Range(0, 100)
                                            select Task.Factory.StartNew<int>(() =>
                                            {
                                                Task.Delay(50 + 10*counter).Wait();
                                                counter++;
                                                return counter;
                                            })).ToList();

            var interleavedTasks = AsyncPatterns.InterleaveTasks(tasks);

            foreach (var task in interleavedTasks)
            {
                // Access each task as they complete
                int result = await task;
                Debug.WriteLine("Task Id: {0}",result);
            }
        }

        

        [TestMethod]
        public void ParallelForEachWithConsumingEnumerableTest()
        {
            // All the MoveNext calls should be made on the same thread
            var enumerable = new MyEnumerable(20);
            AsyncPatterns.ForEachWithEnumerationOnMainThread<int>(enumerable, (i) =>
            {
                Debug.WriteLine("Loop: {0}", i);
            });
        }
    }
}
