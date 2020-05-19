using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncWinforms
{
    public partial class Form1 : Form
    {
        FuseeApp fuseeApp;

        public Form1()
        {
            fuseeApp = new FuseeApp();
            InitializeComponent();
            fuseeApp.Init();
        }

        public void OnIdle(object sender, EventArgs args)
        {
            fuseeApp.RenderAFrame();
        }

    }


   ////////////////////////////////////////////////////////////////////////
    //
    // Application with render loop. async/await ONLY in Asset loading code
    // Neither Run, Init nor RenderAFrame are async!

    public class FuseeApp
    {
        protected string res;

        protected async Task LoadAssets()
        {
            Console.WriteLine($"About to load asset. In Thread: {Thread.CurrentThread.ManagedThreadId}");
            var asset = await AssetLoader.GetStringAsync("honz");
            
            // Do something with the asset right here
            Console.WriteLine($"Asset is present. In Thread: {Thread.CurrentThread.ManagedThreadId}");
            res = asset;
        }

        public void Init()
        {
            // Fire-and-forget call to LoadAssets!!! NO await!!!
            LoadAssets();
            Console.WriteLine("End Of Init");
        }

        public void RenderAFrame()
        {
            
        }

        public bool Stop {get; set;}

        /*
        public void Run()
        {
            Init();

            while (!Stop)
                RenderAFrame();
        }
        */
    }

    ////////////////////////////////////////////////////////////////////////
    //
    // Framework AssetLoader. Should hide await-calls and async creeping from calling instances

    public class AssetLoader
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
