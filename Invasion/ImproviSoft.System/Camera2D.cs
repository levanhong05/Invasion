// This file was provided by Elbert Perez from OccasionalGamer.com for my StarshipShooter
// sample that I showed at the Windows Phone 7 Garage Event in June 2011 at Syracuse University.
// I modified it slightly for performance & to use simple translation and hopefully didn't mess it up!


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImproviSoft.System {

    /// <summary>
    /// Camera to control world translation
    /// </summary>
    public class Camera2D {

        #region Singleton

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static readonly Camera2D instance = new Camera2D();

        static Camera2D() {
        }

        public static Camera2D Instance {
            get { return instance; }
        }

        #endregion        



        #region Members

        private Vector2 position;
        private float rotation;

        private float scale; //zoom
        private float scale3;

        private Matrix transform;        
        private GraphicsDevice graphicsDevice;

        private int shakeDuration; //ms
        private int shakeIntensity;

        #endregion



        #region Properties
       
        public Matrix Transform
        {
            get { return transform; }
            set { transform = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set {
                position = value;
            }
        }

        public float Rotation
        {
            get { return rotation; }
            set {
                rotation = value;
            }
        }

        public float Scale
        {
            get { return scale; }
            set {
                scale = value;
                scale3 = scale * scale * scale;
            }
        }        

        #endregion



        #region Methods

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(GraphicsDevice graphicsDevice) {
            this.graphicsDevice = graphicsDevice;

            shakeDuration = 0;
            shakeIntensity = 0;

            //Our start position, rotation, scale, and translation speed.
            //These are all changeable by property but I'm sure a function would work just as well/better,
            //feel free to experiment.
            position = Vector2.Zero;
            rotation = 0;
            scale3 = scale = 1.0f;
        }

        
        public void Shake(int duration, int intensity)
        {            
            shakeDuration += duration;
            shakeIntensity = intensity;
        }


        private void RecalculateTransformMatrix() {
            transform = Matrix.CreateScale(scale3, scale3, 0)
                * Matrix.CreateRotationZ(rotation)
                * Matrix.CreateTranslation(position.X * scale3, position.Y * scale3, 0);
        }
        

        public void Update(GameTime gameTime) {

            position = Vector2.Zero;

            if (shakeDuration > 0) {
                shakeDuration -= gameTime.ElapsedGameTime.Milliseconds;

                position.X += RandomManager.Instance.Next(-shakeIntensity, shakeIntensity);
                position.Y += RandomManager.Instance.Next(-shakeIntensity, shakeIntensity);
            }

            RecalculateTransformMatrix();
        }

        #endregion
    }
}