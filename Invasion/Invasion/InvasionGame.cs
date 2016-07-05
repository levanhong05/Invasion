using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

namespace Invasion {

    public enum Weapon { Laser1, Laser2, Laser3, Photon1, Photon2, Photon3 }

    public enum ShipState { OK, Exploding, Destroyed }

    

    /// <summary>
    /// This the main Invasion Game class
    /// </summary>
    public class InvasionGame : Microsoft.Xna.Framework.Game {
        internal static GraphicsDeviceManager graphics = null;
        internal static SpriteBatch spriteBatch = null;
        internal static Scoreboard Scoreboard = null;


        static internal SoundEffect explosionSoundEffect = null;
        static internal SoundEffect damageSoundEffect = null;
        static internal SoundEffect ufoShootSoundEffect = null;
        static internal SoundEffect blasterShootSoundEffect = null;
        static internal SoundEffect gameOverSoundEffect = null;
        static internal SoundEffect startGameSoundEffect = null;
        static internal SoundEffect weaponChangeSoundEffect = null;


        Invasion screenManager;
#if DEBUG
        FrameRateCounter frameRateCounter = null; // for diagnostic purposes only (not part of the game)
#endif
        public TouchLocation? touchLocation = null;

        // A simple component to help us manage background music
        static internal MusicManager musicManager;

        // The background music we want playing
        public static Song gameMusic = null;

        public const string GameStateFilename = "GameState.dat";

        public const int SectorWidth = 640;
        public const int SectorHeight = 480;
        public static Vector2 SectorPos = Vector2.Zero;


        public InvasionGame() {
            graphics = new GraphicsDeviceManager(this);
            Services.AddService(typeof(GraphicsDeviceManager), graphics);

            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            //TargetElapsedTime = TimeSpan.FromSeconds(1.0d / 30.0d); //another way to express 1/30th of a second (30 fps)

            Window.Title = "Invasion";
#if DEBUG
            // I do this so that it's easier to tell Debug from Release builds on the phone
            Window.Title += " (D)";
#endif
            // Set up the Scoreboard
            Scoreboard = new Scoreboard(this);
            Scoreboard.DisplaySide = DisplaySide.Right;
            Scoreboard.DifficultyLevel = DifficultyLevel.Medium;
            Components.Add(Scoreboard);

            // In future versions, I plan to use a ScreenManager class and separate GameScreen subclasses for
            // each of the game screens, but for now the Invasion class is handling all of those responsibilities,
            // which makes it quite messy:
            screenManager = new Invasion(this);
            Components.Add(screenManager);

            // Set up Landscape screen resolution
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;

            // Hide the phone's status bar from the top of the screen
            graphics.IsFullScreen = true;

            // Make sure screen-saver doesn't interrupt the game
            Guide.IsScreenSaverEnabled = false;

            // only allowing landscape screen orientations for this game
            graphics.SupportedOrientations =
                DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            try {
                graphics.ApplyChanges();
            }
            catch (Exception ex) {
                Debug.WriteLine("InvasionGame ctor: graphics.ApplyChanges() failed: " + ex.Message);
                Debugger.Break();
            }

            // set up support for playing background music
            musicManager = new MusicManager(this);
            musicManager.PromptGameHasControl += MusicManagerPromptGameHasControl;
            musicManager.PlaybackFailed += MusicManagerPlaybackFailed;
            Components.Add(musicManager);
            Services.AddService(typeof(MusicManager), musicManager);
        }


        void MusicManagerPromptGameHasControl(object sender, EventArgs e) {

            // Show a message box to see if the user wants to turn off their music for the game's music.

            Guide.BeginShowMessageBox(
                "Use game music?",
                "Would you like to turn off your music to listen to the game's music?",
                new[] { "Yes", "No" },
                0,
                MessageBoxIcon.None,
                result =>
                {
                    // Get the choice from the result
                    int? choice = Guide.EndShowMessageBox(result);

                    // If the user hit the yes button, stop the media player. Our music manager will
                    // see that we have a song the game wants to play and that the game will now have control
                    // and will automatically start playing our game song.
                    if (choice.HasValue && choice.Value == 0)
                        MediaPlayer.Stop();
                },
                null);
        }


        /// <summary>
        /// Invoked if music playback fails. The most likely case for this is that the Phone is connected to a PC
        /// that has Zune open, such as while debugging. Most games can probably just ignore this event, but we 
        /// can prompt the user so that they know why we're not playing any music.
        /// </summary>
        private void MusicManagerPlaybackFailed(object sender, EventArgs e) {
            // We're going to show a message box so the user knows why music didn't start playing.
            Guide.BeginShowMessageBox(
                "Music playback failed",
                "Music playback cannot begin if the phone is connected to a PC running Zune.",
                new[] { "Ok" },
                0,
                MessageBoxIcon.None,
                null,
                null);
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {

            Accelerometer.Initialize();
            new VibrationManager(this);  // used as force-feedback for starship explosions

            base.Initialize();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

#if DEBUG
            frameRateCounter = new FrameRateCounter(this, spriteBatch);  //DEBUG Uncomment this line for debugging purposes only
            if (frameRateCounter != null) {
                frameRateCounter.DrawOrder = int.MaxValue;
                Components.Add(frameRateCounter);
            }
#endif
            // In general this is only used for Debug purposes in Invasion (to draw the bounding boxes around sprites)
            SimpleShapes.LoadContent(GraphicsDevice);

            // Load the song to play and start playing it
            gameMusic = Content.Load<Song>("Music/Music");
            musicManager.Play(gameMusic, true);

            // Set up the 2D camera so that we can shake the screen when our ship is hit
            Camera2D.Instance.Initialize(GraphicsDevice);
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            //Note: The Invasion class' HandleInput function handles Back button presses,
            // so not checking for it here

            base.Update(gameTime);

            // Update the 2D Camera, which is used to shake the screen when the starship is hit
            Camera2D.Instance.Update(gameTime);

            // Will probably add SuppressDraw() calls in future versions.  Not critical for v1.0
            //if (Guide.IsVisible) // Add these to suppress calling Draw when the Guide is visible
            //    SuppressDraw();
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            // Set up the SpriteBatch with the 2DCamera Transform
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, Camera2D.Instance.Transform);

            screenManager.Draw(gameTime);
            spriteBatch.End();
        }
    }
}
