using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImproviSoft.System {

    public class RandomManager {
        
        #region Singleton

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static readonly Random instance = new Random();

        static RandomManager() {
        }

        public static Random Instance {
            get { return instance; }
        }

        #endregion


        public static float RandomBetween(float min, float max) {
            return min + (float)Instance.NextDouble() * (max - min);
        }

    }
}
