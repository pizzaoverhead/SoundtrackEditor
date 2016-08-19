using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SoundtrackEditor
{
    public class Fader
    {
        private System.Timers.Timer fadeOutTimer = new System.Timers.Timer();

        private bool _fadingIn = false;
        private float _fadeInStart = 0;
        private float _fadeInEnd = 0;

        private bool _fadingOut = false;
        private float _fadeOutStart = 0;
        private float _fadeOutEnd = 0;

        public AudioSource Speaker;

        public Fader(AudioSource speaker)
        {
            Speaker = speaker;

            fadeOutTimer.Elapsed += new System.Timers.ElapsedEventHandler(fadeOutTimer_Elapsed);
        }

        public void fadeOutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            fadeOutTimer.Stop();
            _fadingOut = true;
            _fadeOutEnd += Time.realtimeSinceStartup;
            Utils.Log("Begining fade out");
        }

        public void BeginPlaylistFadeIn(Playlist p)
        {
            Utils.Log("Begining fade in playlist");
            // TODO: Shorten the fade-in if the track is too short.
            _fadingIn = true;
            _fadeInStart = Time.realtimeSinceStartup;
            _fadeInEnd = _fadeInStart + p.fade.fadeIn;
            Speaker.volume = 0;
        }

        public void BeginTrackFadeIn(Playlist p)
        {
            Utils.Log("Begining fade in track");
            _fadingIn = true;
            _fadeInEnd = Time.realtimeSinceStartup + p.trackFade.fadeIn;
            Speaker.volume = 0;
        }

        public void BeginPlaylistFadeOut(Playlist p)
        {
            Utils.Log("Begining fade out playlist");
            double time = (Speaker.clip.length - p.fade.fadeOut) * 1000;
            if (time > 0)
            {
                fadeOutTimer.Interval = time;
                _fadeOutEnd = p.fade.fadeOut;
                fadeOutTimer.Start();
            }
            else
                Debug.LogError("Invalid fadeOut playlist time: Clip " + Speaker.clip.name +
                    " of length " + Speaker.clip.length + " was shorter than the fade out time of " + p.fade.fadeOut);
        }

        public void BeginTrackFadeOut(Playlist p)
        {
            Utils.Log("Begining fade out track");
            double time = (Speaker.clip.length - p.fade.fadeOut) * 1000;
            if (time > 0)
            {
                fadeOutTimer.Interval = time;
                _fadeOutEnd = p.trackFade.fadeOut;
                fadeOutTimer.Start();
            }
            else
                Debug.LogError("Invalid fadeOut clip time: Clip " + Speaker.clip.name +
                    " of length " + Speaker.clip.length + " was shorter than the fade out time of " + p.fade.fadeOut);
        }

        private bool _wasFadingOut = false;
        private bool _wasFadingIn = false;
        // TODO: This now fades poorly, and interferes with the Player GUI volume control.
        public void Fade()
        {
            float time = Time.realtimeSinceStartup;
            if (_fadingOut)
            {
                if (time >= _fadeOutEnd)
                {
                    _fadingOut = false;
                    Speaker.volume = GameSettings.MUSIC_VOLUME;
                }
                else
                {
                    if (!_wasFadingOut)
                        _fadeOutStart = Time.realtimeSinceStartup;

                    Utils.Log("Fading out to " + GameSettings.MUSIC_VOLUME +
                        " from " + _fadeOutStart + " to " + _fadeOutEnd + " at " + time + " = " + Mathf.InverseLerp(_fadeOutEnd, _fadeOutStart, time));
                    Speaker.volume = Mathf.InverseLerp(_fadeOutEnd, _fadeOutStart, time) * GameSettings.MUSIC_VOLUME;
                }
                _wasFadingOut = true;
            }
            else
                _wasFadingOut = false;

            // TODO: Deal with simultaneous fading out and in.
            if (_fadingIn)
            {
                if (time >= _fadeInEnd)
                {
                    _fadingIn = false;
                    Speaker.volume = GameSettings.MUSIC_VOLUME;
                }
                else
                {
                    if (!_wasFadingIn)
                        _fadeInStart = Time.realtimeSinceStartup;

                    Utils.Log("Fading in to " + GameSettings.MUSIC_VOLUME +
                        " from " + _fadeInStart + " to " + _fadeInEnd + " at " + time + " = " + Mathf.InverseLerp(_fadeInStart, _fadeInEnd, time));
                    Speaker.volume = Mathf.InverseLerp(_fadeInStart, _fadeInEnd, time) * GameSettings.MUSIC_VOLUME;
                }
                _wasFadingIn = true;
            }
            else
                _wasFadingIn = false;


            /*/ TODO: Preload
            if (CurrentPlaylist != null)
            {
                float preloadTime = CurrentPlaylist.preloadTime + CurrentPlaylist.trackFade.fadeOut;
            }*/
        }

        public void PlaybackStopped()
        {
            fadeOutTimer.Stop();
        }
    }
}
