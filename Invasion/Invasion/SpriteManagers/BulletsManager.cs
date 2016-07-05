using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImproviSoft.System;

using Invasion.Sprites;

namespace Invasion.SpriteManagers {


    public class BulletsManager : DrawableGameComponent {

        internal Texture2D photonTex = null;
        internal Texture2D ufoPhotonTex = null;
        internal Texture2D laserTex = null;

        List<Bullet> bulletList = new List<Bullet>(8);

        static internal Texture2D[] BulletTex = new Texture2D[3];

        public static BulletsManager Instance = null;


        public BulletsManager(Game game) : base(game) {
            Instance = this;
        }


        public override void Initialize() {
            
            base.Initialize();
        }


        public void Reset() {
            bulletList.Clear();
        }


        protected override void LoadContent() {

            laserTex = Game.Content.Load<Texture2D>("Sprites/shoot2");
            photonTex = Game.Content.Load<Texture2D>("Sprites/shoot");
            ufoPhotonTex = Game.Content.Load<Texture2D>("Sprites/shootufo");

            BulletTex[0] = photonTex;
            BulletTex[1] = ufoPhotonTex;
            BulletTex[2] = laserTex;
            
            base.LoadContent();
        }


        public override void Update(GameTime gameTime) {

            if (Invasion.CurrentScreen != GameScreen.GameplayScreen)
                return;

            foreach (Bullet bullet in bulletList)
                bullet.Update(gameTime);

            if (bulletList.Count > 0) {
                for (int x = bulletList.Count-1; x >= 0; x--) {

                    Bullet bullet = bulletList[x];

                    if ((bullet.Position.Y <= -bullet.Height) || (bullet.Position.Y >= InvasionGame.SectorHeight)) {
                        bulletList.Remove(bullet);
                    }
                }
            }

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime) {

            foreach (Bullet bullet in bulletList)
                bullet.Draw(gameTime);

            base.Draw(gameTime);
        }


        public void Add(Bullet bullet) {
            bulletList.Add(bullet);
        }


        public bool CheckForHitUFO(UFO ufo) {

            if (ufo.State == ShipState.Destroyed)
                return false;

            Rectangle ufoBoundingBox = ufo.BoundingBox;

            foreach (Bullet bullet in bulletList) {
                if (bullet.BulletSource != BulletSource.UfoPhotonBlaster) {
                    if (bullet.BoundingBox.Intersects(ufoBoundingBox)) {
                        ufo.Damaged(bullet.Power);
                        bulletList.Remove(bullet);
                        return true;
                    }
                }
            }
            return false;
        }



        /// <summary>
        /// check if the ship was hit by a bullet
        /// </summary>
        /// <returns>true if the ship was hit by a bullet
        /// else false</returns>
        public bool CheckForHitShip(StarShip starShip) {
            if (starShip.ShipState != ShipState.OK)
                return false;

            if (bulletList.Count > 0) {
                Rectangle shipBoundingBox = starShip.BoundingBox;

                for (int iBullet = bulletList.Count-1; iBullet >= 0; iBullet--) {
                    Bullet bullet = (Bullet)bulletList[iBullet];
                    Rectangle bulletBoundingBox = bullet.BoundingBox;

                    if (bulletBoundingBox.Intersects(shipBoundingBox) &&
						bullet.BulletSource == BulletSource.UfoPhotonBlaster)
                    {
                        bulletList.Remove(bullet);
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
