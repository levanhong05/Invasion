using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImproviSoft.Drawing;
using ImproviSoft.System;

using Invasion.SpriteManagers;


namespace Invasion.Sprites {

    public class StarShip : DrawableGameComponent {

        public const int MaxShield = 50;

        const int ssRows = 4;
        const int ssCols = 5;
        const int shipTileSize = 70;

        const int ssExplosionRows = 3;
        const int ssExplosionCols = 7;

        Texture2D starshipTex = null;
        Texture2D starshipExplosionTex = null;

        Vector2 initialPos = Vector2.Zero;
        Vector2 position = Vector2.Zero;

        Rectangle srcRect = new Rectangle(0, 0, shipTileSize, shipTileSize);
        Rectangle destRect = new Rectangle(0, InvasionGame.SectorHeight-shipTileSize, shipTileSize, shipTileSize);

        SoundEffect skidSoundEffect = null;

        int ArgPosY = 0;
        int ArgPosX = 0;

        int incFrame = 70;

        private DispatcherTimer timerShoot = new DispatcherTimer();


        public bool IsAbleToShoot {
            get;
            private set;
        }

        public bool IsShooting {
            get;
            private set;
        }

        public Vector2 Position {
            get { return position; }
            set {
                position.X = value.X;
                destRect.X = PosX = (int)position.X;
            }
        }

        public int Shield {
            get { return shield; }

            internal set {
                shield = Math.Max(0, Math.Min(value, MaxShield));
                InvasionGame.Scoreboard.Shield = shield;
            }
        }
        private int shield = MaxShield;


        public Weapon Weapon {
            get { return weapon; }
            internal set {
                weapon = value;
                InvasionGame.Scoreboard.Weapon = weapon;
            }
        }
        private Weapon weapon = Weapon.Laser1;


        public Weapon MaxWeapon {
            get { return maxWeapon; }
            set {
                maxWeapon = value;
                InvasionGame.Scoreboard.MaxWeapon = maxWeapon;
                AutoSelectMaxWeapon();
            }
        }
        private Weapon maxWeapon = Weapon.Laser1;


        public int LaserAmmo {
            get { return laserAmmo; }
            internal set {
                laserAmmo = value;
                InvasionGame.Scoreboard.LaserAmmo = laserAmmo;
            }
        }
        private int laserAmmo = 100;


        public int PhotonAmmo {
            get { return photonAmmo; }
            internal set {
                photonAmmo = value;
                InvasionGame.Scoreboard.PhotonAmmo = photonAmmo;
            }
        }
        private int photonAmmo = 100;
        

        public int PosX {
            get;
            set;
        }

        public int SpeedX {
            get;
            private set;
        }


        public Rectangle BoundingBox {
            get {
                boundingBox.X = destRect.X+13;
                boundingBox.Y = destRect.Y+11;
                return boundingBox;
            }
        }
        Rectangle boundingBox = new Rectangle(0, 0, shipTileSize-20, shipTileSize-26);


        public ShipState ShipState {
            get { return shipState; }
            set { shipState = value; }
        }
        ShipState shipState = ShipState.OK;


        public static bool AutoSelectWeapon {
            get { return autoSelectWeapon; }
            set {
                autoSelectWeapon = value;
            }
        }
        private static bool autoSelectWeapon = true;


        public StarShip(Game game) : base(game) {
            LaserAmmo = 100;
            Reset();
        }


        public void Reset() {
            LaserAmmo = 100;
            PhotonAmmo = 0;

            Weapon = Weapon.Laser1;
            MaxWeapon = Weapon.Laser1;

            Shield = MaxShield;
        }


        public void InitLevel() {
            AutoSelectMaxWeapon();

            IsAbleToShoot = true;
            IsShooting = false;
        
            ShipState = ShipState.OK;
            SpeedX = 0;
            PosX = (int)InvasionGame.SectorPos.X + (InvasionGame.SectorWidth - shipTileSize)/2;
        }


        public bool WeaponIsLaser {
            get { return Weapon <= Weapon.Laser3; }
        }

        public bool WeaponIsPhoton {
            get { return Weapon >= Weapon.Photon1; }
        }


        private void AutoSelectMaxWeapon() {
            if (!autoSelectWeapon)
                return;

            Weapon lastWeapon = Weapon;

            Weapon = MaxWeapon;

            if (WeaponIsPhoton) {
                if (PhotonAmmo < 3) {
                    if (PhotonAmmo == 0) {
                        Weapon = Weapon.Laser3;
                    }
                    else if (Weapon != Weapon.Photon1) {
                        Weapon = (PhotonAmmo == 2)? Weapon.Photon2 : Weapon.Photon1;
                    }
                }
            }
            if (WeaponIsLaser) {
                if (LaserAmmo < 3) {
                    if (LaserAmmo == 0)
                        Weapon = Weapon.Laser1; // use shield power to fire single shot laser
                    else if (Weapon != Weapon.Laser1) {
                        Weapon = (LaserAmmo == 2)? Weapon.Laser2 : Weapon.Laser1;
                    }
                }
            }

            if (Weapon != lastWeapon && InvasionGame.weaponChangeSoundEffect != null)
                SoundManager.Instance.Play(InvasionGame.weaponChangeSoundEffect);
        } 


        protected override void LoadContent() {
            skidSoundEffect = Game.Content.Load<SoundEffect>("Sounds/skid");

            starshipTex = Game.Content.Load<Texture2D>("Sprites/ship");
            starshipExplosionTex = Game.Content.Load<Texture2D>("Sprites/shipexplode");

            initialPos = new Vector2(InvasionGame.SectorPos.X + (InvasionGame.SectorWidth-shipTileSize)/2, InvasionGame.SectorHeight-shipTileSize);
            position = initialPos;

            timerShoot.Tick += timerShoot_Tick;

            base.LoadContent();
        }


        public static int GetShieldDamageForHit() {
            int shieldDamage = 0;

            switch (InvasionGame.Scoreboard.DifficultyLevel) {
                case DifficultyLevel.Easy:
                    shieldDamage = 10;
                    break;

                case DifficultyLevel.Medium:
                    shieldDamage = 15;
                    break;

                case DifficultyLevel.Hard:
                    shieldDamage = 20; // the original Invasion Game spec'd 20
                    break;
            }
            return shieldDamage;
        }


        public override void Update(GameTime gameTime) {

            if (Invasion.CurrentScreen != GameScreen.GameplayScreen)
                return;

            int sectorPosX = (int)InvasionGame.SectorPos.X;

            PosX += SpeedX;

            if (PosX <= sectorPosX) {
                SpeedX = 0;
                PosX = sectorPosX;
            }
            if (PosX >= sectorPosX + InvasionGame.SectorWidth - shipTileSize) {
                SpeedX = 0;
                PosX = sectorPosX + InvasionGame.SectorWidth - shipTileSize;
            }

            position.X = PosX;
            destRect.X = PosX;

            srcRect = new Rectangle(ArgPosX, ArgPosY, shipTileSize, shipTileSize);

            if (shipState == ShipState.OK && BulletsManager.Instance.CheckForHitShip(this)) {
                Shield -= GetShieldDamageForHit();

                if (Shield > 0)
                    SoundManager.Instance.Play(InvasionGame.damageSoundEffect);
                else
                    SoundManager.Instance.Play(InvasionGame.explosionSoundEffect);

                int intensity = (MaxShield - Shield)/6; //(50-30)/6=2; (50-10)/6=6; (50+10)/6=10;
                //Debug.WriteLine("Camera2D.Instance.Shake(750, " + intensity + ") called; Shield = " + Shield);
                Camera2D.Instance.Shake(750, intensity);

                VibrationManager.Vibrate(PlayerIndex.One, 1, 0, 0.35f);
            }

            if (Shield == 0 && shipState == ShipState.OK) {
                shipState = ShipState.Exploding;
                SpeedX = 0;

                ArgPosX = 0;
                ArgPosY = 0;
                incFrame = shipTileSize;
            }

            if (shipState == ShipState.OK)
                ExtrasManager.Instance.CheckForShipAward(this);
            
            switch (shipState) {

                case ShipState.OK:
                    ArgPosX += incFrame;

                    if (ArgPosX == ssCols*shipTileSize && incFrame > 0) {
                        ArgPosX = 0;
                        ArgPosY += incFrame;

                        if (ArgPosY == ssRows*shipTileSize) {
                            ArgPosX = (ssCols-1)*shipTileSize;
                            ArgPosY = (ssRows-1)*shipTileSize;

                            incFrame = -shipTileSize;
                            return;
                        }
                    }

                    if (ArgPosX == -shipTileSize && incFrame < 0) {
                        ArgPosX = (ssCols-1)*shipTileSize;
                        ArgPosY += incFrame;

                        if (ArgPosY == -shipTileSize) {
                            incFrame = shipTileSize;
                            ArgPosX = 0;
                            ArgPosY = 0;
                        }
                    }
                    break;

                case ShipState.Exploding:
                    ArgPosX += incFrame;
                    if (ArgPosX == ssExplosionCols*shipTileSize && incFrame > 0) {
                        ArgPosX = 0;
                        ArgPosY += incFrame;

                        if (ArgPosY == ssExplosionRows*shipTileSize) {
                            shipState = ShipState.Destroyed;
                            return;
                        }
                    }
                    break;
            }
        }


        public override void Draw(GameTime gameTime) {

            Texture2D drawTex = (ShipState == ShipState.OK)? starshipTex : starshipExplosionTex;

            InvasionGame.spriteBatch.Draw(drawTex, destRect, srcRect, Color.White);

            //SimpleShapes.DrawRectangle(InvasionGame.spriteBatch, 2, Color.Red, BoundingBox);

            base.Draw(gameTime);

            IsShooting = false;
        }


        internal void StopMoving() {
            SpeedX = 0;
        }


        internal void SkidToAStop() {
            if (SpeedX != 0) {
                SpeedX = 0;
                Skid();
            }
        }


        internal void Skid() {
            SoundManager.Instance.Play(skidSoundEffect);
        }


        internal void MoveLeft(int speed) {
            SpeedX = -speed;
        }


        internal void MoveRight(int speed) {
            SpeedX = speed;
        }


        internal void SelectNextWeapon() {
            Weapon prevWeapon = Weapon;

            if (Weapon == MaxWeapon)
                Weapon = Weapon.Laser1;
            else
                Weapon++;

            if (prevWeapon != Weapon)
                SoundManager.Instance.Play(InvasionGame.weaponChangeSoundEffect);
        }


        internal void FireWeapon() {

            if (!IsAbleToShoot)
                return;

            // Start shooting flag
            IsShooting = true;

            int centerGunY = destRect.Y + 9;
            int outerGunsY = destRect.Y + 14;

            // Check the weapon we are using
            switch (Weapon) {
                case Weapon.Laser1:
                    // using single laser

                    // if we are out of ammo, get energy from the shields
                    if (LaserAmmo == 0)
                        Shield -= 1;
                    else
                        LaserAmmo--;

                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+27, centerGunY), BulletSource.LaserBlaster));
                    break;

                case Weapon.Laser2:

                    // If we are out of ammo, not shoot
                    if (LaserAmmo < 2) {
                        AutoSelectMaxWeapon();
                        return;
                    }

                    LaserAmmo -= 2;

                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+16, outerGunsY), BulletSource.LaserBlaster));
                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+38, outerGunsY), BulletSource.LaserBlaster));
                    break;

                case Weapon.Laser3:

                    // If we are out of ammo, not shoot
                    if (LaserAmmo < 3) {
                        AutoSelectMaxWeapon();
                        return;
                    }

                    LaserAmmo -= 3;
                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+16, outerGunsY), BulletSource.LaserBlaster));
                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+27, centerGunY), BulletSource.LaserBlaster));
                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+38, outerGunsY), BulletSource.LaserBlaster));
                    break;

                case Weapon.Photon1:
                    if (PhotonAmmo == 0) {
                        AutoSelectMaxWeapon();
                        return;
                    }

                    PhotonAmmo--;

                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+30, centerGunY), BulletSource.PhotonBlaster));
                    break;

                case Weapon.Photon2:
                    if (PhotonAmmo < 2) {
                        AutoSelectMaxWeapon();
                        return;
                    }

                    PhotonAmmo -= 2;

                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+16, outerGunsY), BulletSource.PhotonBlaster));
                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+38, outerGunsY), BulletSource.PhotonBlaster));
                    break;

                case Weapon.Photon3:

                    if (PhotonAmmo < 3) {
                        AutoSelectMaxWeapon();
                        return;
                    }

                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+16, outerGunsY), BulletSource.PhotonBlaster));
                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+27, centerGunY), BulletSource.PhotonBlaster));
                    BulletsManager.Instance.Add(new Bullet(Game, new Vector2(PosX+38, outerGunsY), BulletSource.PhotonBlaster));

                    break;
            }

            SoundManager.Instance.Play(InvasionGame.blasterShootSoundEffect);
            IsAbleToShoot = false;

            if (Weapon == Weapon.Laser1)
                timerShoot.Interval = System.TimeSpan.FromMilliseconds(400);
            else
                timerShoot.Interval = System.TimeSpan.FromMilliseconds(680);

            timerShoot.Start();
        }


        public bool Intersects(Vector2 pt) {
            return
                pt.X >= position.X && pt.X <= position.X + destRect.Width &&
                pt.Y >= position.Y && pt.Y <= position.Y + destRect.Height;
        }


        /// <summary>
        /// Handler for the Timer Events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerShoot_Tick(object sender, System.EventArgs e) {
            timerShoot.Stop();
            IsAbleToShoot = true;
        }


    }
}
