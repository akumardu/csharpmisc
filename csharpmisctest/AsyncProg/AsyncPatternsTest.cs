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

        [TestMethod]
        public void ProducerConsumerWithBlockingCollectionTest()
        {
            string inputFilePath = @"D:\\tempInput.txt", outputFilePath = @"D:\\tempOutput.txt";
            try
            {
                File.Delete(inputFilePath);
                File.Delete(outputFilePath);
            }
            catch(Exception)
            { }
            
            for (int i = 1; i < 100000000; i*=10)
            {
                File.AppendAllLines(inputFilePath, new string[] { "Merhaba + " + i + " nasilsin" });
            }

            AsyncPatterns.ProcessFile(inputFilePath, outputFilePath, (line) => 
            {
                Debug.WriteLine("{0}-{1}",Thread.CurrentThread.ManagedThreadId, line);
                Thread.Sleep(10 + 1000/(line.Length - 18));
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
            foreach(var kvp in counts)
            {
                Debug.WriteLine("Word: {0} Count: {1}", kvp.Key, kvp.Value);
            }
            
        }
    }
}
