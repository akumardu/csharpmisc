using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csharpmisc.AsyncProg
{
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;

    public static class AsyncPatterns
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

        // Creating static Completed Tasks
        public static void CompletedTask()
        {
            // Initially this is how you did it
            Task completedTask = ((Func<Task>)(() =>
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.SetResult(null);
                return tcs.Task;
            }))();

            // Then came .net4.5
            Task completedTask2 = Task.FromResult(false);

            // With .net4.6
            Task completedTask3 = Task.CompletedTask;
        }


        // Producer-Consumer pattern using BlockingCollection
        // supports throttling both consumers and producers, timeouts on waits, 
        // support for arbitrary underlying data structures, and more.
        // It also provides built-in implementations of typical coding patterns related
        // to producer/consumer in order to make such patterns simple to utilize.
        // Example: consider the need to read in a file, transform each line
        // using a regular expression, and write out the transformed line to a new file.
        public static void ProcessFile(string inputPath, string outputPath, Func<string, string> actionBody)
        {
            // Limiting capacity to throttle read/write
            // Blocking collection defaults to using a queue, using ConcurrentBag here
            var inputLines = new BlockingCollection<string>(new ConcurrentBag<string>(), boundedCapacity: 2);
            var processedLines = new BlockingCollection<string>();

            // Stage #1
            var readLines = Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var line in File.ReadLines(inputPath)) inputLines.Add(line);
                }
                finally { inputLines.CompleteAdding(); }
            });

            // Stage #2
            var processLines = Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var line in inputLines.GetConsumingEnumerable()
                    .AsParallel() // To ensure processing is parallelized
                    .AsOrdered() // To ensure output order is maintained
                    .Select(line => actionBody(line))) //Regex.Replace(line, @"\s+", ", ")))
                    {
                        processedLines.Add(line);
                    }
                }
                finally { processedLines.CompleteAdding(); }
            });

            // Stage #3
            var writeLines = Task.Factory.StartNew(() =>
            {
                File.WriteAllLines(outputPath, processedLines.GetConsumingEnumerable());
            });
            Task.WaitAll(readLines, processLines, writeLines);
        }


        // Map-Reduce - ParallelQuery to support parallelism
        public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(
                                                        this ParallelQuery<TSource> source,
                                                        Func<TSource, IEnumerable<TMapped>> map,
                                                        Func<TMapped, TKey> keySelector,
                                                        Func<IGrouping<TKey, TMapped>, IEnumerable<TResult>> reduce)
        {
            return source.SelectMany(map)
            .GroupBy(keySelector)
            .SelectMany(reduce);
        }
    }
}