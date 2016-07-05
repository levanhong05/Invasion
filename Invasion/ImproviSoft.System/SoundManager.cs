#region File Description
//-----------------------------------------------------------------------------
// BackgroundMusicManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ImproviSoft.System {

    public class SoundManager {
        
        #region Singleton

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static readonly SoundManager instance = new SoundManager();

        private SoundManager() {
            MaxVolume = 1.0f;
        }

        public static SoundManager Instance {
            get { return instance; }
        }

        #endregion



        public bool IsMuted {
            get;
            set;
        }

        public float MaxVolume {
            get;
            set;
        }


        public void Play(SoundEffect sound) {
            Play(sound, MaxVolume);
        }

        
        /// <summary>
        /// Plays a given song as the background music.
        /// </summary>
        /// <param name="song">The song to play.</param>
        public void Play(SoundEffect sound, float volume) {
            if (IsMuted)
                return;

            volume = Math.Min(volume, MaxVolume);
            sound.Play(volume, 0.0f, 0.0f);
        }

    }
}
