using System;
using System.Numerics;

using OpenWheels;
using OpenWheels.Rendering;
using OpenWheels.Veldrid;

using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Texture
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create the window and the graphics device
            VeldridInit(out var window, out var graphicsDevice);

            // Create a renderer that implements the OpenWheels.Rendering.IRenderer interface
            // this guy actually draws everything to the backbuffer
            var renderer = new VeldridRenderer(graphicsDevice);

            // OpenWheels always requires a texture to render, so renderer implementations only need a single shader
            // Even for untextured primitives we need to have a texture set. So we create a white 1x1 texture for those.
            Span<Color> blankSpan = stackalloc Color[] { Color.White };

            // Our batcher lets use make calls to render lots of different primitive shapes and text.
            // When we're done the batcher sends the draw calls to the renderer which will actually do the drawing.

            // Alternatively to Batcher you can use StringIdBatcher so you can register and set the active texture
            // and font with a string identifier.
            var batcher = new Batcher(renderer);

            var first = true;

            // We run the game loop here and do our drawing inside of it.
            VeldridRunLoop(window, graphicsDevice, () =>
            {
                renderer.Clear(Color.CornflowerBlue);

                // Start a new batch
                batcher.Start();

                // Finish the batch and let the renderer draw everything to the back buffer.
                batcher.Finish();

                if (first)
                {
                    Console.WriteLine("Vertices: " + batcher.VerticesSubmitted);
                    Console.WriteLine("Indices: " + batcher.IndicesSubmitted);
                    Console.WriteLine("Batches: " + batcher.BatchCount);
                    first = false;
                }
            });

            renderer.Dispose();
            graphicsDevice.Dispose();
        }

        private static void VeldridInit(out Sdl2Window window, out GraphicsDevice graphicsDevice)
        {
            WindowCreateInfo windowCI = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "OpenWheels Texture Sample"
            };

            window = VeldridStartup.CreateWindow(ref windowCI);

            // no debug, no depth buffer and enable v-sync
            var gdo = new GraphicsDeviceOptions(false, null, syncToVerticalBlank: true);
            graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, gdo);
        }

        private static void VeldridRunLoop(Sdl2Window window, GraphicsDevice graphicsDevice, Action action)
        {
            while (window.Exists)
            {
                window.PumpEvents();

                if (window.Exists)
                {
                    action();

                    graphicsDevice.SwapBuffers();
                    graphicsDevice.WaitForIdle();
                }
            }
        }
    }
}
