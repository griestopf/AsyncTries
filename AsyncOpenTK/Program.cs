using System;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using FuseeSim;
using Nito.AsyncEx;

// using OpenTK.Graphics;

namespace AsyncOpenTK
{
    class Program
    {
        static void Main(string[] args)
        {
            var win = new MyGameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
            // AsyncContext.Run(win.Run);
            win.Run();
        }
    }

    public class MyGameWindow : GameWindow
    {
        protected FuseeApp fuseeApp;

        public MyGameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            fuseeApp = new FuseeApp();

        }

        protected override void OnLoad()
        {
            base.OnLoad();
            fuseeApp.Init();
            // AsyncContext.Run(fuseeApp.Init);
        }


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            fuseeApp.RenderAFrame();
            // AsyncContext.Run(fuseeApp.RenderAFrame);
        }

        protected override void OnRenderThreadStarted()
        {
            base.OnRenderThreadStarted();
        }
    }
}
