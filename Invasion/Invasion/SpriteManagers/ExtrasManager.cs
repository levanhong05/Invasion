using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImproviSoft.System;

using Invasion.Sprites;

namespace Invasion.SpriteManagers {


    public class ExtrasManager : DrawableGameComponent {

        static internal Texture2D ExtrasTex = null;

        private SoundEffect gotExtraSoundEffect = null;

        List<Extra> extrasList = new List<Extra>(5);

        public static ExtrasManager Instance = null;



        public ExtrasManager(Game game)
            : base(game) {
            Instance = this;
        }


        public override void Initialize() {
            
            base.Initialize();
        }
        

        public void Reset() {
            extrasList.Clear();
        }


        protected override void LoadContent() {

            ExtrasTex = Game.Content.Load<Texture2D>("Sprites/extras");

            gotExtraSoundEffect = Game.Content.Load<SoundEffect>("Sounds/getextra");

            base.LoadContent();
        }


        public override void Update(GameTime gameTime) {

            if (Invasion.CurrentScreen != GameScreen.GameplayScreen && 
                Invasion.CurrentScreen != GameScreen.HelpScreen)
                return;

            foreach (Extra extra in extrasList)
                extra.Update(gameTime);

            // remove extras that go beyond the bottom of the screen
            for (int iExtra = extrasList.Count-1; iExtra >= 0; iExtra--) {
                Extra extra = (Extra)extrasList[iExtra];

                if (extra.Position.Y > InvasionGame.SectorHeight)
                    extrasList.Remove(extra);
            }

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime) {

            foreach (Extra extra in extrasList)
                extra.Draw(gameTime);

            base.Draw(gameTime);
        }


        public void Add(Extra extra) {
            extrasList.Add(extra);
        }


        public void CheckForShipAward(StarShip starShip) {

            if (extrasList.Count == 0)
                return;

            Extra.ExtraType extra = CheckForHitExtra(starShip);

            switch (extra) {
                case Extra.ExtraType.PhotonAmmo: //A
                    starShip.PhotonAmmo += 25;
                    break;

                case Extra.ExtraType.Weapons: //W
                    if (starShip.MaxWeapon < Weapon.Photon3)
                        starShip.MaxWeapon++;

                    //starShip.Weapon = starShip.MaxWeapon; //now using StarShip.AutoSelectMaxWeapon()
                    break;

                case Extra.ExtraType.Hundred: //100
                    InvasionGame.Scoreboard.Score += 100 * InvasionGame.Scoreboard.Wave;
                    break;

                case Extra.ExtraType.BlasterAmmo: //B
                    starShip.LaserAmmo += 40;
                    break;

                case Extra.ExtraType.Shield: //S
                    starShip.Shield += 10;
                    break;
            }

            if (extra != Extra.ExtraType.NotSet)
                SoundManager.Instance.Play(gotExtraSoundEffect);
        }



        /// <summary>
        /// Check if the ship got an extra
        /// </summary>
        /// <returns>the value of the extra that was hit</returns>
        Extra.ExtraType CheckForHitExtra(StarShip starShip) {
            Extra.ExtraType hitExtra = Extra.ExtraType.NotSet;

            Rectangle shipBoundingBox = starShip.BoundingBox;

            for (int iExtra = extrasList.Count-1; iExtra >= 0; iExtra--) {
                Extra extra = (Extra)extrasList[iExtra];
                Rectangle extraBoundingBox = extra.BoundingBox;

                if (extraBoundingBox.Intersects(shipBoundingBox)) {
                    hitExtra = extra.Type;
                    extrasList.Remove(extra);
                    return hitExtra;
                }
            }
            return hitExtra;
        }


        public int Count {
            get { return extrasList.Count; }
        }

    }
}
