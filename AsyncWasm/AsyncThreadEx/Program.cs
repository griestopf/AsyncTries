//using Nito.AsyncEx;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebAssembly;

namespace AsyncThreadsEx
{
    public class FileLoader<T>
    {
        public EventHandler FileLoaded;

        private bool _isLoaded = false;
        private T _data;
        private delegate void Loaded(T data);

        public FileLoader(string filename)
        {
            // this chunk does NOT work inside Thread.Run() because Runtime is injected from another thread
            var window = (JSObject)Runtime.GetGlobalObject("Loader");
            Loaded l = SetFileLoaded;
            window.Invoke("LoadFileAsync", filename, l);
        }

        public T GetData()
        {
            // This also blocks the javascript part, so it gets stuck in this loop
            // without actually loading the file
            //for(;;)
            //{
            //    Thread.Sleep(1000);
            //    if (_isLoaded) break;
            //}
            

            if (_isLoaded)
                return _data;

            throw new NullReferenceException("Data not loaded yet, please use EventHandler to check for completeness");
        }

        private void SetFileLoaded(T data)
        {
            _isLoaded = true;
            _data = data;
            FileLoaded(this, new EventArgs());
        }
    }

    public class Program
    {
        private delegate void Loaded(string data);


        static async Task LoadAssets()
        {
            var baseAddress = "http://localhost:8000/";
            var id = "someText.txt";

            using var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            //#if DEBUG
            Console.WriteLine($"Requesting '{id}' at '{baseAddress}'...");
            //#endif
            try
            {
                Console.WriteLine($"Before async, Thread - {Thread.CurrentThread.ManagedThreadId}");
                var response = await httpClient.GetAsync(id).ConfigureAwait(false);
                Console.WriteLine($"After async 1, Thread - {Thread.CurrentThread.ManagedThreadId}");
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.WriteLine($"After async 2, Thread - {Thread.CurrentThread.ManagedThreadId}");

            }
            catch (Exception exception)
            {
            }
        }


        static void Main()
        {
            LoadAssets();
            
            /*
            // run something as raw javascript
            Runtime.InvokeJS("alert('Hello from C#');");

            // blocking with warning
            // non-blocking in javascript is working but one can't block the C# part from Javascript
            // as this runs in the same problem as the for(;;) loop above where they block each other again
            var window = (JSObject)Runtime.GetGlobalObject("Loader");
            window.Invoke("LoadFile", "someText.txt", (Loaded)delegate(string data) { Console.WriteLine(data); }) ;

            // non blocking
            var loader = new FileLoader<string>("someText.txt");
            loader.FileLoaded += (_, __) => Console.WriteLine(loader.GetData());

            RunSomethingAsync();

            // this line is never called, therefore something in this code blocks, I do not know why right now (mr -> 15.05.2020, 10:25 am)
            Console.WriteLine("Done");
            */
        }

        /*
        static void RunSomethingAsync()
        {
            // thread before async = thread after async with this nuget package
            Console.WriteLine($"Before async, Thread - {Thread.CurrentThread.ManagedThreadId}");

            AsyncContext.Run(async () =>
            {
                Console.WriteLine($"While async, Thread - {Thread.CurrentThread.ManagedThreadId}");
                int res = await DummyProcess();
            });

            Console.WriteLine($"After async, Thread - {Thread.CurrentThread.ManagedThreadId}");
        }
        */

        static Task<int> DummyProcess()
        {
            return Task.Factory.StartNew(() =>
            {
                var i = 0;

                for (; i < 10; i++)
                    i += i;

                return i;
            });
        }
       
           
           
        
    }
}


