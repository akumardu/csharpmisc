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
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Net;
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
                                                Task.Delay(50 + 10 * counter).Wait();
                                                counter++;
                                                return counter;
                                            })).ToList();

            var interleavedTasks = AsyncPatterns.InterleaveTasks(tasks);

            foreach (var task in interleavedTasks)
            {
                // Access each task as they complete
                int result = await task;
                Debug.WriteLine("Task Id: {0}", result);
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

        [TestMethod]
        public void ProducerConsumerWithBlockingCollectionTest()
        {
            string inputFilePath = @"D:\\tempInput.txt", outputFilePath = @"D:\\tempOutput.txt";
            try
            {
                File.Delete(inputFilePath);
                File.Delete(outputFilePath);
            }
            catch (Exception)
            { }

            for (int i = 1; i < 100000000; i *= 10)
            {
                File.AppendAllLines(inputFilePath, new string[] { "Merhaba + " + i + " nasilsin" });
            }

            AsyncPatterns.ProcessFile(inputFilePath, outputFilePath, (line) =>
            {
                Debug.WriteLine("{0}-{1}", Thread.CurrentThread.ManagedThreadId, line);
                Thread.Sleep(10 + 1000 / (line.Length - 18));
                return Regex.Replace(line, @"\s+", ", ");
            });
        }

        [TestMethod]
        public void MapReduceTest()
        {
            string dirPath = @"D:\";
            char[] delimiters = Enumerable.Range(0, 256)
                                .Select(i => (char)i)
                                .Where(c => Char.IsWhiteSpace(c) || Char.IsPunctuation(c))
                                .ToArray();
            var files = Directory.EnumerateFiles(dirPath, "*.txt").AsParallel();
            var counts = files.MapReduce(
                                path => File.ReadLines(path).SelectMany(line => line.Split(delimiters)),
                                word => word,
                                group => new[] { new KeyValuePair<string, int>(group.Key, group.Count()) });
            foreach (var kvp in counts)
            {
                Debug.WriteLine("Word: {0} Count: {1}", kvp.Key, kvp.Value);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WhenWhenAllOrFirstExceptionTest()
        {
            var tasksInput = new List<Task<string>>();
            tasksInput.Add(Task.Factory.StartNew(() =>
            {
                Task.Delay(10000).Wait();
                return "LongRunning";
            }, TaskCreationOptions.LongRunning));
            tasksInput.Add(Task.Factory.StartNew(() =>
            {
                Task.Delay(5000).Wait();
                return "ShortRunning";
            }));
            tasksInput.Add(Task.Factory.StartNew(() =>
            {
                Task.Delay(1000).Wait();
                int i = 1;
                if (i == (5 / 4))
                {
                    throw new ArgumentNullException("Thrown");
                }

                return "Could Be Exceptional";
            }));

            var taskResult = AsyncPatterns.WhenAllOrFirstException(tasksInput.AsEnumerable());

            await taskResult;

            Debug.WriteLine("IsFaulted: {0}, IsCompleted: {1}, IsCancelled: {2}",
                taskResult.IsFaulted, taskResult.IsCompleted, taskResult.IsCanceled);
            if (taskResult.IsCanceled)
            {
                Debug.WriteLine("Exception: {0}", taskResult.Exception);
            }
            else
            {
                foreach (var result in taskResult.Result)
                {
                    Debug.WriteLine("Result: " + result);
                }
            }
        }

        [TestMethod]
        public async Task RetryOnFaultTest()
        {
            int i = 0;
            string result = await AsyncPatterns.RetryOnFault<string>(
                () => Task.Factory.StartNew<string>(() =>
                {
                    i++;
                    Debug.WriteLine("Retry Value: {0}", i);
                    Task.Delay(1000).Wait();
                    if (i < 3)
                        throw new ArgumentException("Exceptions");
                    return string.Empty;
                }),
                5,
                () => {
                    Debug.WriteLine("Retrying ");
                    return Task.Delay(1000);
                }
                );
            Debug.WriteLine("Done");
        }

        private Task<string> ExecuteWebRequest(string url, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                if (ct.IsCancellationRequested)
                {
                    throw new OperationCanceledException(ct);
                }

                HttpWebRequest WebRequestObject = (HttpWebRequest)HttpWebRequest.Create(url);
                var response = WebRequestObject.GetResponse();
                string result = string.Empty;
                using (Stream WebStream = response.GetResponseStream())
                {
                    using (StreamReader Reader = new StreamReader(WebStream))
                    {

                        //
                        // Read data stream:
                        //
                        while (!Reader.EndOfStream)
                        {
                            result += Reader.ReadLine();
                        }
                    }
                }

                return result;
            });
        }

        [TestMethod]
        public async Task NeedOnlyOneTest()
        {
            string result = await AsyncPatterns.NeedOnlyOne<string>(
                ct => ExecuteWebRequest("https://www.google.com", ct),
                ct => ExecuteWebRequest("https://www.bing.com", ct),
                ct => ExecuteWebRequest("https://www.yahoo.com", ct)
                );
            Debug.WriteLine("Result: {0}", result);
        }

        [TestMethod]
        public async Task AsyncCacheTest()
        { 
            // Initialize the cache
           AsyncCache<string, string> m_webPages =
                        new AsyncCache<string, string>((url) => {
                            return Task.Factory.StartNew(() =>
                            {
                                Task.Delay(10 * url[0]);
                                return Guid.NewGuid().ToString();
                            });
                        });
            // Use the cache
            Stopwatch clock = new Stopwatch();
            clock.Start();
            string result = await m_webPages["https://www.google.com"];
            clock.Stop();
            Debug.WriteLine("Result: " + result + "Delay: " + clock.Elapsed);
            Debug.WriteLine(await m_webPages["abcd"]);

            clock.Reset();
            clock.Start();
            string result2 = await m_webPages["https://www.google.com"];
            Debug.WriteLine("Result: " + result2 + "Delay: " + clock.Elapsed);
        }
    }
}
