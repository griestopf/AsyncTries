using System;
using OpenTK;
using OpenTK.Graphics;

namespace AsyncOpenTK
{
    class Program
    {
        static void Main(string[] args)
        {
            var win = new GameWindow(640, 480, new GraphicsMode(32, 24, 0, 0) /*GraphicsMode.Default*/, "AsyncOpenTK");

            win.Run();
        }
    }
}
