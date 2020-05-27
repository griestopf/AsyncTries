using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using FuseeSim;

namespace Async
{

    ////////////////////////////////////////////////////////////////////////
    //
    // Start-Up code. No asyc/await in here.

    class Program
    {
        static void Main(string[] args)
        {
            RunFuseeApp();
            // AsyncContext.Run(RunFuseeApp);
        }

        static void RunFuseeApp()
        {

            var app = new FuseeApp();
            app.Init();

            while (!app.Stop)
            {
                app.RenderAFrame();
            }

            //  From https://stackoverflow.com/questions/39271492/how-do-i-create-a-custom-synchronizationcontext-so-that-all-continuations-can-be
            //  using (var context = new AsyncContext())
            //  {
            //      // Ensure the context doesn't exit until we say so.
            //      context.SynchronizationContext.OperationStarted();
            //  
            //      // TODO: set up the "exit frame" signal to call `context.SynchronizationContext.OperationCompleted()`
            //      // (note that from within the context, you can alternatively call `SynchronizationContext.Current.OperationCompleted()`
            //  
            //      // Optional: queue any work you want using `context.Factory`.
            //  
            //      // Run the context; this only returns after all work queued to this context has completed and the "exit frame" signal is triggered.
            //      context.Execute();
            //  }
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

}