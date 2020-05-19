using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Async
{

    ////////////////////////////////////////////////////////////////////////
    //
    // Start-Up code. No asyc/await in here.

    class Program
    {
        static void Main(string[] args)
        {
            new FuseeApp().Run();
            // AsyncContext.Run(new FuseeApp().Run);
        }



        //static async Task DoSomeAsyncStuff()
        //{
        //    Console.WriteLine($"Before Task - Current thread: {Thread.CurrentThread.ManagedThreadId}");
        //    await Task.Run(() => 
        //    {
        //        Console.WriteLine($"Inside Task - Current thread: {Thread.CurrentThread.ManagedThreadId}");
        //    });
        //    Console.WriteLine($"After Task - Current thread: {Thread.CurrentThread.ManagedThreadId}");
        //}
    }


    ////////////////////////////////////////////////////////////////////////
    //
    // Application with render loop. async/await ONLY in Asset loading code
    // Neither Run, Init nor RenderAFrame are async!


    // Todo:
    // - Lösen:
    //   - LoadAssets wird zum neuen Init
    //   - AssetStorage lädt über SceneConverter "rekursiv" weitere Assets (fus-Datei Laden -> Konvertieren -> Textur Laden)
    //   - SceneRenderer-Aufrufe nach await in LoadAssets: Könnte in anderem Thread laufen.
    // - SynchronizationContext implementieren oder Nito.AsyncEx.Context abwandeln, so dass in Dektop post-await-Code im
    //   Haupt-Thread aufgerufen wird.

    public class FuseeApp
    {
        protected string res;

        protected async Task LoadAssets()
        {
            Console.WriteLine($"About to load asset. In Thread: {Thread.CurrentThread.ManagedThreadId}");
            var asset = await AssetStorage.GetStringAsync("honz");
            
            // Do something with the asset right here
            Console.WriteLine($"Asset is present. In Thread: {Thread.CurrentThread.ManagedThreadId}");
            res = asset;
        }

        protected void Init()
        {
            // Fire-and-forget call to LoadAssets!!! NO await!!!
            LoadAssets();
            // AsyncContext.Run(LoadAssets);
            Console.WriteLine("End Of Init");
        }

        protected void RenderAFrame()
        {
       
        }

        public bool Stop {get; set;}

        public void Run()
        {
            Init();

            while (!Stop)
            {
                // PerformPendingPostAwaits();
                RenderAFrame();
            }
        }
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
            return await Task<string>.Run(() => 
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(200);
                    System.Console.WriteLine(".");
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