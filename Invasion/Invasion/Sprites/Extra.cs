using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Invasion.SpriteManagers;


namespace Invasion.Sprites
{
	/// <summary>
	/// Extra Class
	/// </summary>
    public class Extra : DrawableGameComponent 
	{
        public enum ExtraType { NotSet = -1, PhotonAmmo, Weapons, Hundred, BlasterAmmo, Shield }

        private ExtraType extraType = ExtraType.PhotonAmmo;

        public Vector2 Position {
            get { return position; }
            set {
                position = value;
                destRect.X = (int)position.X;
                destRect.Y = (int)position.Y;
            }
        }
        private Vector2 position;

        public const int NumExtraTypes = 5;

		private int	frame = 0;

        private Texture2D extrasTex = null;
        
        Rectangle srcRect = Rectangle.Empty;
        Rectangle destRect = Rectangle.Empty;

        public int SpeedY {
            get;
            set;
        }


        public Extra(Game game, Vector2 position, ExtraType extraType) : base(game) {
            this.position = position;
            this.extraType = extraType;

            SpeedY = 0;
            SetupTexture();
        }


        public Extra(Game game, Vector2 position, ExtraType extraType, int speedY) : base(game) {
            this.position = position;
            this.extraType = extraType;

            SpeedY = speedY;
            SetupTexture();
        }


        private void SetupTexture() {
            extrasTex = ExtrasManager.ExtrasTex;
            srcRect  = new Rectangle(0, 0, 25, 25);
            destRect = new Rectangle((int)position.X, (int)position.Y, 25, 25);
        }


        public ExtraType Type {
            get { return extraType; }
            set { extraType = value; }
		}


		public void Move() {
            position.Y -= SpeedY;
            destRect.Y = (int)position.Y;
		}


        public override void Update(GameTime gameTime) {
            Update();
            Move();
        }


		public void Update() 
        {
            srcRect.X = frame*25;
            srcRect.Y = (int)extraType*25;

			frame++;

			if (frame == 19)
				frame = 0;
		}


        public override void Draw(GameTime gameTime) {
            InvasionGame.spriteBatch.Draw(extrasTex, position, srcRect, Color.White);
        }


        public Rectangle BoundingBox {
            get { return destRect; }
        }
	}
}
