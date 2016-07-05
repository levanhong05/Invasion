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

    public class UFOsManager : DrawableGameComponent {

        static internal Texture2D ufo1Tex = null;
        static internal Texture2D ufo2Tex = null;
        static internal Texture2D ufo3Tex = null;

        static internal Texture2D ufoTex = null;

        static internal Texture2D explosion1Tex = null;
        static internal Texture2D explosion2Tex = null;
        static internal Texture2D explosion3Tex = null;

        static internal Texture2D explosionTex = null;

        List<UFO> UFOsList = new List<UFO>(16);

        public static UFOsManager Instance = null;

        int shootChance = 1000;


        public UFOsManager(Game game)
            : base(game) {
            Instance = this;
        }


        public override void Initialize() {

            base.Initialize();
        }


        public void Reset() {
            UFOsList.Clear();
        }

        protected override void LoadContent() {

            ufo1Tex = Game.Content.Load<Texture2D>("Sprites/ufo1");
            ufo2Tex = Game.Content.Load<Texture2D>("Sprites/ufo2");
            ufo3Tex = Game.Content.Load<Texture2D>("Sprites/ufo3");
            ufoTex = ufo1Tex;

            explosion1Tex = Game.Content.Load<Texture2D>("Sprites/explosion1");
            explosion2Tex = Game.Content.Load<Texture2D>("Sprites/explosion2");
            explosion3Tex = Game.Content.Load<Texture2D>("Sprites/explosion3");
            explosionTex = explosion1Tex;

            base.LoadContent();
        }


        public void SetLevel(int iLevel) {

            int iWave = iLevel % 6;

            switch (iWave) {
                case 0: // level 6
                    ufoTex = ufo3Tex;
                    explosionTex = explosion3Tex;
                    break;

                case 4:
                case 5:
                    ufoTex = ufo2Tex;
                    explosionTex = explosion2Tex;
                    break;

                default: // iWave = 1, 2, or 3
                    ufoTex = ufo1Tex;
                    explosionTex = explosion1Tex;
                    break;
            }
        }


        public override void Update(GameTime gameTime) {

            if (Invasion.CurrentScreen != GameScreen.GameplayScreen)
                return;

            foreach (UFO ufo in UFOsList)
                ufo.Update(gameTime);

            if (UFOsList.Count > 0) {
                for (int iUFO = UFOsList.Count-1; iUFO >= 0; iUFO--) {
                    UFO ufo = (UFO)UFOsList[iUFO];

                    if (ufo.State == ShipState.Destroyed) {
                        UFOsList.Remove(ufo);
                        InvasionGame.Scoreboard.NumSectorUFOs--;
                    }
                }
            }

            foreach (UFO ufo in UFOsList) {
                if (CheckForUFOtoUFOCrash(ufo))
                    ufo.Bounce();
            }

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime) {

            foreach (UFO ufo in UFOsList)
                ufo.Draw(gameTime);

            base.Draw(gameTime);
        }


        public void Add(UFO ufo) {
            UFOsList.Add(ufo);
        }


        /// <summary>
        /// Check to see if a ufo has hit into another ufo
        /// </summary>
        /// <param name="ufo">ufo to check against</param>
        /// <returns>true if there is a crash
        /// false if there is no crash</returns>
        bool CheckForUFOtoUFOCrash(UFO ufo) {

            foreach (UFO otherUFO in UFOsList) {
                if (otherUFO.State == ShipState.OK && otherUFO.Y == ufo.Y && otherUFO != ufo) {
                    if (ufo.X < otherUFO.X) {
                        if ((otherUFO.X - ufo.X <= 70) &&
							((ufo.Speed > 0 && ufo.UFOModel == UFOModel.SilverUFO) ||
							 (ufo.Maneuver == 0 && ufo.UFOModel == UFOModel.RedUFO)))
                            return true;
                    }
                    else {
                        if ((ufo.X - otherUFO.X <= 70) &&
							((ufo.Speed < 0 && ufo.UFOModel == UFOModel.SilverUFO) ||
							(ufo.Maneuver == 1 && ufo.UFOModel == UFOModel.RedUFO)))
                            return true;
                    }
                }
            }
            return false;
        }


        public void InitLevel(int iLevel) {
            //int sectorPosX = (int)InvasionGame.SectorPos.X;
            float sectorPosX = InvasionGame.SectorPos.X;

            int valinc = 1;

            int strength = (iLevel-1) / 6 + 1;

            switch (iLevel % 6) {
                case 0:
                    int	i = iLevel / 6;

                    UFO ufo = new UFO(Game, UFOModel.RedUFO, new Vector2(sectorPosX + 20, 20), strength);
                    ufo.Speed = i;
                    ufo.Maneuver = 0;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.RedUFO, new Vector2(sectorPosX + 110, 90), strength);
                    ufo.Speed = i;
                    ufo.Maneuver = 0;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.RedUFO, new Vector2(sectorPosX + 20, 160), strength);
                    ufo.Speed = i;
                    ufo.Maneuver = 0;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.RedUFO, new Vector2(sectorPosX + 540, 20), strength);
                    ufo.Speed = i;
                    ufo.Maneuver = 1;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.RedUFO, new Vector2(sectorPosX + 470, 90), strength);
                    ufo.Speed = i;
                    ufo.Maneuver= 1;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.RedUFO, new Vector2(sectorPosX + 540, 160), strength);
                    ufo.Speed = i;
                    ufo.Maneuver = 1;
                    UFOsList.Add(ufo);
                    ufo = null;

                    break;

                case 1:

                    for (i=0; i<3; i++) {
                        ufo = new UFO(Game, UFOModel.SilverUFO, new Vector2(sectorPosX + 15 + (i*30), (i * 65)+10), strength);
                        ufo.Speed = 1;
                        ufo.Maneuver = 0;
                        ufo.AltitudeTargetDrop = 1;
                        UFOsList.Add(ufo);
                        ufo = null;
                    }

                    ufo = new UFO(Game, UFOModel.SilverUFO, new Vector2(sectorPosX + 45, 195), strength);
                    ufo.Speed = 1;
                    ufo.Maneuver = 0;
                    ufo.AltitudeTargetDrop = 1;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.SilverUFO, new Vector2(sectorPosX + 15, 260), strength);
                    ufo.Speed = 1;
                    ufo.Maneuver = 0;
                    ufo.AltitudeTargetDrop = 1;
                    UFOsList.Add(ufo);
                    ufo = null;

                    for (i=0; i<3; i++) {
                        ufo = new UFO(Game, UFOModel.SilverUFO, new Vector2(sectorPosX + 560 - (i*30), (i * 65)+10), strength);
                        ufo.Speed = -1;
                        ufo.Maneuver = 0;
                        ufo.AltitudeTargetDrop = 1;
                        UFOsList.Add(ufo);
                        ufo = null;
                    }

                    ufo = new UFO(Game, UFOModel.SilverUFO, new Vector2(sectorPosX + 530, 195), strength);
                    ufo.Speed = -1;
                    ufo.Maneuver = 0;
                    ufo.AltitudeTargetDrop = 1;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.SilverUFO, new Vector2(sectorPosX + 560, 260), strength);
                    ufo.Speed = -1;
                    ufo.Maneuver = 0;
                    ufo.AltitudeTargetDrop = 1;
                    UFOsList.Add(ufo);
                    ufo = null;

                    break;

                case 2:
                    for (i=0; i<5; i++) {
                        ufo = new UFO(Game, UFOModel.SilverUFO, new Vector2(sectorPosX + 15 + (i*30), (i * 65)+10), strength);
                        ufo.Speed = 1;
                        ufo.Maneuver = 0;
                        ufo.AltitudeTargetDrop = 1;
                        UFOsList.Add(ufo);
                        ufo = null;
                    }

                    for (i=0; i<5; i++) {
                        ufo = new UFO(Game, UFOModel.SilverUFO, new Vector2(sectorPosX + 560 - (i*30), (i * 65)+10), strength);
                        ufo.Speed = -1;
                        ufo.Maneuver = 0;
                        ufo.AltitudeTargetDrop = 1;
                        UFOsList.Add(ufo);
                        ufo = null;
                    }
                    break;
                case 3:
                    for (int j=0; j<3; j++) {
                        for (i=0; i<8; i++) {
                            ufo = new UFO(Game, UFOModel.SilverUFO, new Vector2(sectorPosX + (i * 77)+15, j*80), strength);
                            ufo.Speed = valinc;
                            ufo.Maneuver = 0;
                            UFOsList.Add(ufo);
                            ufo = null;
                        }
                        valinc = -valinc;
                    }
                    break;

                case 4:
                    ufo  = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 20, 20), strength);
                    ufo.Speed = 2;
                    ufo.Maneuver = 2;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 180, 180), strength);
                    ufo.Speed = 2;
                    ufo.Maneuver = 2;
                    UFOsList.Add(ufo);
                    ufo = null;


                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 530, 110), strength);
                    ufo.Speed = 2;
                    ufo.Maneuver = 3;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 100, 100), strength);
                    ufo.Speed = 2;
                    ufo.Maneuver = 1;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 450, 190), strength);
                    ufo.Speed = 2;
                    ufo.Maneuver = 0;
                    UFOsList.Add(ufo);
                    ufo = null;
                    break;

                case 5:
                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 20, 20), strength);
                    ufo.Speed = 3;
                    ufo.Maneuver = 2;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 90, 90), strength);
                    ufo.Speed = 3;
                    ufo.Maneuver = 2;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 20, 160), strength);
                    ufo.Speed = 3;
                    ufo.Maneuver = 3;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 540, 20), strength);
                    ufo.Speed = 3;
                    ufo.Maneuver = 3;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 470, 90), strength);
                    ufo.Speed = 3;
                    ufo.Maneuver = 3;
                    UFOsList.Add(ufo);
                    ufo = null;

                    ufo = new UFO(Game, UFOModel.OrangeUFO, new Vector2(sectorPosX + 540, 160), strength);
                    ufo.Speed = 3;
                    ufo.Maneuver = 2;
                    UFOsList.Add(ufo);
                    ufo = null;
                    break;
            }

            shootChance = 180 - (40 * (strength-1));

            if (shootChance < 50)
                shootChance = 50;

            InvasionGame.Scoreboard.NumSectorUFOs = UFOsList.Count;
        }


        internal bool IsTimeToShoot {
            get { return RandomManager.Instance.Next(shootChance) == 0; }
        }

        public int Count {
            get { return UFOsList.Count; }
        }

#if DEBUG
        public void DebugPrintList() {
            Debug.WriteLine("");

            for (int i = 0; i < UFOsList.Count; i++) {
                UFO ufo = UFOsList[i];
                Debug.WriteLine("UFO[" + ((i<10)?"0":"") + i + "], State: " + ufo.State + ", Pos = <" + ufo.X + " " + ufo.Y + ">, Model: " + ufo.UFOModel + ", AltitudeTargetDrop: " + ufo.AltitudeTargetDrop + ", Maneuver: " + ufo.Maneuver + ", Speed: " + ufo.Speed + ", Strength: " + ufo.Strength + ", BoundingBox: " + ufo.BoundingBox);
            }
            Debug.WriteLine("");
        }


        public int GetIndex(UFO ufo) {
            return UFOsList.IndexOf(ufo);
        }
#endif
    }
}
