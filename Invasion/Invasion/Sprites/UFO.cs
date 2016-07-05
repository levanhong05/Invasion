using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImproviSoft.Diagnostics;
using ImproviSoft.Drawing;
using ImproviSoft.System;

using Invasion.SpriteManagers;

namespace Invasion.Sprites {

    public enum UFOModel {SilverUFO, OrangeUFO, RedUFO }


    public class UFO : DrawableGameComponent {

        const int ufoTileSize = 70;

		private int		x = 0;
		private int		y = 0;
		private int		maneuver = 0;
		private int		currentAltitudeDrop = 0;
		private int		speed = 1;
		private Extra.ExtraType extraType = Extra.ExtraType.NotSet;

        private Rectangle destRect = Rectangle.Empty;
        private Rectangle srcRect = Rectangle.Empty;

		private int		ArgPosX = 0;
		private int		ArgPosY = 0;
		private int		iExplosionFrame = 0;
		private int		altitudeTargetDrop = 80;
		private ShipState	iState = ShipState.OK;
		private int		strength = 1;
        private UFOModel ufoModel = UFOModel.SilverUFO;

        int	msDelayShoot = 0;
        private TimeSpan timeSinceLastShot = TimeSpan.Zero;

		private Random rand = new Random();

		private int		iDiffRight = 0;
		private int		iDiffLeft = 0;

        Texture2D drawTex = null;



		public UFO(Game game, UFOModel ufoModel, Vector2 position, int strength) : base(game)
		{
            this.ufoModel = ufoModel;

            SetXY((int)position.X, (int)position.Y);

            this.strength = strength;


			timeSinceLastShot = TimeSpan.FromMilliseconds(1000 + RandomManager.Instance.Next(500));

            int factor = 30;

            switch (InvasionGame.Scoreboard.DifficultyLevel) {
                case DifficultyLevel.Easy:
                    factor = 50;
                    break;

                case DifficultyLevel.Medium:
                    factor = 35;
                    break;

                case DifficultyLevel.Hard:
                    factor = 30;
                    break;
            }

            switch (ufoModel) {
                case UFOModel.SilverUFO:
                    msDelayShoot = 100*factor;
                    break;

                case UFOModel.OrangeUFO:
                    msDelayShoot = 10*factor;
                    break;

                case UFOModel.RedUFO:
                    msDelayShoot = 70*factor;
                    break;
            }

            extraType = SelectExtra();
		}


        public Extra.ExtraType SelectExtra() {

            int random = RandomManager.Instance.Next(150);

            // Original Invasion Game Chances: 32% (48/150) of UFOs have extras
            int weaponsBonusChances = 3; // original game didn't allow 2 consecutive Weapons bonuses; this version does
            int pointsBonusChances = 10;
            int photonAmmoBonusChances = 14;
            int laserAmmoBonusChances = 19;
            int shieldBonusChances = 2;

            switch (InvasionGame.Scoreboard.DifficultyLevel) {
                case DifficultyLevel.Easy: // 42% (63/150) of UFOs have extras
                    weaponsBonusChances = 6;
                    pointsBonusChances = 13;
                    photonAmmoBonusChances = 16;
                    laserAmmoBonusChances = 22;
                    shieldBonusChances = 6;
                    break;

                case DifficultyLevel.Medium: // 36% (54/150) of UFOs have extras
                    weaponsBonusChances = 4;
                    pointsBonusChances = 11;
                    photonAmmoBonusChances = 15;
                    laserAmmoBonusChances = 20;
                    shieldBonusChances = 4;
                    break;

                case DifficultyLevel.Hard: // 30% (45/150) of UFOs have extras
                    weaponsBonusChances = 3;
                    pointsBonusChances = 10;
                    photonAmmoBonusChances = 12;
                    laserAmmoBonusChances = 17;
                    shieldBonusChances = 3;
                    break;
            }

            Extra.ExtraType extraType = Extra.ExtraType.NotSet;

            int slot = 0;

            if (random < (slot += weaponsBonusChances)) {
                extraType = Extra.ExtraType.Weapons;
            }
            else if (random < (slot += pointsBonusChances)) {
                extraType = Extra.ExtraType.Hundred;
            }
            else if (random < (slot += photonAmmoBonusChances)) {
                extraType = Extra.ExtraType.PhotonAmmo;
            }
            else if (random < (slot += laserAmmoBonusChances)) {
                extraType = Extra.ExtraType.BlasterAmmo;
            }
            else if (random < (slot += shieldBonusChances)) {
                extraType = Extra.ExtraType.Shield;
            }

            Debug.WriteLine("UFO ExtraType: " + extraType);
            return extraType;
		}


		public void SetXY(int xpos, int ypos)
		{
			x = xpos;
			y = ypos;
			
            int sectorRightX = (int)InvasionGame.SectorPos.X + (int)InvasionGame.SectorWidth;
            int sectorBottomY = (int)InvasionGame.SectorPos.Y + (int)InvasionGame.SectorHeight;

            if (x + 70 > sectorRightX)
                x = sectorRightX - 70;

            if (y + 70 > sectorBottomY)
                y = sectorBottomY - 70;

            destRect = new Rectangle(x,y,70,70);
		}


		public int Speed
		{
			get { return speed; }
			set {
				speed = value;
			}
		}


		public ShipState State
		{
			get { return iState; }
			set {
				iState = value;
			}
		}


		public int Maneuver
		{
			get { return maneuver; }
			set {
				maneuver = value;
			}
		}


		public UFOModel UFOModel
		{
			get { return ufoModel; }
		}


		public int Strength
		{
			get { return strength; }
			set {
				strength = value;
			}
		}


		public int X
		{
			get { return x; }
			set {
				x = value;
			}
		}		


		public int Y
		{
			get { return y; }
			set {
				y = value;
			}
		}		


		public int AltitudeTargetDrop
		{
			get { return altitudeTargetDrop; }
			set {
				altitudeTargetDrop = value;
			}
		}


		public void Damaged(int bulletPower)
		{
            strength -= bulletPower;

            if (Strength <= 0) {
                InvasionGame.Scoreboard.Score += 100 * InvasionGame.Scoreboard.Wave;
                SoundManager.Instance.Play(InvasionGame.explosionSoundEffect);
                State = ShipState.Exploding;
            }
            else {
                InvasionGame.Scoreboard.Score += 10 * InvasionGame.Scoreboard.Wave;
                SoundManager.Instance.Play(InvasionGame.damageSoundEffect);
            }
		} 				


		public bool IsExploding 
		{
            get { return iState == ShipState.Exploding; }
		}


        public override void Update(GameTime gameTime) {

            if (State == ShipState.OK) {

                drawTex = UFOsManager.ufoTex;

                if (Invasion.starShip.IsShooting) {
                    Move(Invasion.starShip.PosX);
                }
                else
                    Move(-1);

                UpdateUFO();

                BulletsManager.Instance.CheckForHitUFO(this);

                timeSinceLastShot += gameTime.ElapsedGameTime;

                if (timeSinceLastShot.TotalMilliseconds > msDelayShoot) {
                    if (UFOsManager.Instance.IsTimeToShoot) {
                        timeSinceLastShot = TimeSpan.Zero;

                        BulletsManager.Instance.Add(new Bullet(Game, new Vector2(destRect.X+24, destRect.Y+50), BulletSource.UfoPhotonBlaster));
                        SoundManager.Instance.Play(InvasionGame.ufoShootSoundEffect);
                    }
                }
            }
            else if (State == ShipState.Exploding) {

                if (iExplosionFrame < 20) {
                    drawTex = UFOsManager.explosionTex;

                    UpdateUFO();

                    if (IsTimeToDropExtra()) {
                        Extra extra = new Extra(Game, new Vector2(X+22, Y+20), (Extra.ExtraType)extraType, -3);
                        ExtrasManager.Instance.Add(extra);
                    }
                }
                else {
                    State = ShipState.Destroyed;
                }
            }
        }


		public void UpdateUFO()
		{
			srcRect = new Rectangle(ArgPosX,ArgPosY,70,70);

            int sectorLeftX = (int)InvasionGame.SectorPos.X;
            int sectorRightX = sectorLeftX + InvasionGame.SectorWidth;

            if (x+70 >= sectorRightX) {
                iDiffRight = (x+70) - sectorRightX;
			}
			else
				iDiffRight = 0;

            if (x < sectorLeftX) {
                iDiffLeft = sectorLeftX - x;
			}
			else
                iDiffLeft = 0;

            switch (iState) {
                case ShipState.OK:
                    break;

                case ShipState.Exploding:
                    if (iExplosionFrame < 5) {
                        ArgPosX = iExplosionFrame * 70;
                        ArgPosY = 0;
                    }
                    if (iExplosionFrame > 4 && iExplosionFrame < 9) {
                        ArgPosX = (iExplosionFrame-5) * 70;
                        ArgPosY = 70;
                    }
                    if (iExplosionFrame > 9 && iExplosionFrame < 14) {
                        ArgPosX = (iExplosionFrame-10) * 70;
                        ArgPosY = 140;
                    }
                    if (iExplosionFrame > 14 && iExplosionFrame < 20) {
                        ArgPosX = (iExplosionFrame-15) * 70;
                        ArgPosY = 210;
                    }

                    iExplosionFrame++;
                    break;
            }

			srcRect = new Rectangle(ArgPosX+iDiffLeft, ArgPosY, 70-iDiffRight-iDiffLeft, 70);
            destRect.Width = srcRect.Width;
            destRect.Height = srcRect.Height;
	
			ArgPosX += 70;

			if (ArgPosX == 350)
			{
				ArgPosX = 0;
				ArgPosY += 70;

				if (ArgPosY == 700)
					ArgPosY = 0;
			}
		}


        public override void Draw(GameTime gameTime) {

            if (State != ShipState.Destroyed) 
                InvasionGame.spriteBatch.Draw(drawTex, destRect, srcRect, Color.White);

            //These are for DEBUG ONLY
            //InvasionGame.spriteBatch.DrawString(FrameRateCounter.font, Str.ToString(), new Vector2(x, y), Color.Red);
            //InvasionGame.spriteBatch.DrawString(FrameRateCounter.font, mov.ToString(), new Vector2(x, y), Color.Red);
            //SimpleShapes.DrawRectangle(InvasionGame.spriteBatch, 2.0f, Color.Red, BoundingBox);
        }


		public void Move(int iFlags)
		{
            int sectorPosX = (int)InvasionGame.SectorPos.X;

            switch (ufoModel) {
                case UFOModel.SilverUFO:
                    switch (maneuver) {
                        case 0:
                            x += speed;

                            if (y > 300) {
                                if (speed > 0) {
                                    if (x > sectorPosX + 650) {
                                        x = sectorPosX + 11;
                                        y = -70;
                                        maneuver = 1;
                                        speed = 1;
                                        currentAltitudeDrop = 0;
                                    }
                                }
                                else {
                                    if (x < sectorPosX - 80) {
                                        x = sectorPosX + 560;
                                        y = -70;
                                        maneuver = 1;
                                        speed = 1;
                                        currentAltitudeDrop = 0;
                                    }
                                }
                            }
                            else {
                                if (x == sectorPosX+560 || x == sectorPosX+10) {
                                    currentAltitudeDrop = 0;
                                    maneuver = 1;
                                    speed = 1;
                                }
                            }
                            break;

                        case 1:
                            y += speed;
                            currentAltitudeDrop += speed;

                            if (y > 0)
                                if (currentAltitudeDrop >= altitudeTargetDrop) {
                                    maneuver = 0;

                                    if (x <= sectorPosX+10) { //DRC changed from +0 to +10
                                        speed = Math.Abs(speed);
                                    }

                                    if (x >= sectorPosX+560) {
                                        speed = -Math.Abs(speed);
                                    }
                                }
                            break;
                    }
                    break;

                case UFOModel.OrangeUFO:

                    switch (maneuver) {
                        case 0:
                            x -= speed;
                            y -= speed;

                            if (x <= sectorPosX) {
                                x = sectorPosX;
                                maneuver = 1;
                            }
                            if (y <= 0) {
                                y = 0;
                                maneuver = 3;
                            }
                            break;

                        case 1:
                            x += speed;
                            y -= speed;

                            if (x >= sectorPosX+560) {
                                x = sectorPosX+560;
                                maneuver = 0;
                            }
                            if (y <= 0) {
                                y = 0;
                                maneuver = 2;
                            }
                            break;

                        case 2:
                            x += speed;
                            y += speed;

                            if (x >= sectorPosX+560) {
                                x = sectorPosX+560;
                                maneuver = 3;
                            }
                            if (y >= 300) {
                                y = 300;
                                maneuver = 1;
                            }
                            break;

                        case 3:
                            x -= speed;
                            y += speed;

                            if (x <= sectorPosX) {
                                x = sectorPosX;
                                maneuver = 2;
                            }
                            if (y >= 300) {
                                y = 300;
                                maneuver = 0;
                            }
                            break;
                    }
                    break;

                case UFOModel.RedUFO:

                    switch (maneuver) {
                        case 0:
                            if (x <= iFlags) {
                                if (iFlags != -1)
                                    maneuver = 1;
                                else
                                    x += speed;
                            }
                            else {
                                x += speed;
                            }

                            if (x >= sectorPosX+650) {
                                x = sectorPosX-80;
                                maneuver = 0;
                            }
                            break;

                        case 1:
                            if (x >= iFlags) {
                                if (iFlags != -1)
                                    maneuver = 0;
                                else
                                    x -= speed;
                            }
                            else {
                                x -= speed;
                            }

                            if (x <= sectorPosX-80) {
                                x = sectorPosX+650;
                                maneuver = 1;
                            }
                            break;
                    }
                    break;
            }

            destRect.X = x;
            destRect.Y = y;

#if DEBUG
            if (x < (int)InvasionGame.SectorPos.X-80 || x > (int)InvasionGame.SectorPos.X + InvasionGame.SectorWidth + 80) {
                int i = UFOsManager.Instance.GetIndex(this);
                Debug.WriteLine(DateTime.Now + ": UFO[" + ((i<10)?"0":"") + i + "]  State: " + State + ", Pos = <" + X + ", " + Y + ">, Model: " + UFOModel + ", currentAltitudeDrop: " + currentAltitudeDrop + " / " + AltitudeTargetDrop + ", Maneuver: " + Maneuver + ", Speed: " + Speed + ", Strength: " + strength + ", BoundingBox: " + BoundingBox + ", DestRect: " + destRect);
            }
#endif
		}


		public void Bounce()
		{
			switch (ufoModel)
			{
                case UFOModel.SilverUFO:
					speed = -speed;
					break;

                case UFOModel.RedUFO:
					maneuver = (maneuver == 1)? 0 : 1;
					break;
			}
		}


		public bool IsTimeToDropExtra()
		{
            return  iState == ShipState.Exploding && iExplosionFrame == 12 && extraType != Extra.ExtraType.NotSet;
		}


        public Rectangle BoundingBox {
            get {
                boundingBox.X = destRect.X+1;
                boundingBox.Y = destRect.Y+12;
                return boundingBox;
            }
        }
        Rectangle boundingBox = new Rectangle(0, 0, ufoTileSize-2, ufoTileSize-24);

	}
}
