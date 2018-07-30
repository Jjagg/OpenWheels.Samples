using System;
using System.Numerics;

using OpenWheels;
using OpenWheels.Fonts;
using OpenWheels.Fonts.ImageSharp;
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

            // OpenWheels always requires a texture to render, so renderer implementations only need a single shader
            // Even for untextured primitives we need to have a texture set. So we create a white 1x1 texture for those.
            Span<Color> blankSpan = stackalloc Color[1]{Color.White};
            var blank = renderer.RegisterTexture(blankSpan, 1, 1);

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

            // We run the game loop here and do our drawing inside of it.
            VeldridRunLoop(window, graphicsDevice, () => 
            {
                renderer.Clear(Color.Black);

                // Start a new batch
                batcher.Start();

                // set the texture to the blank one we registered so we can draw a colored rectangle
                // Note that drawing text changes the active texture to the font atlas texture.
                batcher.SetTexture(blank);
                batcher.FillRect(new RectangleF(70, 35, 200, 200), Color.CornflowerBlue);

                batcher.DrawText("Hello World!", new Vector2(100f), Color.Black);

                // We rotate and translate this one a little bit for style 😎
                batcher.TransformMatrix = Matrix4x4.CreateTranslation(52, -154, 0) * Matrix4x4.CreateRotationZ((float)Math.PI / 2f);
                batcher.DrawText("Hell  World!", Vector2.Zero, Color.Black, va: VerticalAlignment.Bottom);

                // Reset the transformation matrix
                batcher.TransformMatrix = Matrix4x4.Identity;

                // Finish the batch and let the renderer draw everything to the back buffer.
                batcher.Finish();
            });

            renderer.Dispose();
            graphicsDevice.Dispose();
        }

        private static void VeldridInit(out Sdl2Window window, out GraphicsDevice graphicsDevice)
        {
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "OpenWheels Text Rendering",
            };
            window = VeldridStartup.CreateWindow(ref windowCI);

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
