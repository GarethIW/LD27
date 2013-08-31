using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;


namespace LD27
{
    public static class AudioController
    {
        public static float sfxvolume = 1f;
        public static float musicvolume = 0.25f;

        public static Random randomNumber = new Random();

        public static Dictionary<string, SoundEffect> effects;

        public static Dictionary<string, SoundEffectInstance> songs;

        public static Dictionary<string, SoundEffectInstance> instances;

        static string playingTrack = "";
        static bool isPlaying;

        public static string currentlyPlaying = "";

        public static int currentTrack = 0;

        

        public static void LoadContent(ContentManager content)
        {
            if (isPlaying) songs[currentlyPlaying].Stop();

            effects = new Dictionary<string, SoundEffect>();

            effects.Add("roomclunk", content.Load<SoundEffect>("audio/sfx/roomclunk"));
            effects.Add("roomscrape", content.Load<SoundEffect>("audio/sfx/roomscrape"));
            effects.Add("acid_hit", content.Load<SoundEffect>("audio/sfx/acid_hit"));
            effects.Add("door", content.Load<SoundEffect>("audio/sfx/door"));
            effects.Add("explosion1", content.Load<SoundEffect>("audio/sfx/explosion1"));
            effects.Add("explosion2", content.Load<SoundEffect>("audio/sfx/explosion2"));
            effects.Add("face_open", content.Load<SoundEffect>("audio/sfx/face_open"));
            effects.Add("face_gun", content.Load<SoundEffect>("audio/sfx/face_gun"));
            effects.Add("flesh_hit", content.Load<SoundEffect>("audio/sfx/flesh_hit"));
            effects.Add("face_die", content.Load<SoundEffect>("audio/sfx/face_die"));
            effects.Add("face_missile", content.Load<SoundEffect>("audio/sfx/face_missile"));
            effects.Add("sword", content.Load<SoundEffect>("audio/sfx/sword"));
            effects.Add("ooze_die", content.Load<SoundEffect>("audio/sfx/ooze_die"));
            effects.Add("ooze_spit", content.Load<SoundEffect>("audio/sfx/ooze_spit"));
            effects.Add("metal_hit", content.Load<SoundEffect>("audio/sfx/metal_hit"));
            effects.Add("ooze_split", content.Load<SoundEffect>("audio/sfx/ooze_split"));
            effects.Add("sentinel_shoot", content.Load<SoundEffect>("audio/sfx/sentinel_shoot"));
            effects.Add("bomb_place", content.Load<SoundEffect>("audio/sfx/bomb_place"));
            effects.Add("collect_health", content.Load<SoundEffect>("audio/sfx/collect_health"));
            effects.Add("defend", content.Load<SoundEffect>("audio/sfx/defend"));
            effects.Add("deflect", content.Load<SoundEffect>("audio/sfx/deflect"));
            effects.Add("gatling_deflect", content.Load<SoundEffect>("audio/sfx/gatling_deflect"));
            effects.Add("player_hit", content.Load<SoundEffect>("audio/sfx/flesh_hit"));
            effects.Add("ooze_hit", content.Load<SoundEffect>("audio/sfx/ooze_hit"));
            effects.Add("sentinel_die", content.Load<SoundEffect>("audio/sfx/sentinel_die"));
            effects.Add("room_clear", content.Load<SoundEffect>("audio/sfx/room_clear"));
            effects.Add("exit_open", content.Load<SoundEffect>("audio/sfx/exit_open"));
            effects.Add("complete", content.Load<SoundEffect>("audio/sfx/complete"));

            instances = new Dictionary<string, SoundEffectInstance>();
            instances.Add("roomscrape", effects["roomscrape"].CreateInstance());
            instances["roomscrape"].IsLooped = true;
            instances["roomscrape"].Play();
            instances["roomscrape"].Pause();

            songs = new Dictionary<string, SoundEffectInstance>();
            songs.Add("0", content.Load<SoundEffect>("audio/music/game").CreateInstance());
            
        }

        public static void LoadMusic(string piece, ContentManager content)
        {
            //if (currentlyPlaying.ToLower() == piece.ToLower()) return;
            //currentlyPlaying = piece;

            //if (!MediaPlayer.GameHasControl) return;

            //if (MediaPlayer.State != MediaState.Stopped) MediaPlayer.Stop();
            ////if (musicInstance != null)
            ////{
            ////    musicInstance.Dispose();
            ////}

            //musicInstance = content.Load<Song>("audio/music/" + piece);
            //MediaPlayer.IsRepeating = true;
            //// MediaPlayer.Volume = musicvolume;
            //MediaPlayer.Play(musicInstance);

            //if (!OptionsMenuScreen.music) MediaPlayer.Pause();
        }

        public static void PlayMusic()
        {
            PlayMusic(currentTrack.ToString());
            //currentTrack++;
            //if (currentTrack == 5) currentTrack = 0;
        }

        public static void PlayMusic(string track)
        {
            playingTrack = track;
            isPlaying = true;
            if(!songs[track].IsLooped) songs[track].IsLooped = true;
            songs[track].Volume = 0f;
            songs[track].Play();
        }

        public static void StopMusic()
        {

            isPlaying = false;
        }

        public static void ToggleMusic()
        {

            //if (OptionsMenuScreen.music)
            //{
            //    MediaPlayer.Resume();
            //}
            //else
            //    MediaPlayer.Pause();
        }

        public static void PlaySFX(string name)
        {
            //if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, 0f, 0f);
        }
        public static void PlaySFX(string name, float pitch)
        {
            //if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, pitch, 0f);
        }
        //public static void PlaySFX(string name, float volume, float pitch, float pan)
        //{
        //   // if (OptionsMenuScreen.sfx)
        //    if (pan < -1f || pan > 1f) return;
        //    volume = MathHelper.Clamp(volume, 0f, 1f);
        //    effects[name].Play(volume * sfxvolume, pitch, pan);
        //}
        public static void PlaySFX(string name, float volume, float minpitch, float maxpitch)
        {
           // if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume * volume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), 0f);
        }

        //internal static void PlaySFX(string name, float volume, float minpitch, float maxpitch, Vector2 Position)
        //{
        //    //Vector2 screenPos = Vector2.Transform(Position, Camera.Instance.CameraMatrix);
        //    //float dist = (Camera.Instance.Position - Position).Length();
        //    //if (dist < 2000f)
        //    //{
        //    //    float pan = MathHelper.Clamp((screenPos.X - (Camera.Instance.Width / 2)) / (Camera.Instance.Width / 2), -1f, 1f);
        //    //    effects[name].Play(((1f/2000f) * (2000f-dist)) * volume * sfxvolume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), pan);
        //    //}
        //}


        public static void Update(GameTime gameTime)
        {

            if (playingTrack == "") return;

            if(isPlaying)
                if (songs[playingTrack].Volume < musicvolume) songs[playingTrack].Volume=MathHelper.Clamp(songs[playingTrack].Volume + 0.01f, 0f, 1f);

             if (!isPlaying)
                 if (songs[playingTrack].Volume > 0) songs[playingTrack].Volume = MathHelper.Clamp(songs[playingTrack].Volume-0.01f,0f,1f);
                 else songs[playingTrack].Stop();

            // if (MediaPlayer.Volume > musicvolume) MediaPlayer.Volume = musicvolume;
        }

        public static void Unload()
        {

        }



       
    }
}
