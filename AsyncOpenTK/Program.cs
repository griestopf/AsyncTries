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


        public override void Post(SendOrPostCallback d, object? state)
        {
            _allCallbacks.Enqueue(Tuple.Create(d, state));
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

            // should always be empty at this point, but nevertheless
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



    // Todo:
    // - Lösen:
    //   - LoadAssets wird zum neuen Init
    //   - AssetStorage lädt über SceneConverter "rekursiv" weitere Assets (fus-Datei Laden -> Konvertieren -> Textur Laden)

    public class SceneRenderer
    {

    }

    public class FuseeApp
    {
        protected string res = string.Empty;

        SceneRenderer? _renderer;


        protected async void LoadAssets()
        {
            GL.ClearColor(1, 0, 0, 1);

            string asset = await AssetStorage.GetCompoundAsync("someID");

            // TODO:
            // root.Add(asset);

            GL.ClearColor(0, 1, 0, 1);

            /*
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
            */
        }

        public void Init()
        {
            // TODO: 
            // Node root;
            // _renderer = new SceneRenderer(root);


            // Fire-and-forget call to LoadAssets!!! NO await!!!
            LoadAssets();
            // AsyncContext.Run(LoadAssets);
            Console.WriteLine("End Of Init");
        }

        public void RenderAFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (AssetStorage.AreAssetsPending)
            {
                // TODO:
                // ShowLoadingScreen();
                // return;
            }

            // TODO:
            // _renderer.Render();
        }

        public bool Stop { get; set; }
    }

    ////////////////////////////////////////////////////////////////////////
    //
    // Framework AssetLoader. Should hide await-calls and async creeping from calling instances

    public class AssetStorage
    {
        public static bool IsAssetPresent(string id)
        {
            return false; // TODO: Check if identified asset is not pending
        }

        public static bool AreAssetsPending => false; // TODO: Check if assets are pending


        public static async Task<string> GetStringAsync(string id)
        {
            string result;
            using (var stream = new AsyncStream(id))
            {
                result = await stream.GetContentsAsync();
            }
            return result;

        }

        public static async Task<string> GetCompoundAsync(string id)
        {
            string raw;
            using (var stream = new AsyncStream(id))
            {
                raw = await stream.GetContentsAsync();
            }
            string converted = await ConvertAsync(raw);
            // string converted = await ConvertAsync(result);
            return converted;
        }

        public static async Task<string> ConvertAsync(string raw)
        {
            string additionalResource = await GetStringAsync("extra");
            return raw + " " + additionalResource + " converted";
        }

        /*public async Task<string> ConvertAsync(string raw)
        {
            return await Task.Run(async () =>
            {
                Thread.Sleep(200);
                return raw + " asynchronously converted";
            });
        }*/
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
                    
                }
            
                disposedValue = true;
            }
        }
     
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
