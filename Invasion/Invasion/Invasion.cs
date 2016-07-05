using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

using ImproviSoft.Diagnostics;
using ImproviSoft.Drawing;
using ImproviSoft.System;

using Invasion.Sprites;
using Invasion.SpriteManagers;


namespace Invasion
{
    public enum DifficultyLevel { Easy, Medium, Hard }
	public enum GameScreen { MainMenuScreen, OptionsScreen, LevelScreen, GameplayScreen, GameOverScreen, CreditsScreen, HelpScreen, PauseScreen };

	/// <summary>
	/// The Invasion class currently is being used as a ScreenManager and implements all of the game screens.
    /// In future versions I plan to actually replace this with a ScreenManager class and separate game screen classes.
	/// </summary>
	public class Invasion : DrawableGameComponent
	{
		// Define the application states
		const int appMainMenu = 0;
		const int appGameScreen = 1;
		const int appCredits = 2;
		const int appHelpScreen = 3;

        Texture2D title_bgTex = null;
        Texture2D backgroundTex = null;

        Texture2D selectedMenuItemSS = null;


        public static GameScreen CurrentScreen {
            get { return currentScreen; }
        }
        static GameScreen prevFrameScreen = GameScreen.GameOverScreen;
        static GameScreen currentScreen = GameScreen.MainMenuScreen;

		int iOption = 0;
		int iSelectFrame = 0;

        internal static StarShip starShip;

		int iLevel = 0;

		// setup the delay variables and constants;
		int tickLast = 0; // Holds the value of the last call to GetTick.
		int delayStart = 0;
		int delayOffset = 0;

		const int delayTime = 18;
		const int delayLevel = 3000;
        const int delayCredits = 30000;
        const int delayHelp = 30000;
        const int delayGameOver = 20000;
		const int delayBlink = 800;

        const int optionsTitleY = 40;
        const int optionsFirstMenuItemY = 110;
        const int optionsMenuItemDeltaY = 45;
        const int optionsMenuNumItems = 2;
        const int options2ndSubmenuY = optionsFirstMenuItemY + 9 * optionsMenuItemDeltaY / 2;

        const int pauseMenuItemDeltaY = 50;

        const int helpScreenExtrasFirstRowY = 110;
        const int helpScreenExtrasLineSpacing = 30;

        readonly Color versionDimColor = new Color(80, 80, 80);

        SoundEffect menuSoundEffect = null; // when menu selection changes

        private AccelerometerState accelerometerState;
        InputState input = new InputState();

        bool waitForRelease = false;

        public static bool GameInProgress {
            get;
            private set;
        }
        

		/// <summary>
		/// Constructor for the Invasion main class
		/// </summary>
		public Invasion(Game game) : base(game) 
        {
            GameInProgress = false;

            // Initialize the static Instance 
            Game.Components.Add(new BulletsManager(Game));
            Game.Components.Add(new ExtrasManager(Game));
            Game.Components.Add(new UFOsManager(Game));

            starShip = new StarShip(Game);
            Game.Components.Add(starShip);

            ContentLoader.LoadContent(Game.Content);

            TouchPanel.EnabledGestures = GestureType.None;
		}


        protected override void LoadContent() {
            base.LoadContent();

            TextHandler.LoadContent(Game.Content);

            title_bgTex = Game.Content.Load<Texture2D>("Backgrounds/title_bg");
            backgroundTex = Game.Content.Load<Texture2D>("Backgrounds/starfield_bg");

            selectedMenuItemSS = Game.Content.Load<Texture2D>("Sprites/select");
            menuSoundEffect = Game.Content.Load<SoundEffect>("Sounds/tap");
        }


        public override void Update(GameTime gameTime) {

            input.Update();

            if (currentScreen != prevFrameScreen) {
                SetupGestures();

                if (input.TouchState.Count > 0)
                    waitForRelease = true;

                // Don't allow any gestures generated on previous screen to be handled by the new one
                while (input.Gestures.Count > 0)
                    input.Gestures.Clear();

                if (currentScreen == GameScreen.MainMenuScreen) {
                    GameInProgress = false;
                    iOption = 0;
                    UFOsManager.Instance.Reset();
                }

                prevFrameScreen = currentScreen;
            }

            HandleInput(gameTime);
            
            if (currentScreen == GameScreen.GameplayScreen) {
                if (starShip.ShipState == ShipState.Destroyed) {
                    currentScreen = GameScreen.GameOverScreen;
                    GameInProgress = false;
                    delayStart = Environment.TickCount;
                    SoundManager.Instance.Play(InvasionGame.gameOverSoundEffect);
                }
            }

            starShip.Update(gameTime);

            tickLast = Environment.TickCount;
		}


        /// <summary>
        /// Define the gestures that will be enabled for each Game Screen
        /// </summary>
        private void SetupGestures() {
            switch (currentScreen) {
                case GameScreen.MainMenuScreen:
                    TouchPanel.EnabledGestures = GestureType.Tap;
                    break;

                case GameScreen.GameplayScreen:
                    TouchPanel.EnabledGestures =
                        GestureType.Flick |
                        GestureType.Hold |
                        GestureType.HorizontalDrag |
                        GestureType.Tap;
                    break;

                case GameScreen.LevelScreen:
                    TouchPanel.EnabledGestures = GestureType.None;
                    break;

                case GameScreen.GameOverScreen:
                    TouchPanel.EnabledGestures = GestureType.None;
                    break;

                case GameScreen.CreditsScreen:
                    TouchPanel.EnabledGestures = GestureType.Tap;
                    break;

                case GameScreen.HelpScreen:
                    TouchPanel.EnabledGestures = GestureType.Tap;
                    break;
                    
                case GameScreen.OptionsScreen:
                    TouchPanel.EnabledGestures = GestureType.Tap;
                    break;

                case GameScreen.PauseScreen:
                    TouchPanel.EnabledGestures = GestureType.Tap;
                    break;

                default:
                    TouchPanel.EnabledGestures = GestureType.None;
                    break;
            }
        }


		/// <summary>
		/// Draw the sprites and text to the screen.
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
            Vector2 pos = Vector2.Zero;

            Viewport viewport = InvasionGame.graphics.GraphicsDevice.Viewport;
            Vector2 centerPos = new Vector2(viewport.Width, viewport.Height)/2;
            float horizCenter = centerPos.X;

			// Fill the back buffer with the appropriate things.
			switch (currentScreen)
			{
				case GameScreen.MainMenuScreen:
                    const int itemHeight = 40;

				    InvasionGame.spriteBatch.Draw(title_bgTex, pos, Color.White);
                    pos = new Vector2(130,260);

					TextHandler.DrawText("START GAME", pos, 2.0f, Color.White); pos.Y += itemHeight;
					TextHandler.DrawText("OPTIONS", pos, 2.0f, Color.White); pos.Y += itemHeight;
					TextHandler.DrawText("CREDITS", pos, 2.0f, Color.White); pos.Y += itemHeight;
					TextHandler.DrawText("HELP", pos, 2.0f, Color.White); pos.Y += itemHeight;
                    TextHandler.DrawText("QUIT", pos, 2.0f, Color.White);

                    pos.X = 700;//centerPos.X;
                    pos.Y = 460;
                    TextHandler.DrawTextCentered("VERSION 1.1", pos, 1.0f, versionDimColor); 

					Rectangle srcRect = new Rectangle(iSelectFrame * 32,0,32,20);
                    pos = new Vector2(75, 260+(itemHeight * iOption));
                    InvasionGame.spriteBatch.Draw(selectedMenuItemSS, pos, srcRect, Color.White, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.0f);

					iSelectFrame = ++iSelectFrame % 20;
					break;

				case GameScreen.GameplayScreen:

                    InvasionGame.spriteBatch.Draw(backgroundTex, Vector2.Zero, Color.White);

                    InvasionGame.Scoreboard.Draw(gameTime);

					if (UFOsManager.Instance.Count == 0 && ExtrasManager.Instance.Count == 0)
					{
						SoundManager.Instance.Play(InvasionGame.startGameSoundEffect);
						InitLevel();
						return;
					}
					
                    ExtrasManager.Instance.Draw(gameTime);
                    BulletsManager.Instance.Draw(gameTime);
					UFOsManager.Instance.Draw(gameTime);
                    starShip.Draw(gameTime);
					break;

                case GameScreen.LevelScreen:
                    InvasionGame.spriteBatch.Draw(backgroundTex, Vector2.Zero, Color.White);

                    int screenWidth = InvasionGame.graphics.GraphicsDevice.Viewport.Width;
					
					char padding = '0';
					string level = iLevel.ToString().PadLeft(3,padding);

                    TextHandler.DrawTextCentered("LEVEL " + level, new Vector2(screenWidth/2, 160.0f), 2.0f, Color.White);
                    TextHandler.DrawTextCentered("WAVE " + InvasionGame.Scoreboard.Wave + ", SECTOR " + InvasionGame.Scoreboard.Sector, new Vector2(screenWidth/2, 240.0f), 2.0f, Color.White);

					if (delayStart + delayLevel < tickLast)
						currentScreen = GameScreen.GameplayScreen;
					break;

                case GameScreen.GameOverScreen:

                    InvasionGame.spriteBatch.Draw(backgroundTex, Vector2.Zero, Color.White);

                    float sectorCenter = InvasionGame.SectorPos.X + InvasionGame.SectorWidth/2;

                    pos.X = sectorCenter;
                    pos.Y = 200;
					
                    TextHandler.DrawTextCentered("GAME OVER", pos, 2.0f, Color.White);

                    InvasionGame.Scoreboard.Draw(gameTime);

                    if (((tickLast - delayStart) / delayBlink) % 3 != 1) {
                        pos.Y = 420;
                        TextHandler.DrawTextCentered("NHAN PHIM BACK", pos);
                        pos.Y += 30;
                        TextHandler.DrawTextCentered("DE TRO VE MENU CHINH", pos);
                    }
					break;

                case GameScreen.CreditsScreen:

                    InvasionGame.graphics.GraphicsDevice.Clear(Color.Black);

                    int yPos = 35;
                    int lineSpacing = 30;

                    TextHandler.DrawTextCentered("GAME THE UFO INVASION CHO WINDOWS PHONE", new Vector2(horizCenter, yPos)); yPos += 3* lineSpacing / 2;

                    TextHandler.DrawTextCentered("HAY GUI NHUNG THAC MAC HOAC Y KIEN CUA BAN", new Vector2(horizCenter, yPos)); yPos += lineSpacing;
                    TextHandler.DrawTextCentered("VE SAN PHAM CHO CHUNG TOI QUA DIA CHI SAU:", new Vector2(horizCenter, yPos)); yPos += 3 * lineSpacing / 2;

                    TextHandler.DrawTextCentered("WINDOWS PHONE - XNA VERSION, 2013", new Vector2(horizCenter, yPos)); yPos += lineSpacing;
                    TextHandler.DrawTextCentered("LEVANHONG05@GMAIL.COM", new Vector2(horizCenter, yPos)); yPos += lineSpacing; 

                    if (((tickLast - delayStart) / delayBlink) % 2 == 1) {
                        pos.X = centerPos.X;
                        pos.Y = 450;
                        TextHandler.DrawTextCentered("NHAN VAO MAN HINH DE TIEP TUC", pos);
                    }

					if (delayStart + delayCredits < tickLast)
						currentScreen = GameScreen.MainMenuScreen;
					break;

				case GameScreen.HelpScreen:

                    InvasionGame.graphics.GraphicsDevice.Clear(Color.Black);

                    pos = new Vector2(centerPos.X, 40);
                    TextHandler.DrawTextCentered("HELP", pos, 2.0f, Color.White); pos.Y += 36;
                    TextHandler.DrawTextCentered("VERSION 1.1", pos, 1.0f, versionDimColor); 
                    
                    yPos = helpScreenExtrasFirstRowY;

                    pos = new Vector2(280, yPos);
                    lineSpacing = helpScreenExtrasLineSpacing;

					TextHandler.DrawText("PHOTON AMMO BONUS", pos); pos.Y += lineSpacing;
					TextHandler.DrawText("WEAPON UPGRADE BONUS",pos); pos.Y += lineSpacing;
					TextHandler.DrawText("100X WAVE POINTS BONUS",pos); pos.Y += lineSpacing;
					TextHandler.DrawText("LASER AMMO BONUS",pos);	 pos.Y += lineSpacing;
					TextHandler.DrawText("SHIELD CHARGE BONUS",pos);
                    
                    pos.X = centerPos.X; pos.Y += 3 * lineSpacing / 2;

					TextHandler.DrawTextCentered("TILT SCREEN OR FLICK SHIP - MOVE SHIP", pos);	 pos.Y += lineSpacing;
					TextHandler.DrawTextCentered("DRAG SHIP - MOVE AND THEN STOP SHIP", pos);  pos.Y += lineSpacing;
					TextHandler.DrawTextCentered("TAP SCREEN - FIRE WEAPON", pos);  pos.Y += lineSpacing;
                    TextHandler.DrawTextCentered("TAP WEAPON - CHANGE WEAPON IF AVAILABLE", pos);  pos.Y += lineSpacing;
                    TextHandler.DrawTextCentered("HOLD SCOREBOARD - GO TO OPTIONS SCREEN", pos);
					
					ExtrasManager.Instance.Draw(gameTime);

                    if (((tickLast - delayStart) / delayBlink) % 2 == 1) {
                        pos.X = centerPos.X;
                        pos.Y = 450;
                        TextHandler.DrawTextCentered("NHAN VAO MAN HINH DE TIEP TUC", pos);
                    }

                    if (delayStart + delayHelp < tickLast)
                        currentScreen = GameScreen.MainMenuScreen;

                    break;

                case GameScreen.OptionsScreen:

                    string scoreboardPos = InvasionGame.Scoreboard.DisplaySide.ToString().ToUpper();

                    string gameMusicEnabled = (InvasionGame.musicManager.Enabled)? "ON" : "OFF";

                    string difficultyText = InvasionGame.Scoreboard.DifficultyLevel.ToString().ToUpper();

                    string autoSelectWeaponText = (StarShip.AutoSelectWeapon)? "YES" : "NO";

                    InvasionGame.graphics.GraphicsDevice.Clear(Color.Black);

                    pos = new Vector2(centerPos.X, optionsTitleY);
                    TextHandler.DrawTextCentered("OPTIONS", pos, 2.0f, Color.White);

                    Color dimRowColor = Color.Gray;
                    Color rowColor;
                    pos.Y = optionsFirstMenuItemY;

                    int iEntry = -1;

                    if (!GameInProgress) {
                        rowColor = (selectedEntry == ++iEntry)? Color.White : dimRowColor;
                        TextHandler.DrawTextCentered("VI TRI BANG DIEM: " + scoreboardPos, pos, 1.5f, rowColor); pos.Y += optionsMenuItemDeltaY;
                    }

                    rowColor = (selectedEntry == ++iEntry)? Color.White : dimRowColor;
                    TextHandler.DrawTextCentered("AM THANH: " + gameMusicEnabled, pos, 1.5f, rowColor); pos.Y += optionsMenuItemDeltaY;

                    rowColor = (selectedEntry == ++iEntry)? Color.White : dimRowColor;
                    TextHandler.DrawTextCentered("DO KHO: " + difficultyText, pos, 1.5f, rowColor); pos.Y += optionsMenuItemDeltaY;

                    rowColor = (selectedEntry == ++iEntry)? Color.White : dimRowColor;
                    TextHandler.DrawTextCentered("TU DONG CHON VU KHI: " + autoSelectWeaponText, pos, 1.5f, rowColor); pos.Y = options2ndSubmenuY;


                    rowColor = (selectedEntry == ++iEntry)? Color.White : dimRowColor;
                    TextHandler.DrawTextCentered("DANH GIA VA BINH CHON", pos, 1.5f, rowColor); pos.Y += optionsMenuItemDeltaY;

                    if (((tickLast - delayStart) / delayBlink) % 3 != 1) {
                        pos.Y = 420;
                        TextHandler.DrawTextCentered("NHAN PHIM BACK", pos, 1.0f, Color.White);
                        pos.Y += 30;
                        if (GameInProgress)
                            TextHandler.DrawTextCentered("DE RESUME GAME", pos);
                        else
                            TextHandler.DrawTextCentered("DE TRO VE MENU CHINH", pos);
                    }

                    break;

                case GameScreen.PauseScreen:

                    pos = new Vector2(centerPos.X, 45);

                    InvasionGame.spriteBatch.Draw(backgroundTex, Vector2.Zero, Color.White);

                    TextHandler.DrawTextCentered("GAME IS PAUSED", pos, 2.0f, Color.White);
                    pos.Y = optionsFirstMenuItemY;

                    rowColor = (selectedEntry == 0)? Color.White : Color.DarkGray;
                    TextHandler.DrawTextCentered("RESUME GAME", pos, 1.5f, rowColor);
                    pos.Y += optionsMenuItemDeltaY;
                    rowColor = (selectedEntry == 1)? Color.White : Color.DarkGray;
                    TextHandler.DrawTextCentered("MAIN MENU", pos, 1.5f, rowColor);
                    pos.Y += optionsMenuItemDeltaY;
                    rowColor = (selectedEntry == 2)? Color.White : Color.DarkGray;
                    TextHandler.DrawTextCentered("QUIT", pos, 1.5f, rowColor);
                    pos.Y += optionsMenuItemDeltaY;

                    break;
			}
		}



        #region Handle Input

        private bool HandleBackButton() {

            PlayerIndex player;
            bool backPressed = input.IsNewButtonPress(Buttons.Back, PlayerIndex.One, out player);

            if (backPressed) {
                if (currentScreen == GameScreen.MainMenuScreen) {
                    Game.Exit();
                }
                else if (currentScreen == GameScreen.GameplayScreen) {
                    currentScreen = GameScreen.PauseScreen;
                    delayOffset = 0;
                }
                else if (currentScreen == GameScreen.PauseScreen) {
                    currentScreen = GameScreen.GameplayScreen;
                    delayOffset = 0;
                }
                else if (currentScreen == GameScreen.OptionsScreen) {
                    currentScreen = (GameInProgress)? GameScreen.GameplayScreen : GameScreen.MainMenuScreen;
                    delayOffset = 0;
                }
                else {
                    currentScreen = GameScreen.MainMenuScreen;
                    UFOsManager.Instance.Reset();
                    ExtrasManager.Instance.Reset();
                    starShip.Reset();
                    delayOffset = 0;
                }
            }

            return backPressed;
        }


        readonly Vector2 notTouched = new Vector2(-1, -1);

        private void HandleInput(GameTime gameTime) {

			if (HandleBackButton())
                return;

            TouchCollection touches = input.TouchState;

            if (waitForRelease) {
                if (touches.Count == 0)
                    waitForRelease = false;
                else
                    return;
            }

            Vector2 touchPt = notTouched;
            Vector2 releasePt = notTouched;

            if (touches.Count > 0) {
                if (touches[0].State == TouchLocationState.Pressed || touches[0].State == TouchLocationState.Moved)
                    touchPt = touches[0].Position;

                else if (touches[0].State == TouchLocationState.Released)
                    releasePt = touches[0].Position;
            }

            switch (currentScreen) {

                case GameScreen.GameplayScreen:

                    HandleGameplayScreenTouchInput(gameTime, touchPt, releasePt);
                    break;

                case GameScreen.MainMenuScreen:
                    const int itemHeight  = 40;
                    Rectangle itemRect = new Rectangle(0, 260, 800, itemHeight);

                    int touchedEntry = -1;

                    const int numMenuItems = 5;

                    for (int menuItem = 0; menuItem < numMenuItems; menuItem++) {
                        if (releasePt.X >= itemRect.Left && releasePt.X <= itemRect.Right &&
                            releasePt.Y >= (itemRect.Top + menuItem*itemHeight) && releasePt.Y <= (itemRect.Bottom + menuItem*itemHeight))
                        {
                            touchedEntry = menuItem;
                            break;
                        }
                    }

                    if (touchedEntry != -1) {
                        iOption = touchedEntry;

                        switch (iOption) {
                            case 0: // Play Game Menu Item
                                currentScreen = GameScreen.GameplayScreen;
                                ResetGame();

                                SoundManager.Instance.Play(menuSoundEffect);
                                break;

                            case 1: // Options Menu Item
                                currentScreen = GameScreen.OptionsScreen;
                                delayStart = Environment.TickCount;
                                break;

                            case 2: // Credits Menu Item
                                currentScreen = GameScreen.CreditsScreen;
                                delayStart = Environment.TickCount;
                                break;

                            case 3: // Help Menu Item
                                currentScreen = GameScreen.HelpScreen;
                                ExtrasManager.Instance.Reset();

                                for (int iExtra = 0; iExtra < Extra.NumExtraTypes; iExtra++) {
                                    Debug.WriteLine("Setting up iExtra = " + iExtra + ", Extra = " + (Extra.ExtraType)(iExtra));
                                    Vector2 extraPos = new Vector2(240, helpScreenExtrasFirstRowY+helpScreenExtrasLineSpacing*iExtra);
                                    Extra extra = new Extra(Game, extraPos, (Extra.ExtraType)(iExtra));
                                    ExtrasManager.Instance.Add(extra);
                                }

                                delayOffset = 20;
                                delayStart = Environment.TickCount;
                                Debug.WriteLine("delayOffset = " + delayOffset + ", delayStart = " + delayStart);
                                break;

                            case 4: // Quit Menu Item Exits the Game
                                Game.Exit();
                                break;
                        }
                    }
                    else {
                        for (int menuItem = 0; menuItem < numMenuItems; menuItem++) {

                            if (touchPt.X >= itemRect.Left && touchPt.X <= itemRect.Right &&
                                touchPt.Y >= (itemRect.Top + menuItem*itemHeight) && touchPt.Y <= (itemRect.Bottom + menuItem*itemHeight))
                            {
                                touchedEntry = menuItem;
                                break;
                            }
                        }

                        if (touchedEntry != -1) {
                            iOption = touchedEntry;
                        }
                    }
                    break;

                case GameScreen.OptionsScreen:

                    HandleOptionsScreenTouchInput(gameTime, touchPt, releasePt);
                    break;

                case GameScreen.CreditsScreen:
                case GameScreen.HelpScreen:
                    // go back to Main Menu when screen is tapped
                    if (releasePt != notTouched) {
                        currentScreen = GameScreen.MainMenuScreen;
                        delayStart = Environment.TickCount;
                    }
                    break;

                case GameScreen.PauseScreen:

                    HandlePauseScreenTouchInput(gameTime, touchPt, releasePt);
                    break;

                case GameScreen.LevelScreen:
                default:
                    break;
            }
		}



        Rectangle menuItemBoundingBox = new Rectangle(0, 0, 800, 10+15+15);
        const float optionsButtonDebounceSec = 0.5f;
        float timeSinceButtonPressSec = 60.0f;
        int selectedEntry = -1;

        /// <summary>
        /// Handles Input for the Options Screen
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="touchPt"></param>
        /// <param name="tapPt"></param>
        private void HandleOptionsScreenTouchInput(GameTime gameTime, Vector2 touchPt, Vector2 tapPt) {

            timeSinceButtonPressSec += (float)gameTime.ElapsedGameTime.TotalSeconds;

            selectedEntry = -1;
            int iEntry = -1;

            if (touchPt != notTouched) {

                if (!GameInProgress) {
                    // Scoreboard Entry
                    menuItemBoundingBox.Y = optionsFirstMenuItemY + optionsMenuItemDeltaY*(++iEntry) - 10;

                    if (Intersects(menuItemBoundingBox, touchPt))
                        selectedEntry = iEntry;
                }

                // Game Music
                menuItemBoundingBox.Y = optionsFirstMenuItemY + optionsMenuItemDeltaY*(++iEntry) - 10;

                if (Intersects(menuItemBoundingBox, touchPt))
                    selectedEntry = iEntry;

                // Difficulty Level
                menuItemBoundingBox.Y = optionsFirstMenuItemY + optionsMenuItemDeltaY*(++iEntry) - 10;

                if (Intersects(menuItemBoundingBox, touchPt))
                    selectedEntry = iEntry;

                // Auto-Select Weapon
                menuItemBoundingBox.Y = optionsFirstMenuItemY + optionsMenuItemDeltaY*(++iEntry) - 10;

                if (Intersects(menuItemBoundingBox, touchPt))
                    selectedEntry = iEntry;


                // Rate & Review
                menuItemBoundingBox.Y = options2ndSubmenuY - 10;
                ++iEntry;

                if (Intersects(menuItemBoundingBox, touchPt))
                    selectedEntry = iEntry;

                // Find our other Games
                menuItemBoundingBox.Y = options2ndSubmenuY + optionsMenuItemDeltaY - 10;
                ++iEntry;

                if (Intersects(menuItemBoundingBox, touchPt))
                    selectedEntry = iEntry;

                return;
            }

            if (tapPt == notTouched || timeSinceButtonPressSec <= optionsButtonDebounceSec)
                return;

            timeSinceButtonPressSec = 0.0f;

            iEntry = -1;

            // Scoreboard Position
            if (!GameInProgress) {
                menuItemBoundingBox.Y = optionsFirstMenuItemY + optionsMenuItemDeltaY*(++iEntry) - 10;

                if (Intersects(menuItemBoundingBox, tapPt)) {
                    InvasionGame.Scoreboard.DisplaySide = (InvasionGame.Scoreboard.DisplaySide == DisplaySide.Left)?
                    DisplaySide.Right : DisplaySide.Left;
                    return;
                }
            }

            // Game Music
            menuItemBoundingBox.Y = optionsFirstMenuItemY + optionsMenuItemDeltaY*(++iEntry) - 10;

            if (Intersects(menuItemBoundingBox, tapPt)) {
                InvasionGame.musicManager.Enabled = !InvasionGame.musicManager.Enabled;

                MusicManager musicManager = (MusicManager)Game.Services.GetService(typeof(MusicManager));

                if (InvasionGame.musicManager.Enabled)
                    musicManager.Play(InvasionGame.gameMusic, true);

                else if (musicManager.IsGameMusicPlaying)
                    musicManager.Stop();

                return;
            }

            // Difficulty Level
            menuItemBoundingBox.Y = optionsFirstMenuItemY + optionsMenuItemDeltaY*(++iEntry) - 10;

            if (Intersects(menuItemBoundingBox, tapPt)) {
                InvasionGame.Scoreboard.DifficultyLevel = (DifficultyLevel)(((int)InvasionGame.Scoreboard.DifficultyLevel+1)%3);
                return;
            }

            // Auto-Select Weapon
            menuItemBoundingBox.Y = optionsFirstMenuItemY + optionsMenuItemDeltaY*(++iEntry) - 10;

            if (Intersects(menuItemBoundingBox, tapPt)) {
                StarShip.AutoSelectWeapon = !StarShip.AutoSelectWeapon;
                return;
            }

            // Rate & Review
            menuItemBoundingBox.Y = options2ndSubmenuY - 10;

            if (Intersects(menuItemBoundingBox, tapPt)) {
                try {
                    Microsoft.Phone.Tasks.MarketplaceReviewTask mr = new Microsoft.Phone.Tasks.MarketplaceReviewTask();
                    mr.Show();
                }
                catch (Exception ex) {
                    Debug.WriteLine("Exception while attempting to Show MarketplaceReviewTask:\n" + ex.Message);
                }

                return;
            }

            // Find our other Games
            menuItemBoundingBox.Y = options2ndSubmenuY + optionsMenuItemDeltaY - 10;

            if (Intersects(menuItemBoundingBox, tapPt)) {
                try {
                    Microsoft.Phone.Tasks.MarketplaceSearchTask task = new Microsoft.Phone.Tasks.MarketplaceSearchTask();
                    task.ContentType = Microsoft.Phone.Tasks.MarketplaceContentType.Applications;
                    task.SearchTerms = "ImproviSoft";
                    task.Show();
                }
                catch (Exception ex) {
                    Debug.WriteLine("Exception while attempting to Show MarketplaceSearchTask:\n" + ex.Message);
                }

                return;
            }
        }


        /// <summary>
        /// Handles Input for the Pause Screen
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="touchPt"></param>
        /// <param name="tapPt"></param>
        private void HandlePauseScreenTouchInput(GameTime gameTime, Vector2 touchPt, Vector2 tapPt) {

            selectedEntry = -1;

            if (touchPt != notTouched) {

                // Resume Game
                menuItemBoundingBox.Y = optionsFirstMenuItemY - 10;

                if (Intersects(menuItemBoundingBox, touchPt))
                    selectedEntry = 0;

                // Main Menu
                menuItemBoundingBox.Y = optionsFirstMenuItemY + pauseMenuItemDeltaY - 10;

                if (Intersects(menuItemBoundingBox, touchPt))
                    selectedEntry = 1;

                // Quit
                menuItemBoundingBox.Y = optionsFirstMenuItemY + pauseMenuItemDeltaY*2 - 10;

                if (Intersects(menuItemBoundingBox, touchPt))
                    selectedEntry = 2;

                return;
            }

            if (tapPt == notTouched)
                return;

            // Resume Game
            menuItemBoundingBox.Y = optionsFirstMenuItemY - 10;

            if (Intersects(menuItemBoundingBox, tapPt)) {
                currentScreen = GameScreen.GameplayScreen;
                delayOffset = 0;
                return;
            }

            // Main Menu
            menuItemBoundingBox.Y = optionsFirstMenuItemY + pauseMenuItemDeltaY - 10;

            if (Intersects(menuItemBoundingBox, tapPt)) {
                currentScreen = GameScreen.MainMenuScreen;
                delayOffset = 0;
                return;
            }

            // Quit
            menuItemBoundingBox.Y = optionsFirstMenuItemY + pauseMenuItemDeltaY*2 - 10;

            if (Intersects(menuItemBoundingBox, tapPt)) {
                Game.Exit();
                return;
            }

        }


        bool starshipSelected = false;
        float lastDragDelta = 0.0f;
        const float minTimeBetweenSpeedChangesSec = 1.0f;
        float timeSinceSpeedChangeSec = 60.0f;

        /// <summary>
        /// Handles Input for the Gameplay Screen
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="touchPt"></param>
        /// <param name="tapPt"></param>
        private void HandleGameplayScreenTouchInput(GameTime gameTime, Vector2 touchPt, Vector2 tapPt) {

            timeSinceSpeedChangeSec += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (tapPt != notTouched) {
                if (Intersects(InvasionGame.Scoreboard.WeaponTapRect, tapPt)) {
                    starShip.SelectNextWeapon();
                    InvasionGame.Scoreboard.Weapon = starShip.Weapon;
                    input.Gestures.Clear();
                    //while (TouchPanel.IsGestureAvailable)
                    //    TouchPanel.ReadGesture();
                    return;
                }
                else if (Intersects(InvasionGame.Scoreboard.AutoSelectTapRect, tapPt)) {
                    StarShip.AutoSelectWeapon = !StarShip.AutoSelectWeapon;
                    input.Gestures.Clear();
                    return;
                }
            }

            bool firePressed = false;
            int moveShipLeft = 0;
            int moveShipRight = 0;
            bool skidToAStop = false;
            bool skid = false;
            bool stopMovingShip = false;

            if (touchPt != notTouched && starShip.Intersects(touchPt)) {
                starshipSelected = true;
            }

            foreach (GestureSample gesture in input.Gestures) {

                // we can use the type of gesture to determine our behavior
                switch (gesture.GestureType) {

                    // on taps, we change the color of the selected sprite
                    case GestureType.Tap:
                    //case GestureType.DoubleTap:
                        if (starshipSelected) {
                            if (Math.Abs(starShip.SpeedX) > 2)
                                skidToAStop = true;
                            else
                                stopMovingShip = true;
                        }

                        firePressed = true;
                        break;

                    // on drags, we just want to move the selected sprite with the drag
                    case GestureType.HorizontalDrag:
                        if (starshipSelected) {
                            stopMovingShip = true;
                            starShip.Position += gesture.Delta;

                            // skid if switching directions
                            if (gesture.Delta.X > 0.0f) { // dragging right
                                if (starShip.SpeedX < 0 || lastDragDelta < 0.0f)
                                    skid = true;
                            }
                            else if (gesture.Delta.X < 0.0f) { //dragging left
                                if (starShip.SpeedX > 0 || lastDragDelta > 0.0f)
                                    skid = true;
                            }
                            lastDragDelta = gesture.Delta.X;
                        }
                        break;

                    // When the scoreboard is held, go to the Options menu
                    case GestureType.Hold:
                        if ((InvasionGame.Scoreboard.DisplaySide == DisplaySide.Left && touchPt.X < Scoreboard.Width) ||
                            (InvasionGame.Scoreboard.DisplaySide == DisplaySide.Right && touchPt.X >= Scoreboard.RightPos.X)) 
                        {
                            currentScreen = GameScreen.OptionsScreen;
                            delayOffset = 500;
                        }
                        break;

                    // on flicks, we want to update the selected sprite's velocity with
                    // the flick velocity, which is in pixels per second.
                    case GestureType.Flick:
                        if (starshipSelected) {
                            if (gesture.Delta.X > 0.0f) {
                                if (starShip.SpeedX < 0)
                                    skid = true;
                                moveShipRight = 4;
                            }
                            else if (gesture.Delta.X < 0.0f) {
                                if (starShip.SpeedX > 0)
                                    skid = true;
                                moveShipLeft = 4;
                            }
                        }
                        break;
                }
            }

            // give priority to touch gesture input
            if (moveShipLeft == 0 && moveShipRight == 0 && !stopMovingShip && !skidToAStop) {

                // Handle Accelerometer Input
                accelerometerState = Accelerometer.GetState();

                float accelX = accelerometerState.Acceleration.X;
                float accelY = accelerometerState.Acceleration.Y;

                if (accelY < -0.1f) {
                    moveShipRight = (accelY < -0.2f)? 4 : 2;

                    if (starShip.SpeedX < -2)
                        skid = true;
                }

                if (accelY > 0.1f) {
                    moveShipLeft = (accelY > 0.2f)? 4 : 2;

                    if (starShip.SpeedX > 2)
                        skid = true;
                }

                // Make very slow moving starship slow to a stop
                if (accelY >= -0.1f && accelY <= 0.1f) {
                    if (timeSinceSpeedChangeSec > minTimeBetweenSpeedChangesSec) {
                        timeSinceSpeedChangeSec = 0.0f;

                        int speed = Math.Abs(starShip.SpeedX);

                        if (starShip.SpeedX < 0) {
                            starShip.MoveLeft(speed-1);
                        }
                        else if (starShip.SpeedX > 0) {
                            starShip.MoveRight(speed-1);
                        }
                    }
                }
            }

            if (firePressed) {
                if (starShip.ShipState == ShipState.OK) {
                    starShip.FireWeapon();
                }
            }

            if (skidToAStop) {
                starShip.SkidToAStop();
            }
            else {
                if (skid)
                    starShip.Skid();

                if (stopMovingShip)
                    starShip.StopMoving();
            }

            // Move after skid/stop in case change in direction : skid, then move opposition direction
            if (moveShipLeft != 0) {
                starShip.MoveLeft(moveShipLeft);
            }
            else if (moveShipRight != 0) {
                starShip.MoveRight(moveShipRight);
            }

            if (touchPt == notTouched) {
                starshipSelected = false;
                lastDragDelta = 0.0f;
            }
        }

        #endregion


        /// <summary>
        /// Set up a new game of Invasion
        /// </summary>
        private void ResetGame() {
            GameInProgress = true;

            iLevel = 0;

            starShip.Reset();

            InvasionGame.Scoreboard.Score = 0;

            InitLevel();
        }


		/// <summary>
		/// Initialize the next level
		/// </summary>
		public void InitLevel()
		{
            iLevel++;
            InvasionGame.Scoreboard.Level = iLevel;

            BulletsManager.Instance.Reset();
            UFOsManager.Instance.Reset();
            ExtrasManager.Instance.Reset();

            starShip.InitLevel();

			delayStart = Environment.TickCount;
			currentScreen = GameScreen.LevelScreen;

            UFOsManager.Instance.SetLevel(iLevel);
            UFOsManager.Instance.InitLevel(iLevel);
		}



        // Helper function to determine if a point (Vector2) is contained within a Rectangle
        public bool Intersects(Rectangle rect, Vector2 pt) {
            return
                pt.X >= rect.X && pt.X <= rect.X + rect.Width &&
                pt.Y >= rect.Y && pt.Y <= rect.Y + rect.Height;
        }
	}
}
