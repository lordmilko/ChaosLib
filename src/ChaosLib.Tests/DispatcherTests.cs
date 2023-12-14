using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiDispatcher = System.Windows.Threading.Dispatcher;
using UiDispatcherOperationStatus = System.Windows.Threading.DispatcherOperationStatus;

namespace ChaosLib.Tests
{
    class IgnoredException : Exception
    {
        public IgnoredException(string message) : base(message)
        {
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class DispatcherTests
    {
        private static UiDispatcher uiDispatcher;

        private static UiDispatcher UiDispatcher
        {
            get
            {
                if (uiDispatcher == null)
                {
                    var wait = new ManualResetEventSlim(false);

                    var thread = new Thread(() =>
                    {
                        var app = new Application();

                        app.Startup += (s, e) =>
                        {
                            wait.Set();
                        };

                        var window = new Window
                        {
                            WindowState = WindowState.Minimized
                        };

                        app.Run(window);
                    });
                    thread.SetApartmentState(ApartmentState.STA);

                    thread.Start();

                    wait.Wait();

                    uiDispatcher = Application.Current.Dispatcher;
                }

                return uiDispatcher;
            }
        }

        #region Invoke Action

        [TestMethod]
        public void Dispatcher_Invoke_Action_NoResult()
        {
            using var thread = CreateThread();

            Action action = () => Thread.Sleep(100);

            UiDispatcher.Invoke(action);
            thread.Dispatcher.Invoke(action);
        }

        [TestMethod]
        public void Dispatcher_Invoke_Action_WithException()
        {
            using var thread = CreateThread();

            Action action = () => throw new IgnoredException("Test");

            Assert.ThrowsException<IgnoredException>(
                () => UiDispatcher.Invoke(action),
                "Test"
            );

            Assert.ThrowsException<IgnoredException>(
                () => thread.Dispatcher.Invoke(action),
                "Test"
            );
        }

        [TestMethod]
        public void Dispatcher_Invoke_Action_InsideDispatcher()
        {
            using var thread = CreateThread();

            UiDispatcher.Invoke(() =>
            {
                UiDispatcher.Invoke(() => Thread.Sleep(100));
            });

            thread.Dispatcher.Invoke(() =>
            {
                thread.Dispatcher.Invoke(() => Thread.Sleep(100));
            });
        }

        [TestMethod]
        public void Dispatcher_Invoke_Action_CancellationToken_AlreadyStarted()
        {
            using var thread = CreateThread();

            var cts = new CancellationTokenSource();

            UiDispatcher.Invoke(() =>
            {
                cts.Cancel();
            }, DispatcherPriority.Normal, cts.Token);

            cts = new CancellationTokenSource();

            thread.Dispatcher.Invoke(() =>
            {
                cts.Cancel();
            }, cts.Token);
        }

        [TestMethod]
        public void Dispatcher_Invoke_Action_CancellationToken_NotStarted()
        {
            using var dt = CreateThread();

            var cts = new CancellationTokenSource();

            UiDispatcher.Invoke(() =>
            {
                var wait = new ManualResetEventSlim(false);

                var thread = new Thread(() =>
                {
                    try
                    {
                        UiDispatcher.Invoke(() => Thread.Sleep(100), DispatcherPriority.Normal, cts.Token);
                    }
                    catch
                    {
                        wait.Set();
                    }
                });
                thread.Start();

                Thread.Sleep(100);

                cts.Cancel();

                wait.Wait();
            });

            cts = new CancellationTokenSource();

            dt.Dispatcher.Invoke(() =>
            {
                var wait = new ManualResetEventSlim(false);

                var thread = new Thread(() =>
                {
                    try
                    {
                        dt.Dispatcher.Invoke(() => Thread.Sleep(100), cts.Token);
                    }
                    catch
                    {
                        wait.Set();
                    }
                });
                thread.Start();

                Thread.Sleep(100);

                cts.Cancel();

                wait.Wait();
            });
        }

        #endregion
        #region Invoke Func

        [TestMethod]
        public void Dispatcher_Invoke_Func_WithResult()
        {
            using var thread = CreateThread();

            Func<string> func = () => "hi";

            Assert.AreEqual("hi", UiDispatcher.Invoke(func));
            Assert.AreEqual("hi", thread.Dispatcher.Invoke(func));
        }

        [TestMethod]
        public void Dispatcher_Invoke_Func_WithException()
        {
            using var thread = CreateThread();

            Func<string> func = () => throw new IgnoredException("Test");

            Assert.ThrowsException<IgnoredException>(
                () => UiDispatcher.Invoke(func),
                "Test"
            );

            Assert.ThrowsException<IgnoredException>(
                () => thread.Dispatcher.Invoke(func),
                "Test"
            );
        }

        [TestMethod]
        public void Dispatcher_Invoke_Func_InsideDispatcher()
        {
            using var thread = CreateThread();

            var result1 = UiDispatcher.Invoke(() =>
            {
                return UiDispatcher.Invoke(() => "hi");
            });

            Assert.AreEqual("hi", result1);

            var result2 = thread.Dispatcher.Invoke(() =>
            {
                return thread.Dispatcher.Invoke(() => "hi");
            });

            Assert.AreEqual("hi", result2);
        }

        [TestMethod]
        public void Dispatcher_Invoke_Func_CancellationToken_AlreadyStarted()
        {
            using var thread = CreateThread();

            var cts = new CancellationTokenSource();

            var result1 = UiDispatcher.Invoke(() =>
            {
                cts.Cancel();
                return "hi";
            }, DispatcherPriority.Normal, cts.Token);

            cts = new CancellationTokenSource();

            var result2 = thread.Dispatcher.Invoke(() =>
            {
                cts.Cancel();
                return "hi";
            }, cts.Token);

            Assert.AreEqual("hi", result1);
            Assert.AreEqual("hi", result2);
        }

        [TestMethod]
        public void Dispatcher_Invoke_Func_CancellationToken_NotStarted()
        {
            using var dt = CreateThread();

            var cts = new CancellationTokenSource();

            var result1 = UiDispatcher.Invoke(() =>
            {
                var wait = new ManualResetEventSlim(false);

                var thread = new Thread(() =>
                {
                    try
                    {
                        UiDispatcher.Invoke(() =>
                        {
                            Thread.Sleep(100);
                            return "bye";
                        }, DispatcherPriority.Normal, cts.Token);
                    }
                    catch
                    {
                        wait.Set();
                    }
                });
                thread.Start();

                Thread.Sleep(100);

                cts.Cancel();

                wait.Wait();

                return "hi";
            });

            cts = new CancellationTokenSource();

            var result2 = dt.Dispatcher.Invoke(() =>
            {
                var wait = new ManualResetEventSlim(false);

                var thread = new Thread(() =>
                {
                    try
                    {
                        dt.Dispatcher.Invoke(() =>
                        {
                            Thread.Sleep(100);
                            return "bye";
                        }, cts.Token);
                    }
                    catch
                    {
                        wait.Set();
                    }
                });
                thread.Start();

                Thread.Sleep(100);

                cts.Cancel();

                wait.Wait();

                return "hi";
            });

            Assert.AreEqual("hi", result1);
            Assert.AreEqual("hi", result2);
        }

        #endregion
        #region Invoke Action Async

        [TestMethod]
        public async Task Dispatcher_InvokeAsync_Action_NoResult()
        {
            using var thread = CreateThread();

            Action action = () => Thread.Sleep(100);

            await UiDispatcher.InvokeAsync(action);
            await thread.Dispatcher.InvokeAsync(action);
        }

        [TestMethod]
        public async Task Dispatcher_InvokeAsync_Action_WithException()
        {
            using var thread = CreateThread();

            Action action = () => throw new IgnoredException("Test");

            await Assert.ThrowsExceptionAsync<IgnoredException>(
                async () => await UiDispatcher.InvokeAsync(action),
                "Test"
            );

            await Assert.ThrowsExceptionAsync<IgnoredException>(
                async () => await thread.Dispatcher.InvokeAsync(action),
                "Test"
            );
        }

        [TestMethod]
        public async Task Dispatcher_InvokeAsync_Action_InsideDispatcher()
        {
            using var thread = CreateThread();

            await UiDispatcher.InvokeAsync(() =>
            {
                UiDispatcher.InvokeAsync(() => Thread.Sleep(100)).Wait();
            });

            await thread.Dispatcher.InvokeAsync(() =>
            {
                thread.Dispatcher.InvokeAsync(() => Thread.Sleep(100)).Wait();
            });
        }

        [TestMethod]
        public async Task Dispatcher_InvokeAsync_Action_CancellationToken_AlreadyStarted()
        {
            using var thread = CreateThread();

            var cts = new CancellationTokenSource();

            var preWait = new ManualResetEventSlim(false);
            var postWait = new ManualResetEventSlim(false);

            Action action = () =>
            {
                preWait.Set();
                postWait.Wait();
            };

            //UI

            var result1 = UiDispatcher.InvokeAsync(action, DispatcherPriority.Normal, cts.Token);

            preWait.Wait();
            cts.Cancel();
            postWait.Set();

            await result1;

            Assert.AreEqual(UiDispatcherOperationStatus.Completed, result1.Status);

            //Custom

            preWait.Reset();
            postWait.Reset();

            cts = new CancellationTokenSource();

            var result2 = thread.Dispatcher.InvokeAsync(action, cts.Token);

            preWait.Wait();
            cts.Cancel();
            postWait.Set();

            await result2;

            Assert.AreEqual(DispatcherOperationStatus.Completed, result2.Status);
        }

        [TestMethod]
        public void Dispatcher_InvokeAsync_Action_CancellationToken_NotStarted()
        {
            using var thread = CreateThread();

            var cts = new CancellationTokenSource();

            var preWait = new ManualResetEventSlim(false);
            var postWait = new ManualResetEventSlim(false);

            Action action = () =>
            {
                preWait.Set();
                postWait.Wait();
            };

            //UI

            UiDispatcher.InvokeAsync(action);

            preWait.Wait();
            var uiOp = UiDispatcher.InvokeAsync(() => Thread.Sleep(100), DispatcherPriority.Normal, cts.Token);
            cts.Cancel();
            postWait.Set();

            Assert.AreEqual(UiDispatcherOperationStatus.Aborted, uiOp.Status);

            //Custom

            preWait.Reset();
            postWait.Reset();

            cts = new CancellationTokenSource();

            thread.Dispatcher.InvokeAsync(action);

            preWait.Wait();
            var customOp = thread.Dispatcher.InvokeAsync(() => Thread.Sleep(100), cts.Token);
            cts.Cancel();
            postWait.Set();

            Assert.AreEqual(DispatcherOperationStatus.Aborted, customOp.Status);
        }

        #endregion
        #region Invoke Func Async

        [TestMethod]
        public async Task Dispatcher_InvokeAsync_Func_WithResult()
        {
            using var thread = CreateThread();

            Func<string> func = () => "hi";

            Assert.AreEqual("hi", await UiDispatcher.InvokeAsync(func));
            Assert.AreEqual("hi", await thread.Dispatcher.InvokeAsync(func));
        }

        [TestMethod]
        public async Task Dispatcher_InvokeAsync_Func_WithException()
        {
            using var thread = CreateThread();

            Func<string> func = () => throw new IgnoredException("Test");

            await Assert.ThrowsExceptionAsync<IgnoredException>(
                async () => await UiDispatcher.InvokeAsync(func),
                "Test"
            );

            await Assert.ThrowsExceptionAsync<IgnoredException>(
                async () => await thread.Dispatcher.InvokeAsync(func),
                "Test"
            );
        }

        [TestMethod]
        public async Task Dispatcher_InvokeAsync_Func_InsideDispatcher()
        {
            using var thread = CreateThread();

            var result1 = await UiDispatcher.InvokeAsync(() =>
            {
                var op = UiDispatcher.InvokeAsync(() => "hi");
                op.Wait();
                return op.Result;
            });

            var result2 = await thread.Dispatcher.InvokeAsync(() =>
            {
                var op = thread.Dispatcher.InvokeAsync(() => "hi");
                op.Wait();
                return op.Result;
            });

            Assert.AreEqual("hi", result1);
            Assert.AreEqual("hi", result2);
        }

        [TestMethod]
        public async Task Dispatcher_InvokeAsync_Func_CancellationToken_AlreadyStarted()
        {
            using var thread = CreateThread();

            var cts = new CancellationTokenSource();

            var preWait = new ManualResetEventSlim(false);
            var postWait = new ManualResetEventSlim(false);

            Func<string> action = () =>
            {
                preWait.Set();
                postWait.Wait();
                return "hi";
            };

            //UI

            var result1 = UiDispatcher.InvokeAsync(action, DispatcherPriority.Normal, cts.Token);

            preWait.Wait();
            cts.Cancel();
            postWait.Set();

            await result1;

            Assert.AreEqual(UiDispatcherOperationStatus.Completed, result1.Status);

            //Custom

            preWait.Reset();
            postWait.Reset();

            cts = new CancellationTokenSource();

            var result2 = thread.Dispatcher.InvokeAsync(action, cts.Token);

            preWait.Wait();
            cts.Cancel();
            postWait.Set();

            await result2;

            Assert.AreEqual(DispatcherOperationStatus.Completed, result2.Status);
        }

        [TestMethod]
        public void Dispatcher_InvokeAsync_Func_CancellationToken_NotStarted()
        {
            using var thread = CreateThread();

            var cts = new CancellationTokenSource();

            var preWait = new ManualResetEventSlim(false);
            var postWait = new ManualResetEventSlim(false);

            Func<string> action = () =>
            {
                preWait.Set();
                postWait.Wait();
                return "hi";
            };

            //UI

            UiDispatcher.InvokeAsync(action);

            preWait.Wait();
            var uiOp = UiDispatcher.InvokeAsync(() => Thread.Sleep(100), DispatcherPriority.Normal, cts.Token);
            cts.Cancel();
            postWait.Set();

            Assert.AreEqual(UiDispatcherOperationStatus.Aborted, uiOp.Status);

            //Custom

            preWait.Reset();
            postWait.Reset();

            cts = new CancellationTokenSource();

            thread.Dispatcher.InvokeAsync(action);

            preWait.Wait();
            var customOp = thread.Dispatcher.InvokeAsync(() => Thread.Sleep(100), cts.Token);
            cts.Cancel();
            postWait.Set();

            Assert.AreEqual(DispatcherOperationStatus.Aborted, customOp.Status);
        }

        #endregion

        [TestMethod]
        public void Dispatcher_InvokeShutdown_QueueEmpty()
        {
            using var thread = CreateThread();

            UiDispatcher.InvokeShutdown();
            thread.Dispatcher.InvokeShutdown();

            try
            {
                Assert.IsTrue(UiDispatcher.HasShutdownStarted);
                Assert.IsFalse(UiDispatcher.HasShutdownFinished); //The UI thread hasn't actually stopped yet

                Assert.IsTrue(thread.Dispatcher.HasShutdownStarted);
                Assert.IsTrue(thread.Dispatcher.HasShutdownFinished); //No tasks running, we want to shutdown immediately
            }
            finally
            {
                RestartApplication();
            }
        }

        [TestMethod]
        public void Dispatcher_BeginInvokeShutdown_QueueEmpty()
        {
            using var thread = CreateThread();

            try
            {
                UiDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                thread.Dispatcher.BeginInvokeShutdown();

                Thread.Sleep(100);

                //For some reason the UiDispatcher's shutdown hasn't started yet
                Assert.IsTrue(thread.Dispatcher.HasShutdownStarted);
            }
            finally
            {
                RestartApplication();
            }
        }

        [TestMethod]
        public async Task Dispatcher_InvokeShutdown_ItemRunning()
        {
            using var thread = CreateThread();

            var startWait = new ManualResetEventSlim(false);
            var endWait = new ManualResetEventSlim(false);

            var op = thread.Dispatcher.InvokeAsync(() =>
            {
                thread.Dispatcher.InvokeShutdown();

                startWait.Set();
                endWait.Wait();
            });

            startWait.Wait();

            Assert.IsTrue(thread.Dispatcher.HasShutdownStarted);
            Assert.IsFalse(thread.Dispatcher.HasShutdownFinished);

            endWait.Set();

            await op;

            Assert.IsTrue(thread.Dispatcher.HasShutdownStarted);
            Assert.IsTrue(thread.Dispatcher.HasShutdownFinished);
        }

        [TestMethod]
        public void Dispatcher_Abort_RunningItem()
        {
            using var thread = CreateThread();

            //UI

            var preWait = new ManualResetEventSlim(false);
            var postWait = new ManualResetEventSlim(false);

            Action action = () =>
            {
                preWait.Set();
                postWait.Wait();
            };

            var uiOp = UiDispatcher.InvokeAsync(action);

            preWait.Wait();
            uiOp.Abort();
            postWait.Set();
            uiOp.Wait();

            Assert.AreEqual(UiDispatcherOperationStatus.Completed, uiOp.Status);

            preWait.Reset();
            postWait.Reset();

            //Custom

            var normalOp = thread.Dispatcher.InvokeAsync(action);

            preWait.Wait();
            normalOp.Abort();
            postWait.Set();
            normalOp.Wait();

            Assert.AreEqual(DispatcherOperationStatus.Completed, normalOp.Status);
        }

        [TestMethod]
        public async Task Dispatcher_Abort_FinishedItem()
        {
            using var thread = CreateThread();

            Action action = () => Thread.Sleep(100);

            var uiOp = UiDispatcher.InvokeAsync(action);
            await uiOp;
            uiOp.Abort();
            Assert.AreEqual(UiDispatcherOperationStatus.Completed, uiOp.Status);

            var normalOp = thread.Dispatcher.InvokeAsync(action);
            await normalOp;
            normalOp.Abort();

            Assert.AreEqual(DispatcherOperationStatus.Completed, normalOp.Status);
        }

        [TestMethod]
        public async Task Dispatcher_Abort_QueuedItem()
        {
            using var thread = CreateThread();

            var wait = new ManualResetEventSlim(false);

            var uiOp1 = UiDispatcher.InvokeAsync(() => wait.Wait());
            var uiOp2 = UiDispatcher.InvokeAsync(() => wait.Wait());

            uiOp2.Abort();
            Assert.AreEqual(UiDispatcherOperationStatus.Aborted, uiOp2.Status);

            wait.Set();
            await uiOp1;
        }

        private DispatcherThread CreateThread([CallerMemberName] string threadName = null)
        {
            var thread = new DispatcherThread(threadName);
            thread.Start();
            return thread;
        }

        private void RestartApplication()
        {
            //If we're calling this method, the UI thread has imploded due an unhandled exception. There's no hope to do anything, so just try and
            //restart from scratch
            uiDispatcher = null;

            //Clear the internal field that prevents creating multiple Application instances in the same AppDomain
            typeof(Application).GetField("_appCreatedInThisAppDomain", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, false);

            //Created a new UiDispatcher and verify it's working
            UiDispatcher.Invoke(() => Debug.WriteLine("Restarted UiDispatcher due to an unhandled exception"));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }
    }
}
