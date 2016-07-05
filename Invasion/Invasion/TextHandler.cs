using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

using ImproviSoft.Diagnostics;
using ImproviSoft.System;

namespace Invasion {

    /// <summary>
    /// The TextHandler class is specifically used to handle the InvasionContent/Fonts/alpha.png
    /// font.  It's implementation has been changed in the port to WP7, but it's still cannot be
    /// used with other Bitmap fonts (and shouldn't be).  For a better example of using Bitmapped
    /// fonts, see the BuxtonSketchBitmapFont usage in the 2D graphics sample at create.msdn.com:
    /// http://create.msdn.com/en-US/education/catalog/sample/graphics_2d
    /// </summary>
    static class TextHandler {

        static Texture2D alphabetTex = null;

        static Rectangle[] CharSrcRect = null;

        const int AlphabetSize = 43;
        
        /// <summary>
        /// Initialize the positions of the alphabet characters
        /// </summary>
        static public void LoadContent(ContentManager content) {
            alphabetTex = content.Load<Texture2D>("Fonts/alpha");

            CharSrcRect = new Rectangle[AlphabetSize];

            CharSrcRect[0] = new Rectangle(0, 0, 15, 15); // 0
            CharSrcRect[1] = new Rectangle(19, 0, 9, 15); // 1
            CharSrcRect[2] = new Rectangle(31, 0, 16, 15); // 2
            CharSrcRect[3] = new Rectangle(47, 0, 15, 15); // 3
            CharSrcRect[4] = new Rectangle(65, 0, 14, 15); // 4
            CharSrcRect[5] = new Rectangle(80, 0, 15, 15); // 5
            CharSrcRect[6] = new Rectangle(96, 0, 15, 15); // 6
            CharSrcRect[7] = new Rectangle(114, 0, 12, 15); // 7
            CharSrcRect[8] = new Rectangle(128, 0, 16, 15); // 8
            CharSrcRect[9] = new Rectangle(145, 0, 15, 15); // 9
            CharSrcRect[10] = new Rectangle(160, 0, 16, 15); // A
            CharSrcRect[11] = new Rectangle(176, 0, 16, 15); // B
            CharSrcRect[12] = new Rectangle(193, 0, 14, 15); // C
            CharSrcRect[13] = new Rectangle(207, 0, 16, 15); // D
            CharSrcRect[14] = new Rectangle(224, 0, 14, 15); // E
            CharSrcRect[15] = new Rectangle(238, 0, 15, 15); // F
            CharSrcRect[16] = new Rectangle(253, 0, 15, 15); // G
            CharSrcRect[17] = new Rectangle(269, 0, 16, 15); // H
            CharSrcRect[18] = new Rectangle(285, 0, 6, 15); // I
            CharSrcRect[19] = new Rectangle(292, 0, 16, 15); // J
            CharSrcRect[20] = new Rectangle(308, 0, 16, 15); // K
            CharSrcRect[21] = new Rectangle(324, 0, 16, 15); // L
            CharSrcRect[22] = new Rectangle(340, 0, 25, 15); // M
            CharSrcRect[23] = new Rectangle(365, 0, 16, 15); // N
            CharSrcRect[24] = new Rectangle(382, 0, 16, 15); // O
            CharSrcRect[25] = new Rectangle(398, 0, 16, 15); // P
            CharSrcRect[26] = new Rectangle(414, 0, 16, 15); // Q
            CharSrcRect[27] = new Rectangle(430, 0, 15, 15); // R
            CharSrcRect[28] = new Rectangle(447, 0, 14, 15); // S
            CharSrcRect[29] = new Rectangle(464, 0, 12, 15); // T
            CharSrcRect[30] = new Rectangle(477, 0, 16, 15); // U
            CharSrcRect[31] = new Rectangle(494, 0, 15, 15); // V
            CharSrcRect[32] = new Rectangle(509, 0, 24, 15); // W
            CharSrcRect[33] = new Rectangle(533, 0, 16, 15); // X
            CharSrcRect[34] = new Rectangle(550, 0, 15, 15); // Y
            CharSrcRect[35] = new Rectangle(564, 0, 16, 15); // Z
            CharSrcRect[36] = new Rectangle(579, 0, 21, 15); // @
            CharSrcRect[37] = new Rectangle(600, 0, 11, 15); // -
            CharSrcRect[38] = new Rectangle(611, 0, 6, 15); //comma
            CharSrcRect[39] = new Rectangle(617, 0, 6, 15); //period
            CharSrcRect[40] = new Rectangle(623, 0, 6, 15); //colon
            CharSrcRect[41] = new Rectangle(629, 0, 6, 15); //exclamation point
            CharSrcRect[42] = new Rectangle(635, 0, 8, 15); //space
        }


        public static Vector2 MeasureString(string text) {
            Vector2 size = Vector2.Zero;
            Rectangle srcRect;

            foreach (char ch in text) {
                int index = GetCharIndex(ch);
                srcRect = CharSrcRect[index];

                size.X += srcRect.Width;

                if (srcRect.Height > size.Y)
                    size.Y = srcRect.Height;
            }

            return size;
        }


        /// <summary>
        /// Draw text on the screen
        /// </summary>
        public static void DrawText(string text, Vector2 pos, float scale, Color color) {
            Rectangle srcRect;
            int index;

            foreach (char ch in text) {
                index = GetCharIndex(ch);
                srcRect = CharSrcRect[index];
                InvasionGame.spriteBatch.Draw(alphabetTex, pos, srcRect, color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
                pos.X += srcRect.Width * scale;
            }
        }


        /// <summary>
        /// Draw text on the screen
        /// </summary>
        public static void DrawText(string text, Vector2 pos) {
            Rectangle srcRect;
            int index;

            foreach (char ch in text) {
                index = GetCharIndex(ch);
                srcRect = CharSrcRect[index];
                InvasionGame.spriteBatch.Draw(alphabetTex, pos, srcRect, Color.White);
                pos.X += srcRect.Width;
            }
        }


        /// <summary>
        /// Draw text centered around the given X position at the given Y height
        /// </summary>
        public static void DrawTextCentered(string text, Vector2 pos) {
            Vector2 size = MeasureString(text);
            pos.X -= size.X/2;
            DrawText(text, pos);
        }


        /// <summary>
        /// Draw text centered around the given X position at the given Y height
        /// </summary>
        public static void DrawTextCentered(string text, Vector2 pos, float scale, Color color) {
            Vector2 size = MeasureString(text) * scale;
            pos.X -= size.X/2;
            DrawText(text, pos, scale, color);
        }


        private static int GetCharIndex(char ch) {
            int index = AlphabetSize-1; // default character (currently set to Space)

            if (ch != ' ') {
                if (ch >= 48 && ch <= 57) // 0 to 9
                    index = (int)ch - 48;
                else if (ch == '@')
                    index = 36;
                else if (ch == '-')
                    index = 37;
                else if (ch == ',')
                    index = 38;
                else if (ch == '.')
                    index = 39;
                else if (ch == ':')
                    index = 40;
                else if (ch == '!')
                    index = 41;
                else
                    index = (int)ch - 55; // A to Z
            }
            else
                index = AlphabetSize-1;

            return index;
        }
    }
}
