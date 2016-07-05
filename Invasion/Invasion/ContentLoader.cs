using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Invasion {

    /// <summary>
    /// The intent of this class was to preload all content for the game, but it's
    /// only partially implemented.  Future versions might not have this class - ?
    /// </summary>
    static class ContentLoader {
        
        public static void LoadContent(ContentManager content) {
            LoadTextures(content);
            LoadSoundEffects(content);
        }


        static void LoadTextures(ContentManager content) {

        }


        static void LoadSoundEffects(ContentManager content) {

            InvasionGame.damageSoundEffect = content.Load<SoundEffect>("Sounds/tap");
            InvasionGame.blasterShootSoundEffect = content.Load<SoundEffect>("Sounds/blaster");
            InvasionGame.explosionSoundEffect = content.Load<SoundEffect>("Sounds/explosion");
            InvasionGame.ufoShootSoundEffect = content.Load<SoundEffect>("Sounds/ufoshoot");
            InvasionGame.gameOverSoundEffect = content.Load<SoundEffect>("Sounds/gameover");
            InvasionGame.startGameSoundEffect = content.Load<SoundEffect>("Sounds/blub");
            InvasionGame.weaponChangeSoundEffect = content.Load<SoundEffect>("Sounds/weaponchange");

            content.Load<SoundEffect>("Sounds/getextra");
            content.Load<SoundEffect>("Sounds/skid");
            content.Load<SoundEffect>("Sounds/blub");

        }

    }
}
