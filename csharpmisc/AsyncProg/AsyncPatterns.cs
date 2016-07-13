using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.AsyncProg
{
    using System.Net.Sockets;
    using System.Threading;

    public class AsyncPatterns
    {
        // Interleaved: Potential performance problem with Task.WhenAny 
        // to support interleaving scenario with a very large number of tasks. 
        // Ever call to WhenAny will result in a continuation being registered 
        // with each task => O(N^2) complexity.
        static IEnumerable<Task<T>> Interleaved<T>(IEnumerable<Task<T>> tasks)
        {
            var inputTasks = tasks.ToList();
            // Creates inputTasks.Count number of TaskCompletionSources
            var sources = (from _ in Enumerable.Range(0, inputTasks.Count)
                           select new TaskCompletionSource<T>()).ToList(); 
            int nextTaskIndex = -1;
            foreach (var inputTask in inputTasks)
            {
                inputTask.ContinueWith(completed =>
                {
                    var source = sources[Interlocked.Increment(ref nextTaskIndex)];
                    if (completed.IsFaulted)
                        source.TrySetException(completed.Exception.InnerExceptions);
                    else if (completed.IsCanceled)
                        source.TrySetCanceled();
                    else
                        source.TrySetResult(completed.Result);
                }, CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            }

            return from source in sources
                   select source.Task;
        }

        static async Task UseInterleaved()
        {
            int counter = 0;
            IEnumerable<Task<int>> tasks = (from _ in Enumerable.Range(0, 100)
                                            select Task.Factory.StartNew<int>(() =>
                                            {
                                                Task.Delay(500).Wait();
                                                counter++;
                                                return counter;
                                            })).ToList();

            var interleavedTasks = Interleaved(tasks);

            foreach (var task in interleavedTasks)
            {
                int result = await task;
            }
        }


    }
}