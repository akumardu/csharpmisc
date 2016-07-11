using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using csharpmisc.AsyncProg;

namespace csharpmisctest.AsyncProg
{
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
            int value = 1;
            try
            {
                firstTask = Task.Factory.StartNew((cToken) =>
                {
                    value = 2;
                    Debug.WriteLine("First task called");
                    var cancellationToken = (CancellationToken)cToken;
                    if (cancellationToken.IsCancellationRequested)
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
            catch (Exception)
            {

            }
        }
    }
}
