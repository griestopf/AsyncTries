using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;

namespace AsyncOpenTK
{
    public class AsyncCtx : SynchronizationContext
    {
        private ConcurrentQueue<Tuple<SendOrPostCallback, object?>> _allCallbacks =
            new ConcurrentQueue<Tuple<SendOrPostCallback, object?>>();

        int _outstandingOperations = 0;

        /// <summary>
        /// Increments the outstanding asynchronous operation count.
        /// </summary>
        public void Started()
        {
            _ = Interlocked.Increment(ref _outstandingOperations);
        }

        /// <summary>
        /// Decrements the outstanding asynchronous operation count.
        /// </summary>
        public void Completed()
        {
            var newCount = Interlocked.Decrement(ref _outstandingOperations);
            //if (newCount == 0)
            //    _allCallbacks.
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            Started();
            //try
            //{
            _allCallbacks.Enqueue(Tuple.Create(d, state));
            //}
            //catch (InvalidOperationException)
            //{
            //    // vexing exception
            //    return;
            //}

        }

        public void ExecutePendingPostAwaits()
        {
            while (!_allCallbacks.IsEmpty)
            {
                _allCallbacks.TryDequeue(out var callback);

                if (callback != null)
                {
                    try
                    {
                        var d = callback.Item1;
                        var state = callback.Item2;
                        d(state);
                    }
                    catch (Exception exception)
                    {
                        ExceptionDispatchInfo.Capture(exception).Throw();
                    }
                }
            }

            // should always be empty
            _allCallbacks.Clear();
        }
    }

    class Program
    {
        static void Main()
        {
            var win = new MyGameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
            win.Run();
        }
    }


    public class MyGameWindow : GameWindow
    {
        protected FuseeApp fuseeApp;

        public AsyncCtx ctx;

        public MyGameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            fuseeApp = new FuseeApp();
            ctx = new AsyncCtx();
            SynchronizationContext.SetSynchronizationContext(ctx);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            fuseeApp.Init();

        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            ctx.ExecutePendingPostAwaits();
            base.OnRenderFrame(args);
            fuseeApp.RenderAFrame();
            SwapBuffers();

        }

        protected override void OnRenderThreadStarted()
        {
            base.OnRenderThreadStarted();
        }
    }


    public class FuseeApp
    {
        protected string res;

        protected async void LoadAssets()
        {
            GL.ClearColor(1, 0, 0, 1);

            Console.WriteLine($"About to load asset. In Thread: {Thread.CurrentThread.ManagedThreadId}");
            var asset = await AssetStorage.GetStringAsync("honz");

            // Do something with the asset right here
            Console.WriteLine($"Asset is present. In Thread: {Thread.CurrentThread.ManagedThreadId}");
            res = asset;
            Console.WriteLine(res);

            GL.ClearColor(1, 1, 0, 1);

            Console.WriteLine($"About to load asset 2. In Thread: {Thread.CurrentThread.ManagedThreadId}");
            asset = await AssetStorage.GetStringAsync("honz");

            // Do something with the asset right here
            Console.WriteLine($"Asset is present 2. In Thread: {Thread.CurrentThread.ManagedThreadId}");
            res = asset;
            Console.WriteLine(res);

            GL.ClearColor(0, 1, 0, 1);
        }

        public void Init()
        {
            // Fire-and-forget call to LoadAssets!!! NO await!!!
            LoadAssets();

            // AsyncContext.Run(LoadAssets);
            Console.WriteLine("End Of Init");
        }

        public void RenderAFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public bool Stop { get; set; }
    }

    ////////////////////////////////////////////////////////////////////////
    //
    // Framework AssetLoader. Should hide await-calls and async creeping from calling instances

    public class AssetStorage
    {
        public static async Task<string> GetStringAsync(string id)
        {
            string result;
            using (var stream = new AsyncStream(id))
            {
                result = await stream.GetContentsAsync();
            }
            return result;
        }
    }

    ////////////////////////////////////////////////////////////////////////
    //
    // Standard-Lib async file loader. Called by the AssetLoader

    public class AsyncStream : IDisposable
    {
        private bool disposedValue;

        public AsyncStream(string streamID)
        {

        }

        public async Task<string> GetContentsAsync()
        {

            return await Task.Run(async () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    await Task.Run(async () =>
                    {
                        Console.WriteLine("Just some text from within an async method ...");

                        await Task.Run(() => Console.WriteLine("Just some async text from within an async async method :)"));

                        Console.WriteLine("Just some text after some async text from within an async async method ...");
                    });

                    Thread.Sleep(200);
                    System.Console.WriteLine(".");

                    //if (i == 3) throw new Exception("Exception!");
                }

                return "This is the Stream Contents";
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                disposedValue = true;
            }
        }

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~AsyncStream()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
