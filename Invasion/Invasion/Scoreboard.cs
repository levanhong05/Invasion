using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Invasion.SpriteManagers;
using Invasion.Sprites;

namespace Invasion {

    enum DisplaySide { Left, Right };


    /// <summary>
    /// The Scoreboard class displays the game's score, level, and key data items.
    /// In previous versions, the Scoreboard was known as the Status bar and was 
    /// located at the bottom of the screen.  The Windows Phone port moves the
    /// Scoreboard to either side of the gameplay screen (user-configurable), changes
    /// the displayed font and adds some data-items with the added screen space.
    /// </summary>
    class Scoreboard : DrawableGameComponent {

        #region Fields & Properties

        Texture2D scoreboardTex = null;


        // Scoreboard Location -------------------------------------

        Vector2 position = LeftPos;

        public DisplaySide DisplaySide {
            get { return scoreboardPos; }
            set {
                scoreboardPos = value;
                if (scoreboardPos == DisplaySide.Left) {
                    position = LeftPos;
                    InvasionGame.SectorPos = new Vector2(159, 0);
                }
                else {
                    position = RightPos;
                    InvasionGame.SectorPos = Vector2.Zero;
                }
            }
        }
        private DisplaySide scoreboardPos = DisplaySide.Left;

        public static readonly Vector2 LeftPos = Vector2.Zero;
        public static readonly Vector2 RightPos = new Vector2(800-159, 0);
        public const int Width = 159;


        // Weapon --------------------------------------------------

        Texture2D weaponsTex = null;
        Texture2D autoSelectTex = null;
        Texture2D autoSelectDisabledTex = null;
        Texture2D weaponNextTex = null;

        Vector2 weaponPos = new Vector2(44, 450);
        Rectangle weaponSrcRect = new Rectangle(0, 0, 87, 20);

        public Weapon Weapon {
            get { return weapon; }
            set {
                weapon = value;
                weaponSrcRect.X = (int)Weapon*weaponSrcRect.Width;
            }
        }
        private Weapon weapon = Weapon.Laser1;


        private Vector2 WeaponAreaPos {
            get { return position + weaponAreaPos; }
        }
        readonly Vector2 weaponAreaPos = new Vector2(0, 420);

        public Rectangle WeaponTapRect {
            get {
                weaponTapRect.X = (int)(WeaponAreaPos.X + 38);
                return weaponTapRect;
            }
        }
        Rectangle weaponTapRect = new Rectangle(38, 420, 159-38, 60);


        public Rectangle AutoSelectTapRect {
            get {
                autoSelectTapRect.X = (int)(WeaponAreaPos.X);
                return autoSelectTapRect;
            }
        }
        Rectangle autoSelectTapRect = new Rectangle(0, 420, 38, 60);


        // MaxWeapon --------------------------------------------------

        public Weapon MaxWeapon {
            get { return maxWeapon; }
            set {
                maxWeapon = value;
                //maxWeaponSrcRect.X = (int)MaxWeapon*maxWeaponSrcRect.Width;
                photonAmmoColor = (maxWeapon <= Weapon.Laser3)? Color.Gray : Color.White;
            }
        }
        private Weapon maxWeapon = Weapon.Laser1;


        // UFOs   --------------------------------------------------

        readonly Vector2 sectorUFOsPos = new Vector2(83, 193);
        string sectorUFOsText = "-";

        public int NumSectorUFOs {
            get { return numSectorUFOs; }
            set {
                numSectorUFOs = value;
                sectorUFOsText = numSectorUFOs.ToString();
            }
        }
        int numSectorUFOs = 0;


        // Ammo   --------------------------------------------------

        readonly Vector2 laserAmmoPos = new Vector2(22, 270);
        readonly Vector2 photonAmmoPos = new Vector2(93, 270);
        string laserAmmoText = "000";
        string photonAmmoText = "000";

        public int LaserAmmo {
            get { return laserAmmo; }
            set {
                laserAmmo = Math.Max(0, Math.Min(value, maxLaserAmmo));
                laserAmmoText = laserAmmo.ToString().PadLeft(3, '0');
            }
        }
        int laserAmmo = 0;
        const int maxLaserAmmo = 999;

        public int PhotonAmmo {
            get { return photonAmmo; }
            set {
                photonAmmo = Math.Max(0, Math.Min(value, maxPhotonAmmo));
                photonAmmoText = photonAmmo.ToString().PadLeft(3, '0');
            }
        }
        int photonAmmo = 0;
        const int maxPhotonAmmo = 999;

        Color photonAmmoColor = Color.Gray;


        // Shield --------------------------------------------------

        const int maxShield = 50;

        Texture2D shieldTex;
        string shieldText = (maxShield*2).ToString();
        readonly Rectangle initShieldSrcRect = new Rectangle(0, 0, maxShield, 13);
        Rectangle shieldSrcRect = new Rectangle(0, 0, maxShield, 13);
        Vector2 shieldPos = new Vector2(17, 103);
        Vector2 shieldTextPos = new Vector2(98, 101);
        Color shieldColor = Color.Lime;

        public int Shield {
            get { return shield;  }
            set {
                shield = Math.Max(0, Math.Min(value, maxShield));

                shieldSrcRect.Width = shield;

                int damagePerHit = StarShip.GetShieldDamageForHit();

                if (shield <= damagePerHit)
                    shieldColor = Color.Red;
                else if (shield <= maxShield/2)
                    shieldColor = Color.Orange;
                else
                    shieldColor = Color.Lime;

                shieldText = (shield*2).ToString();
            }
        }
        int shield = maxShield;


        // Score  --------------------------------------------------

        const char scorePadding = '0';

        public int Score {
            get { return score; }
            set {
                score = value;
                scoreText = score.ToString().PadLeft(8, scorePadding);
            }
        }
        int score = 0;
        string scoreText = "00000000";

        Vector2 scorePos = new Vector2(18, 36);


        // Level  --------------------------------------------------

        const int sectorsPerWave = 6;

        public int Level {
            get { return level; }
            set {
                level = value;
                levelText = level.ToString();

                Wave = (level-1) / sectorsPerWave + 1;
                Sector = (level-1) % sectorsPerWave + 1;
            }
        }
        int level = 0;
        string levelText = "";

        Vector2 levelPos = new Vector2(83, 340);


        // Wave  --------------------------------------------------

        public int Wave {
            get { return wave; }
            set {
                wave = value;
                waveText = wave.ToString();
            }
        }
        int wave = 0;
        string waveText = "";

        Vector2 wavePos = new Vector2(43, 382);


        // Sector  --------------------------------------------------

        public int Sector {
            get { return sector; }
            set {
                sector = value;
                sectorText = sector.ToString();
            }
        }
        int sector = 0;
        string sectorText = "";

        Vector2 sectorPos = new Vector2(113, 382);


        // Difficulty Level  ---------------------------------------

        public DifficultyLevel DifficultyLevel {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// The Scoreboard constructor
        /// </summary>
        /// <param name="game"></param>
        public Scoreboard(Game game) : base(game) {
            DifficultyLevel = DifficultyLevel.Medium;

            scoreboardPos = DisplaySide.Left;
        }


        public override void Initialize() {
            base.Initialize();
        }


        protected override void LoadContent() {
            //TODO: These textures should probably be put into a SpriteSheet in the next version:

            autoSelectDisabledTex = Game.Content.Load<Texture2D>("Scoreboard/AutoSelectDisabled");
            autoSelectTex = Game.Content.Load<Texture2D>("Scoreboard/AutoSelect");
            weaponNextTex = Game.Content.Load<Texture2D>("Scoreboard/WeaponNext");
            weaponsTex = Game.Content.Load<Texture2D>("Scoreboard/Weapons");
            shieldTex = Game.Content.Load<Texture2D>("Scoreboard/Shield");

            scoreboardTex = Game.Content.Load<Texture2D>("Scoreboard/Scoreboard");
        
            base.LoadContent();
        }

        
        public override void Update(GameTime gameTime) {

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime) {
            
            // Scoreboard
            InvasionGame.spriteBatch.Draw(scoreboardTex, position, Color.White);

            // Weapon
            Vector2 autoSelectPos = new Vector2(weaponPos.X-36, weaponPos.Y-2);
            if (StarShip.AutoSelectWeapon)
                InvasionGame.spriteBatch.Draw(autoSelectTex, position + autoSelectPos, Color.White);
            else
                InvasionGame.spriteBatch.Draw(autoSelectDisabledTex, position + autoSelectPos, Color.White);

            if (Weapon != MaxWeapon || MaxWeapon != Weapon.Laser1) {
                Vector2 weaponNextPos = new Vector2(weaponPos.X+88, weaponPos.Y-2);
                InvasionGame.spriteBatch.Draw(weaponNextTex, position + weaponNextPos, Color.White);
            }
            InvasionGame.spriteBatch.Draw(weaponsTex, position + weaponPos, weaponSrcRect, Color.White);

            // Ammo
            TextHandler.DrawText(laserAmmoText, position + laserAmmoPos);
            TextHandler.DrawText(photonAmmoText, position + photonAmmoPos, 1.0f, photonAmmoColor);

            // Shield
            if (shield > 0)
                InvasionGame.spriteBatch.Draw(shieldTex, position + shieldPos, shieldSrcRect, shieldColor);
            TextHandler.DrawTextCentered(shieldText, position + shieldTextPos);

            // Score
            TextHandler.DrawText(scoreText, position + scorePos);

            // Level
            TextHandler.DrawTextCentered(levelText, position + levelPos);
            TextHandler.DrawTextCentered(waveText, position + wavePos);
            TextHandler.DrawTextCentered(sectorText, position + sectorPos);

            // UFOs in Sector
            TextHandler.DrawTextCentered(sectorUFOsText, position + sectorUFOsPos);

            base.Draw(gameTime);
        }

    }
}
