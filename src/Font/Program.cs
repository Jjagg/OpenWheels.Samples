using System;
using System.Numerics;

using OpenWheels;
using OpenWheels.Rendering;
using OpenWheels.Rendering.ImageSharp;
using OpenWheels.Veldrid;

using SixLabors.Fonts;

using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Font
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

            // Our batcher lets use make calls to render lots of different primitive shapes and text.
            // When we're done the batcher sends the draw calls to the renderer which will actually do the drawing.
            // StringIdBatcher is an extension of batcher that supports registering and setting fonts and
            // textures using a string identifier. Internally in OpenWheels textures are identified by an integer.
            var batcher = new StringIdBatcher(renderer);

            const string fontId = "font";

            // This creates a font atlas and the corresponding image for the letters.
            // By default it includes only the basic Latin characters in the atlas.
            // The created image is registered with the renderer and the font can be
            // set using the StringIdBatcher by calling `SetFont(fontId)`.

            // LoadSystemFont and other extension methods to load textures and fonts into a batcher in a single method call
            // are defined in the OpenWheels.Rendering.ImageSharp library.
            // Using this library is the easiest way to handle font and texture loading, but it's a separate lib so you can
            // use another solution if you prefer.

            // Note: System fonts only work on Windows, you can use a font file and call instead `LoadFont` on other platforms.
            batcher.LoadSystemFont("Consolas", 24, '?', fontId);

            // set the font
            batcher.SetFont(fontId);

            var first = true;

            // We run the game loop here and do our drawing inside of it.
            VeldridRunLoop(window, graphicsDevice, () => 
            {
                renderer.Clear(Color.CornflowerBlue);

                // Start a new batch
                batcher.Start();

                // Note that drawing text changes the active texture to the font atlas texture.
                // So if you're rendering other stuff, make sure you set a texture before drawing anything else
                batcher.DrawText("Hello World!", new Vector2(100f), Color.Black);

                // We rotate and translate this one a little bit for style 😎
                batcher.PositionTransform = Matrix3x2.CreateTranslation(52, -154) * Matrix3x2.CreateRotation((float)Math.PI / 2f);
                batcher.DrawText("Hell  World!", Vector2.Zero, Color.Black, va: VerticalAlignment.Bottom);

                // Reset the transformation matrix
                batcher.PositionTransform = Matrix3x2.Identity;

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
                WindowTitle = "OpenWheels Text Rendering"
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
