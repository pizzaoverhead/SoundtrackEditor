using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

/* Plans
 * 
 * Fixes:
 * Linux/Mac support
 * Memory usage
 * Loading times
 * 
 * Features:
 * Allow playlists for current single-track items.
 * Situational music
 */

// Main: Make GUI for debugging/viewing state.

// TODO: Multichannel sound.
// TODO: Ensure preloading takes into account pausing/playlist changing.
// TODO: Merge playlists when multiple are valid. Play non-shuffled tracks first, then fill with the rest.

// Idea: Play main theme for big milestones: First time reaching orbit, munar flag etc.

// Working: Continue playing track across scenes.

namespace SoundtrackEditor
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class SoundtrackEditor : MonoBehaviour
    {
        // TODO: Find time of day, play night sounds.
        //SunLight sl = (SunLight)FindObjectOfType(typeof(SunLight));
        //float x = sl.timeOfTheDay;
        MusicLogic music;
        // Create a new empty audio clip to replace the stock ones when they are disabled.
        AudioClip emptyTrack = AudioClip.Create("none", 44100, 1, 44100, false, true);
        public static Playlist.Prerequisites CurrentSituation = new Playlist.Prerequisites() { scene = Enums.Scene.Loading };
        public List<Playlist> Playlists = new List<Playlist>();
        public Playlist CurrentPlaylist;
        public AudioSource Speaker;
        public AudioClip CurrentClip;
        public AudioClip PreloadClip;
        public static bool InitialLoadingComplete = false; // Whether we have reached the main menu yet during this session.
        private System.Timers.Timer preloadTimer = new System.Timers.Timer();
        private Fader fader;

        private static SoundtrackEditor _instance;
        public static SoundtrackEditor Instance { get { return _instance; } }

        public void Start()
        {
            _instance = this;
            music = MusicLogic.fetch;

            preloadTimer.Elapsed += new System.Timers.ElapsedEventHandler(preloadTimer_Elapsed);

            //Utils.Log("# OS Version: " + Environment.OSVersion + ", Platform: " + Environment.OSVersion.Platform);
            //DeleteAllStock();

            DontDestroyOnLoad(gameObject);

            /*Load();
            UnloadUnusedTracks();

            // Remove positional effects.
            music.audio1.panLevel = 0;
            music.audio1.dopplerLevel = 0;
            music.audio2.panLevel = 0;
            music.audio2.dopplerLevel = 0;*/

            UnloadStockMusicPlayer();

            // Set up the main audio source.
            Speaker = gameObject.AddComponent<AudioSource>();
            // Disable positional effects.
            Speaker.panLevel = 0;
            Speaker.dopplerLevel = 0;
            Speaker.loop = false;
            Speaker.volume = GameSettings.MUSIC_VOLUME;
            fader = new Fader(Speaker);

            // Set up test playlists.
            //SoundTest soundTest = new SoundTest();
            //SoundTest.CreatePlaylists(Playlists);

            Persistor persistor = new Persistor();
            Playlists = persistor.LoadPlaylists();

            // TODO: Change volume on unpause or main menu.

            EventManager eventManager = new EventManager();
            eventManager.AddEvents();
        }

        /*private List<string> GetUserTrackNames()
        {
            List<string> clipNames = new List<string>();
            foreach (string file in Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file).Equals(".wav", StringComparison.InvariantCultureIgnoreCase) ||
                    Path.GetExtension(file).Equals(".ogg", StringComparison.InvariantCultureIgnoreCase) ||
                    Path.GetExtension(file).Equals(".mp3", StringComparison.InvariantCultureIgnoreCase))
                    clipNames.Add(Path.GetFileNameWithoutExtension(file));
            }
            return clipNames;

            //List<AudioClip> userTracks = GameDatabase.Instance.databaseAudio.FindAll(c => c.ToString().StartsWith("SoundtrackEditor/"));
            //return userTracks.ConvertAll(c => c.name);
        }

        private List<string> GetAllTrackNames()
        {
            List<string> trackNames = new List<string>();
            trackNames.AddRange(GetUserTrackNames());
            trackNames.AddRange(music.constructionPlaylist.ConvertAll(t => t.name));
            trackNames.AddRange(music.spacePlaylist.ConvertAll(t => t.name));
            trackNames.Add(music.missionControlAmbience.name);
            trackNames.Add(music.astroComplexAmbience.name);
            trackNames.Add(music.credits.name);
            trackNames.Add(music.menuAmbience.name);
            trackNames.Add(music.menuTheme.name);
            trackNames.Add(music.researchComplexAmbience.name);
            trackNames.Add(music.spaceCenterAmbience.name);
            trackNames.Add(music.SPHAmbience.name);
            trackNames.Add(music.trackingAmbience.name);
            trackNames.Add(music.VABAmbience.name);

            return trackNames;
        }

        private static bool IsRunningOnLinux()
        {
            // see http://mono-project.com/FAQ%3a_Technical#Mono_Platforms
            var p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }*/

        // TODO:
        //    [x] Get a list of all the files we have
        //    [x] Import the ones selected.
        //    [ ] Unload unused stock tracks
        //    [ ] Only load tracks when they are being played:    GameEvents.onGameSceneLoadRequested.Add(MyMethod); //(don't forget destroy)
        //    [ ] Unload them when done.

        #region GUI
        protected Rect windowPos = new Rect(Screen.width / 10, Screen.height / 10, 10f, 10f);

        internal void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(-5236628, windowPos, mainGUI, "Soundtrack Editor", GUILayout.Width(250), GUILayout.Height(50));
        }

        private string _previousTimeTrack = String.Empty;
        private void mainGUI(int windowID)
        {
            if (CurrentClip == null || CurrentClip.name == null)
                GUILayout.Label("Now Playing: None");
            else
                GUILayout.Label("Now Playing: " + CurrentClip.name);


            if (CurrentClip != null)
            {
                GUILayout.BeginHorizontal();
                TimeSpan currentTime = TimeSpan.FromSeconds(Speaker.time);
                string ct = currentTime.Hours > 0 ?
                    String.Format("{0:D2}:{1:D2}:{2:D2}", currentTime.Hours, currentTime.Minutes, currentTime.Seconds) :
                    String.Format("{0:D2}:{1:D2}", currentTime.Minutes, currentTime.Seconds);
                GUILayout.Label(ct);

                float newTime = GUILayout.HorizontalSlider(Speaker.time, 0, CurrentClip.length);
                // Filter to prevent stuttering when not changing the time.
                if (CurrentClip.name != _previousTimeTrack)
                {
                    Debug.Log("_previousTimeTrack: " + _previousTimeTrack + ", cur: " + CurrentClip.name + ", new: " + newTime + ", sTime: " + Speaker.time);
                    _previousTimeTrack = CurrentClip.name;
                }
                else
                {
                    Debug.Log("_previousTimeTrack: " + _previousTimeTrack + ", cur: " + CurrentClip.name + ", new: " + newTime);
                    if (Math.Abs(Speaker.time - newTime) > 1)
                        Speaker.time = newTime;
                }

                TimeSpan endTime = TimeSpan.FromSeconds(CurrentClip.length);
                string et = endTime.Hours > 0 ?
                    String.Format("{0:D2}:{1:D2}:{2:D2}", endTime.Hours, endTime.Minutes, endTime.Seconds) :
                    String.Format("{0:D2}:{1:D2}", endTime.Minutes, endTime.Seconds);
                GUILayout.Label(et);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("|◄")) { }
            if (GUILayout.Button("◄◄"))
                Rewind();
            if (GUILayout.Button("■"))
                StopPlayback();
            if (_playbackPaused)
            {
                if (GUILayout.Button("►"))
                    ResumePlayback();
            }
            else
            {
                if (GUILayout.Button("▌▌"))
                    PausePlayback();
            }
            if (GUILayout.Button("►►"))
                FastForward();
            if (GUILayout.Button("►|"))
                PlayNextTrack();
            GUILayout.EndHorizontal();

            if (_loadingClip != null)
                GUILayout.Label("Preloading track " + _loadingClip.name);

            if (CurrentPlaylist != null && CurrentPlaylist.tracks != null)
            {
                foreach (var t in CurrentPlaylist.tracks)
                {
                    if (CurrentPlaylist.tracks[CurrentPlaylist.trackIndex].Equals(t))
                        GUILayout.Label("► " + t);
                    else
                        GUILayout.Label("  " + t);
                }
            }

            GUI.DragWindow();
        }
        #endregion GUI

        private void UnloadStockMusicPlayer()
        {
            // Add the stock sounds to the database so we can access them later.
            GameDatabase.Instance.databaseAudio.AddRange(music.constructionPlaylist);
            GameDatabase.Instance.databaseAudio.AddRange(music.spacePlaylist);
            GameDatabase.Instance.databaseAudio.Add(music.astroComplexAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.credits);
            GameDatabase.Instance.databaseAudio.Add(music.menuAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.menuTheme);
            GameDatabase.Instance.databaseAudio.Add(music.missionControlAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.researchComplexAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.spaceCenterAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.SPHAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.trackingAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.VABAmbience);

            music.audio1.Stop();
            music.audio1.volume = 0;
            music.audio1.clip = emptyTrack;
            music.audio2.Stop();
            music.audio2.volume = 0;
            music.audio2.clip = emptyTrack;

            music.constructionPlaylist.Clear();
            music.spacePlaylist.Clear();
            music.astroComplexAmbience = emptyTrack;
            music.credits = emptyTrack;
            music.menuAmbience = emptyTrack;
            music.menuTheme = emptyTrack;
            music.missionControlAmbience = emptyTrack;
            music.researchComplexAmbience = emptyTrack;
            music.spaceCenterAmbience = emptyTrack;
            music.SPHAmbience = emptyTrack;
            music.trackingAmbience = emptyTrack;
            music.VABAmbience = emptyTrack;
        }

        private void DeleteAllStock()
        {
            List<AudioClip> allTracks = new List<AudioClip>{
                music.astroComplexAmbience,
                music.credits,
                music.menuAmbience,
                music.menuTheme,
                music.missionControlAmbience,
                music.researchComplexAmbience,
                music.spaceCenterAmbience,
                music.SPHAmbience,
                music.trackingAmbience,
                music.VABAmbience
            };
            allTracks.AddRange(music.constructionPlaylist);
            allTracks.AddRange(music.spacePlaylist);

            foreach (var clip in allTracks)
            {
                //GameDatabase.Instance.RemoveAudioClip(clip.name);
                AudioClip.DestroyImmediate(clip, true);
            }

            //deleted = true;
        }

        private bool _loading = false;
        public void Update()
        {
            if (_loadingClip != null && _loadingClip.isReadyToPlay)
                PlayClip(_loadingClip);

            UpdateSituation();
            UpdateCurrentTrack();
            fader.Fade();
        }

        public void UpdateSituation()
        {
            bool changed = false;

            // Scene: Handled by events.
            // Body: Handled by events. TODO: Not suitable?
            // Situation: Handled by events.
            // Camera mode

            // Throws exceptions before the initial loading screen is completed.
            if (CurrentSituation.scene == Enums.Scene.Flight)
            {
                Vessel v = SoundtrackEditor.InitialLoadingComplete ? FlightGlobals.ActiveVessel : null;

                if (v != null)
                {
                    if (!FlightGlobals.currentMainBody.name.Equals(CurrentSituation.bodyName))
                    {
                        Utils.Log("Body name changed");
                        CurrentSituation.bodyName = FlightGlobals.currentMainBody.name;
                        changed = true;
                    }

                    Enums.Selector inAtmosphere = v.atmDensity > 0 ? Enums.Selector.Yes : Enums.Selector.No;
                    if (CurrentSituation.inAtmosphere != inAtmosphere)
                    {
                        Utils.Log("In atmosphere changed");
                        changed = true;
                    }
                    CurrentSituation.inAtmosphere = inAtmosphere;

                    //FlightGlobals.currentMainBody.maxAtmosphereAltitude
                    //v.atmDensity

                    if (CameraManager.Instance != null)
                    {
                        changed |= CurrentSituation.cameraMode != Enums.ConvertCameraMode(CameraManager.Instance.currentCameraMode);
                        CurrentSituation.cameraMode = Enums.ConvertCameraMode(CameraManager.Instance.currentCameraMode);
                        if (changed)
                            Utils.Log("Camera mode changed");
                    }
                }
            }


            // TODO:
            // Velocity

            if (changed)
            {
                Utils.Log("Situation has changed");
                OnSituationChanged();
            }
        }

        public void OnSituationChanged()
        {
            Playlist p = SelectPlaylist();
            if (p == null)
            {
                StopPlayback();
                CurrentPlaylist = null;
                return;
            }

            if (!p.Equals(CurrentPlaylist))
            {
                Utils.Log("Switching to playlist " + p.name + " in scene " + CurrentSituation.scene);
                SwitchToPlaylist(p);
            }
        }

        public void UpdateCurrentTrack()
        {
            // TODO: Pre-emtptively load tracks when near the end of the current one.
            if (Speaker.clip == null)
            {
                if (CurrentPlaylist != null && CurrentPlaylist.tracks.Count > 0)
                {
                    Utils.Log("Loading initial track");
                    PlayNextTrack(CurrentPlaylist);
                }
            }
            else if (!Speaker.isPlaying && !_playbackPaused)
            {
                if (!Speaker.clip.isReadyToPlay)
                {
                    Utils.Log("Loading...");
                    _loading = true;
                }
                else
                {
                    if (_loading)
                    {
                        _loading = false;
                        Speaker.Play();
                    }
                    else
                        PlayNextTrack(CurrentPlaylist);
                }
            }
            // Else loaded and playing.
        }

        public Playlist SelectPlaylist()
        {
            var validPlaylists = new List<Playlist>();
            foreach (Playlist p in Playlists)
            {
                if (p.enabled)
                {
                    if (p.playWhen.PrerequisitesMet())
                        validPlaylists.Add(p);
                    /*else
                        Utils.Log("Playlist " + p.name + " failed prereqs");*/
                }
                else
                    Utils.Log("Playlist " + p.name + " is disabled");
            }

            // TODO: Select an appropriate playlist.
            // Merge all valid playlists?
            if (validPlaylists.Count > 0)
                return validPlaylists[0];
            else
            {
                Utils.Log("No valid playlists found!");
                return null;
            }
        }

        public void SwitchToPlaylist(Playlist p)
        {
            // TODO: If the new playlist contains the current track, continue playing it.
            Utils.Log("Changing playlist to " + p.name);
            CurrentPlaylist = p;
            p.trackIndex = 0;

            if (p.shuffle) p.Shuffle();

            if (p.tracks.Count > 0)
                PlayClip(p.tracks[p.trackIndex]);
            else
                Utils.Log("Playlist was empty: " + p.name);
        }

        public void PlayNextTrack()
        {
            PlayNextTrack(CurrentPlaylist);
        }

        public void PlayNextTrack(Playlist p)
        {
            if (p == null)
            {
                Debug.LogError("PlayNextTrack: Playlist was null");
                return;
            }
            if (p.tracks.Count == 0)
            {
                // TODO: Play next playlist.
                Utils.Log("PlayNextTrack: No tracks found");
                return;
            }

            Utils.Log("Playing next track");
            p.trackIndex++;
            if (p.trackIndex >= p.tracks.Count)
            {
                if (!p.loop)
                {
                    // TODO: Play next playlist.
                    Utils.Log("PlayNextTrack: All tracks played (" + p.trackIndex + " >= " + p.tracks.Count + ")");
                    p.trackIndex--;

                    if (!String.IsNullOrEmpty(p.playNext))
                    {
                        Utils.Log("Playlist ended. Playing next " + p.playNext);
                        foreach (Playlist play in Playlists)
                        {
                            if (play.name.Equals(p.playNext, StringComparison.CurrentCultureIgnoreCase))
                            {
                                Utils.Log("Playlist up next:  " + p.playNext);
                                play.enabled = true;
                                SwitchToPlaylist(play);
                                break;
                            }
                        }
                    }
                    return;
                }
                p.trackIndex = 0;
            }

            PlayClip(p.tracks[p.trackIndex]);
        }

        public void PlayClip(string clipName)
        {
            if (PreloadClip != null && PreloadClip.name.Equals(clipName))
                PlayClip(PreloadClip);
            else
            {
                AudioClip clip = AudioLoader.GetAudioClip(clipName);
                PlayClip(clip);
            }
        }

        private AudioClip _loadingClip = null;
        public void PlayClip(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("[STED] PlayClip: Unabled to load clip");
                return;
            }
            if (!clip.isReadyToPlay)
            {
                Utils.Log("Loading " + clip.name + "...");
                _loadingClip = clip;
                return;
            }
            else
                _loadingClip = null;
            if (clip.length == 0)
            {
                Debug.LogError("[STED] PlayClip: Clip was empty " + clip.name);
                return;
            }

            if (CurrentClip != null)
            {
                Utils.Log("PlayClip: Unloading previous clip");
                //AudioClip.DestroyImmediate(CurrentClip, true);
            }
            Utils.Log("Now Playing: " + clip.name);
            CurrentClip = clip;
            Speaker.clip = clip;

            if (CurrentPlaylist.fade.fadeIn > 0)
                fader.BeginPlaylistFadeIn(CurrentPlaylist);
            else if (CurrentPlaylist.trackFade.fadeIn > 0) // Playlist fade has precedence over track fade.
                fader.BeginTrackFadeIn(CurrentPlaylist);

            if (CurrentPlaylist.fade.fadeOut > 0)
                fader.BeginPlaylistFadeOut(CurrentPlaylist);
            else if (CurrentPlaylist.trackFade.fadeOut > 0)
                fader.BeginTrackFadeOut(CurrentPlaylist);

            if (CurrentPlaylist.preloadTime > 0)
            {
                if (CurrentPlaylist.trackIndex < CurrentPlaylist.tracks.Count - 1)
                    preloadTimer.Interval = Math.Max((Speaker.clip.length - CurrentPlaylist.preloadTime) * 1000, 0);
            }

            Speaker.Play();
        }

        public void StopPlayback()
        {
            Speaker.Stop();
            fader.PlaybackStopped();
        }

        private bool _playbackPaused = false;
        public void PausePlayback()
        {
            Speaker.Pause();
            _playbackPaused = true;
        }

        public void ResumePlayback()
        {
            if (_playbackPaused)
            {
                Speaker.Play();
                _playbackPaused = false;
            }
        }

        private int _skipTime = 10;
        public void FastForward()
        {
            float t = Speaker.time + _skipTime;
            Speaker.time = t >= Speaker.clip.length ? Speaker.clip.length - 0.5f : t;
        }

        public void Rewind()
        {
            float t = Speaker.time - _skipTime;
            Speaker.time = t > 0 ? t : 0;
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

    } // End of class.


    /* Tasks:
     * Create your own AudioSource, make sure it stays alive in all scenes.
     * Method of loading a specific playlist using a set of prerequisite flags as a key.
     * 
     * Playlist object, defined by flags. e.g. planet=kerbin, situation:orbitHigh, sunRise
     * Prerequisite
     * 
     * Sound areas:
     *   Scenes - scene change
     *   Bodies 
     *     Surface
     *     Atmosphere
     *       Aerobraking
     *       Flight
     *     Orbit
     *       Low
     *       High
     *     Flyby
     *     Each biome
     *   Targeted craft
     *   
     *   Vessel Situations
            LANDED = 0,
            SPLASHED = 1,
            PRELAUNCH = 2,
            FLYING = 3,
            SUB_ORBITAL = 4,
            ORBITING = 5,
            ESCAPING = 6,
            DOCKED = 7,
     *   
     * Ability to play track on first discovery only.
     * Ability to queue a track to play immediately after the current one ends.
     */
}
