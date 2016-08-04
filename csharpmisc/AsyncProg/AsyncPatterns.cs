using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csharpmisc.AsyncProg
{
    using System.Collections.Concurrent;
    using System.Threading;

    public class AsyncPatterns
    {
        // Interleaved: Potential performance problem with Task.WhenAny 
        // to support interleaving scenario with a very large number of tasks. 
        // Ever call to WhenAny will result in a continuation being registered 
        // with each task => O(N^2) complexity.
        public static IEnumerable<Task<T>> InterleaveTasks<T>(IEnumerable<Task<T>> tasks)
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

        // The consuming implementation needs to change to ensure that 
        // the data source is only accessed by
        // the thread making the call to the loop.
        // That can be achieved with a producer/consumer pattern
        // An example of this could be if MoveNext were accessing a user interface (UI)
        // control in Windows Forms or Windows Presentation Foundation in order to 
        // retrieve its data, or if the control were
        // pulling data from the object model of one of the Microsoft Office applications.
        public static void ForEachWithEnumerationOnMainThread<T>(IEnumerable<T> source, Action<T> body)
        {
            var collectedData = new BlockingCollection<T>();
            var loop = Task.Factory.StartNew(() =>
                Parallel.ForEach(collectedData.GetConsumingEnumerable(), body));
            try
            {
                foreach (var item in source) collectedData.Add(item);
            }
            finally
            {
                collectedData.CompleteAdding();
            }

            loop.Wait();
        }
    }
}