// Adapted from the FrameRateCounter provided by Shawn Hargreaves

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace ImproviSoft.Diagnostics {

	public class FrameRateCounter : DrawableGameComponent {

        private static int frameRate = 0;
        private int frameCounter = 0;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        private Vector2 textPos;
        private Vector2 textShadowPos;

        private Color textColor = Color.Red;
        private Color textShadowColor = Color.Black;

        public static SpriteFont font = null;

        private string fps = "FPS: 00";

        private SpriteBatch spriteBatch;


        public FrameRateCounter(Game game, SpriteBatch spriteBatch) : base(game) {
            this.spriteBatch = spriteBatch;

            font = Game.Content.Load<SpriteFont>(@"Fonts\fpsfont");
            Debug.Assert(font != null);
            base.LoadContent();
            Initialize();
        }


        public override void Initialize() {
            base.Initialize();

            textPos = new Vector2(GraphicsDevice.Viewport.Width-250, GraphicsDevice.Viewport.Height-25);
            //textPos = new Vector2(0, 0);

            textShadowPos = new Vector2(textPos.X+2, textPos.Y+2);

        }


		public override void Update(GameTime gameTime) {

			elapsedTime += gameTime.ElapsedGameTime;

			if (elapsedTime > TimeSpan.FromSeconds(1)) {
				elapsedTime -= TimeSpan.FromSeconds(1);
				frameRate = frameCounter;
				frameCounter = 0;
                fps = string.Format("FPS: {0} {1}", frameRate, (gameTime.IsRunningSlowly? "SLOW" : ""));
			}
		}


		public override void Draw(GameTime gameTime) {
            frameCounter++;

            string timestamp = gameTime.TotalGameTime.ToString();
            timestamp = timestamp.Remove(timestamp.Length - timestamp.LastIndexOf('.') + 2);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, timestamp + " " + fps, textShadowPos, textShadowColor);
            spriteBatch.DrawString(font, timestamp + " " + fps, textPos, textColor);
            spriteBatch.End();
		}


	}
}


