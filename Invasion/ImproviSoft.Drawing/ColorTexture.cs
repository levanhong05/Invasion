//from xnawiki.com

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace ImproviSoft.Drawing {

    public static class ColorTexture {

        /// <summary>
        /// Creates a 1x1 pixel black texture.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <returns>The newly created texture.</returns>
        public static Texture2D Create(GraphicsDevice graphicsDevice) {
            return Create(graphicsDevice, 1, 1, new Color());
        }

        /// <summary>
        /// Creates a 1x1 pixel texture of the specified color.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <param name="color">The color to set the texture to.</param>
        /// <returns>The newly created texture.</returns>
        public static Texture2D Create(GraphicsDevice graphicsDevice, Color color) {
            return Create(graphicsDevice, 1, 1, color);
        }

        /// <summary>
        /// Creates a texture of the specified color.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="color">The color to set the texture to.</param>
        /// <returns>The newly created texture.</returns>
        public static Texture2D Create(GraphicsDevice graphicsDevice, int width, int height, Color color) {
            // create the rectangle texture without colors
            Texture2D texture = new Texture2D(
                graphicsDevice,
                width,
                height,
                false, //mipmap
                SurfaceFormat.Color);

            // Create a color array for the pixels
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = new Color(color.ToVector3());
            }

            // Set the color data for the texture
            texture.SetData(colors);

            return texture;
        }


    }

}
