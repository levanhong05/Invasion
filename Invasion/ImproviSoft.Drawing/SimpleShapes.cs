using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImproviSoft.Drawing {

    /// <summary>
    /// Draw Lines and Shapes from xnawiki.com
    /// </summary>
    public static class SimpleShapes {

        public static Texture2D blank;
        private static GraphicsDevice GraphicsDevice;


        public static void LoadContent(GraphicsDevice device) {
            GraphicsDevice = device;
            blank = new Texture2D(device, 1, 1, true, SurfaceFormat.Color);
            blank.SetData(new[] { Color.White });
        }


        public static void DrawLine(SpriteBatch spriteBatch, float width, Color color, Vector2 point1, Vector2 point2) {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            spriteBatch.Draw(blank, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
        }


        public static void DrawRectangle(SpriteBatch spriteBatch, float width, Color color, Rectangle rect) {
            DrawLine(spriteBatch, width, color, new Vector2(rect.X, rect.Y), new Vector2(rect.X+rect.Width, rect.Y));
            DrawLine(spriteBatch, width, color, new Vector2(rect.X+rect.Width, rect.Y), new Vector2(rect.X+rect.Width, rect.Y+rect.Height));
            DrawLine(spriteBatch, width, color, new Vector2(rect.X+rect.Width, rect.Y+rect.Height), new Vector2(rect.X, rect.Y+rect.Height));
            DrawLine(spriteBatch, width, color, new Vector2(rect.X, rect.Y+rect.Height), new Vector2(rect.X, rect.Y));
        }


        public static void DrawNgon(SpriteBatch spriteBatch, float width, Color color, Vector2 Centre, float Radius, int N) {
            //szize of angle between each vertex
            float Increment = (float)Math.PI * 2 / N;
            Vector2[] Vertices = new Vector2[N];

            //compute the locations of all the vertices
            for (int i = 0; i < N; i++) {
                Vertices[i].X = (float)Math.Cos(Increment * i);
                Vertices[i].Y = (float)Math.Sin(Increment * i);
            }
            //Now draw all the lines
            for (int i = 0; i < N-1; i++) {
                DrawLine(spriteBatch, width, color, Centre + Vertices[i]*Radius, Centre + Vertices[i + 1]*Radius);
            }
            DrawLine(spriteBatch, width, color, Centre + Radius*Vertices[0], Centre + Radius*Vertices[N - 1]);
        }


        public static void DrawCircle(SpriteBatch spriteBatch, float width, Color color, Vector2 Centre, float Radius) {
            //compute how many vertices we want so it looks circular
            int N = (int)(Radius/2);
            DrawNgon(spriteBatch, width, color, Centre, Radius, N);
        }

    }
}
