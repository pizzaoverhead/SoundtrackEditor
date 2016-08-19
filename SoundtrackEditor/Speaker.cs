/*
 * Nothing to see here, move along...
 * 
using UnityEngine;

namespace SoundtrackEditor
{
    public class Speaker
    {
        public Enums.Channel Channel;
        public AudioSource Source;
        public AudioClip CurrentClip;
        public AudioClip PreloadClip;
        public Fader Fade;
        private System.Timers.Timer preloadTimer = new System.Timers.Timer();

        public Speaker(GameObject gameObject, Enums.Channel channel)
        {
            Source = gameObject.AddComponent<AudioSource>();
            Channel = channel;
            preloadTimer.Elapsed += new System.Timers.ElapsedEventHandler(preloadTimer_Elapsed);

            // Disable positional effects.
            Source.panLevel = 0;
            Source.dopplerLevel = 0;
            Source.loop = false;
            switch (Channel)
            {
                case (Enums.Channel.Ship):
                    Source.volume = GameSettings.SHIP_VOLUME;
                    break;
	            case (Enums.Channel.Voice):
                    Source.volume = GameSettings.VOICE_VOLUME;
                    break;
	            case (Enums.Channel.Ambient):
                    Source.volume = GameSettings.MUSIC_VOLUME;
                    break;
                case (Enums.Channel.Music):
                    Source.volume = GameSettings.SHIP_VOLUME;
                    break;
                case (Enums.Channel.UI):
                    Source.volume = GameSettings.UI_VOLUME;
                    break;
                default:
                    break;
                    // TODO: Add channel volume.
            }
            Fade = new Fader(Source);
        }

        public void preloadTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            preloadTimer.Stop();
            if (CurrentPlaylist.trackIndex + 1 < CurrentPlaylist.tracks.Count)
            {
                Utils.Log("Beginning preload");
                PreloadClip = AudioLoader.GetAudioClip(CurrentPlaylist.tracks[CurrentPlaylist.trackIndex + 1]);
            }
        }
    }
}*/