using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using csharpmisc.AsyncProg;

namespace csharpmisctest.AsyncProg
{
    using System.Collections.Generic;

    [TestClass]
    public class TapTest
    {
        [TestMethod]
        public async Task TestTaskContinuationOption()
        {
            int value = 1;
            Task firstTask = null;
            try
            {


                firstTask = Task.Factory.StartNew(() => { value = 2; Debug.WriteLine("First Task Called"); throw new ArgumentException("abcd"); });
                var secondTask = firstTask.ContinueWith((fTask) => { value = 3; Debug.WriteLine("Second Task Called"); });
                await secondTask;
            }
            catch(Exception)
            {

            }
            
            // Second task will run
            Assert.AreEqual(value, 3);
            Assert.AreEqual(firstTask.IsCompleted, true);

            try
            {


                firstTask = Task.Factory.StartNew(() => { value = 4; Debug.WriteLine("First Task Called"); throw new ArgumentException("abcd"); });
                var secondTask = firstTask.ContinueWith((fTask) => { value = 5; Debug.WriteLine("Second Task Called"); }, TaskContinuationOptions.NotOnFaulted);
                await secondTask;
            }
            catch (Exception)
            {

            }

            Assert.AreEqual(value, 4);
        }

        [TestMethod]
        public async Task TestCancellationToken()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var cancelTask = new Action(() => { cts.Cancel(); });

            Task firstTask = null;
            int value = 1;
            try
            {
                firstTask = Task.Factory.StartNew((cToken) =>
                {
                    value = 2;
                    Debug.WriteLine("First task called");
                    var cancellationToken = (CancellationToken)cToken;
                    if(cancellationToken.IsCancellationRequested)
                    {
                        // same as throw new OperationCancelledException(cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        value = 3;
                    }
                }, cts.Token, cts.Token);

                await firstTask;

            }
            catch(Exception)
            {

            }

            // Cancellation wasn't actually requested;
            Assert.AreEqual(value, 3);
            Assert.AreEqual(firstTask.IsCanceled, false);

            try
            {
                firstTask = Task.Factory.StartNew((cToken) =>
                {
                    cancelTask.Invoke();
                    value = 4;
                    Debug.WriteLine("Second task called");
                    var cancellationToken = (CancellationToken)cToken;
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // same as throw new OperationCancelledException(cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        value = 5;
                    }
                }, cts.Token, cts.Token);

                await firstTask;
            }
            catch (Exception)
            {

            }

            // Cancellation was actually requested;
            Assert.AreEqual(value, 4);
            Assert.AreEqual(firstTask.IsCanceled, true);

        }

        [TestMethod]
        public void TestCancellationTokenRegisterMethod()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var cancelTask = new Action(() => { cts.Cancel(); });
            int value = 1;
            cts.Token.Register(new Action(() => 
            {
                value = 2;
                Debug.WriteLine("Registered cancellation method called");
            }));

            cancelTask.Invoke();

            Assert.AreEqual(value, 2);
        }

        [TestMethod]
        public async Task TestProgressMethod()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var progressReporter = new ProgressString();
            Task firstTask = null;
            try
            {
                firstTask = Task.Factory.StartNew(() =>
                {
                   progressReporter.Report("4");  
                }, cts.Token);

                await firstTask;
            }
            catch (Exception)
            {

            }

            Assert.AreEqual(progressReporter.GetProgressValue(),"4");
        }

        [TestMethod]
        public async Task TestNeedOnlyOneDesign()
        {
            var cts = new CancellationTokenSource();
            var tasks = new List<Task<int>>();
            int value = 1;
            tasks.Add(Task.Factory.StartNew<int>((cToken) =>
            {
                Task.Delay(50000, (CancellationToken)cToken).Wait((CancellationToken)cToken);
                if (((CancellationToken)cToken).IsCancellationRequested)
                {
                    throw new OperationCanceledException((CancellationToken)cToken);
                }
                value = 2;
                return value;
            }, cts.Token, cts.Token));

            tasks.Add(Task.Factory.StartNew<int>((cToken) =>
            {
                Task.Delay(50000, (CancellationToken)cToken).Wait((CancellationToken)cToken);
                if (((CancellationToken)cToken).IsCancellationRequested)
                {
                    throw new OperationCanceledException((CancellationToken)cToken);
                }
                value = 3;
                return value;
            }, cts.Token, cts.Token));

            tasks.Add(Task.Factory.StartNew<int>((cToken) =>
            {
                Task.Delay(50000, (CancellationToken)cToken).Wait((CancellationToken)cToken);
                if (((CancellationToken)cToken).IsCancellationRequested)
                {
                    throw new OperationCanceledException((CancellationToken)cToken);
                }
                value = 4;
                return value;
            }, cts.Token, cts.Token));

            tasks.Add(Task.Factory.StartNew<int>((cToken) =>
            {
                value = 5;
                if (((CancellationToken)cToken).IsCancellationRequested)
                {
                    throw new OperationCanceledException((CancellationToken)cToken);
                }
                return value;
            }, cts.Token, cts.Token));

            var completed = await Task.WhenAny(tasks).ConfigureAwait(false);

            Assert.AreEqual(completed.Result, 5);

            cts.Cancel();

            foreach (var task in tasks)
            {
                var ignored = task.ContinueWith(
                    t =>
                    {
                        Debug.WriteLine(t);

                    }, TaskContinuationOptions.OnlyOnFaulted
                    );
            }
        }
    }
}
