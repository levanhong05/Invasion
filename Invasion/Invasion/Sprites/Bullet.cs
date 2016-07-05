using System;
using System.Collections;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Invasion.SpriteManagers;

using ImproviSoft.Drawing;

namespace Invasion.Sprites
{
    public enum BulletSource { Unknown = -1, PhotonBlaster, UfoPhotonBlaster, LaserBlaster }

	/// <summary>
	/// Bullet class
	/// </summary>
    public class Bullet : DrawableGameComponent 
	{
        private Vector2 position = Vector2.Zero;
        private BulletSource bulletSource = BulletSource.Unknown;

		private int	frame = 0;

        Texture2D bulletTex = null;

        Rectangle srcRect = Rectangle.Empty;
        Rectangle destRect = Rectangle.Empty;


		public Bullet(Game game, Vector2 position, BulletSource bulletSource) : base(game)
		{
            this.position = position;
            this.bulletSource = bulletSource;
            
            bulletTex = BulletsManager.BulletTex[(int)bulletSource];

            srcRect  = new Rectangle(0, 0, 20, 20);
            destRect = new Rectangle((int)position.X, (int)position.Y, 20, 20);
        }


        public BulletSource BulletSource {
            get { return bulletSource; }
        }


        public int Height {
            get { return bulletTileSize; }
        }
        const int bulletTileSize = 20;


        public Vector2 Position {
            get { return position; }

            private set {
                position = value;
                destRect.X = (int)position.X;
                destRect.Y = (int)position.Y;
            }
        }


        public int Power {
            get {
                int power = 0;

                switch (bulletSource) {
                    case Sprites.BulletSource.LaserBlaster:
                    case Sprites.BulletSource.UfoPhotonBlaster:
                        power = 1;
                        break;

                    case Sprites.BulletSource.PhotonBlaster:
                        power = 2;
                        break;
                }
                return power;
            }
        }


		public void Move(int speedY)
		{
            position.Y -= speedY;
            destRect.Y = (int)position.Y;
		}


        public override void Update(GameTime gameTime) {

            switch (bulletSource) {
                case BulletSource.PhotonBlaster:
                    Move(6);
                    break;

                case BulletSource.UfoPhotonBlaster:
                    Move(-6);
                    break;

                case BulletSource.LaserBlaster:
                    Move(10);
                    break;
            }

			if (position.Y > -20) {

				srcRect.X = frame*20;
                srcRect.Y = 0;

                if (bulletSource != BulletSource.LaserBlaster) {
                    frame++;

                    if (frame == 11 && bulletSource == BulletSource.PhotonBlaster)
                        frame = 0;

                    if (frame == 20 && bulletSource == BulletSource.UfoPhotonBlaster)
                        frame = 0;
                }
			}
		}


        public override void Draw(GameTime gameTime) {

            InvasionGame.spriteBatch.Draw(bulletTex, position, srcRect, Color.White);

            //These are for DEBUG ONLY
            //SimpleShapes.DrawRectangle(InvasionGame.spriteBatch, 2.0f, Color.Red, BoundingBox);
        }


        public Rectangle BoundingBox {
            get { return destRect; }
        }
	}
}
