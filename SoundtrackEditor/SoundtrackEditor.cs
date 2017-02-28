using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using KSP.UI.Screens;//required for ApplicationLauncherButton type

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

// TODO: Pause music when the game pauses.
// TODO: Don't change tracks just because the camera view changed.

// TODO: Multichannel sound.
// TODO: Ensure preloading takes into account pausing/playlist changing.
// TODO: Merge playlists when multiple are valid. Play non-shuffled tracks first, then fill with the rest.
// TODO: Unload any unused stock tracks.
// TODO: Only check the situation for things playlists are actually interested in.

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
        AudioClip emptyTrack = AudioClip.Create("none", 44100, 1, 44100, false);
        public static Playlist.Prerequisites CurrentSituation = new Playlist.Prerequisites() { scene = Enums.Scenes.Loading };
        public List<Playlist> Playlists = new List<Playlist>();
        public List<Playlist> ActivePlaylists; // Currently selected for use.
        public Playlist CurrentPlaylist;
        public AudioSource Speaker;
        public AudioClip CurrentClip;
        public AudioClip PreloadClip;
        public static bool InitialLoadingComplete = false; // Whether we have reached the main menu yet during this session.
        private System.Timers.Timer preloadTimer = new System.Timers.Timer();
        private Fader fader;
        //private EventManager _eventManager = new EventManager();

        public static SoundtrackEditor Instance { get; private set; }

        public void Start()
        {
            Instance = this;
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

            /*for (int i = 0; i < Enum.GetNames(typeof(Enums.Channel)).Length; i++)
            {
                Speakers[i] = new Speaker(gameObject, (Enums.Channel)i);
            }*/

            // Set up the main audio source.
            Speaker = gameObject.AddComponent<AudioSource>();
            // Disable positional effects.
            Speaker.spatialBlend = 0;
            Speaker.dopplerLevel = 0;
            Speaker.loop = false;
            Speaker.volume = GameSettings.MUSIC_VOLUME;
            fader = new Fader(Speaker);

            // TODO: Change volume on unpause or main menu.

            EventManager.Instance.AddEvents();

            // Set up test playlists.
            //SoundTest soundTest = new SoundTest();
            //SoundTest.CreatePlaylists(Playlists);

            Playlists = Persistor.LoadPlaylists();
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

        private void UnloadStockMusicPlayer()
        {
            // Add the stock sounds to the database so we can access them later.
            GameDatabase.Instance.databaseAudio.AddRange(music.constructionPlaylist);
            GameDatabase.Instance.databaseAudio.AddRange(music.spacePlaylist);
            GameDatabase.Instance.databaseAudio.Add(music.adminFacilityAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.astroComplexAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.credits);
            GameDatabase.Instance.databaseAudio.Add(music.menuAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.menuTheme);
            GameDatabase.Instance.databaseAudio.Add(music.missionControlAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.researchComplexAmbience);
            GameDatabase.Instance.databaseAudio.Add(music.spaceCenterAmbienceDay);
            GameDatabase.Instance.databaseAudio.Add(music.spaceCenterAmbienceNight);
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
            music.spaceCenterAmbienceDay = emptyTrack;
            music.spaceCenterAmbienceNight = emptyTrack;
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
                music.spaceCenterAmbienceDay,
                music.spaceCenterAmbienceNight,
                music.SPHAmbience,
                music.trackingAmbience,
                music.VABAmbience
            };
            allTracks.AddRange(music.constructionPlaylist);
            allTracks.AddRange(music.spacePlaylist);

            foreach (var clip in allTracks)
            {
                //GameDatabase.Instance.RemoveAudioClip(clip.name);
                DestroyImmediate(clip, true);
            }

            //deleted = true;
        }

        private bool _loading = false;
        public void Update()
        {
            if (LoadingClip != null && LoadingClip.loadState == AudioDataLoadState.Loaded)
                PlayClip(LoadingClip);

            UpdateCurrentTrack();
            fader.Fade();
        }

        public void OnSituationChanged()
        {
            UpdatePlaylist();
        }

        #region Playback management

        public void UpdateCurrentTrack()
        {
            // TODO: Pre-emtptively load tracks when near the end of the current one.
            if (Speaker.clip == null || Speaker.clip.loadState == AudioDataLoadState.Failed)
            {
                if (Speaker.clip != null)
                    Utils.Log("Failed loading track " + Speaker.clip.name);

                if (CurrentPlaylist != null && CurrentPlaylist.tracks.Count > 0)
                {
                    //Utils.Log("Loading initial track");
                    PlayNextTrack(CurrentPlaylist);
                }
            }
            else if (!Speaker.isPlaying && !PlaybackPaused)
            {
                if (Speaker.clip.loadState != AudioDataLoadState.Loaded)
                {
                    //Utils.Log("Loading...");
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

        public void UpdatePlaylist()
        {
            ActivePlaylists = GetValidPlaylists();
            if (ActivePlaylists == null || ActivePlaylists.Count == 0)
            {
                StopPlayback();
                CurrentPlaylist = null;
                return;
            }

            SortActivePlaylists();

            if (!ActivePlaylists.Equals(CurrentPlaylist))
            {
                CurrentPlaylist = ActivePlaylists[0];
                //Utils.Log("Switching to playlist " + ActivePlaylists[0].name + " of " + ActivePlaylists.Count + " matching playlists.");
                SwitchToPlaylist(ActivePlaylists[0]);
            }
        }

        private void SortActivePlaylists()
        {
            int numPlaylists = ActivePlaylists.Count;
            bool movedCurrent = false;
            for (int i = 0; i < numPlaylists; i++)
            {
                if (!movedCurrent && i > 0 && ActivePlaylists[i] == CurrentPlaylist)
                {
                    ActivePlaylists.RemoveAt(i);
                    ActivePlaylists.Insert(0, CurrentPlaylist);
                    i = 0;
                    movedCurrent = true;
                }

                bool hasPlayNext = !String.IsNullOrEmpty(ActivePlaylists[i].playNext);
                bool hasPlayAfter = !String.IsNullOrEmpty(ActivePlaylists[i].playAfter);
                bool hasPlayBefore = !String.IsNullOrEmpty(ActivePlaylists[i].playBefore);
                for (int j = 0; j < numPlaylists; j++)
                {
                    string n = ActivePlaylists[j].name;
                    if (j > i && ((hasPlayNext && n == ActivePlaylists[i].playNext) || (hasPlayAfter && n == ActivePlaylists[i].playAfter)))
                    {
                        Playlist p = ActivePlaylists[j];
                        ActivePlaylists.RemoveAt(j);
                        ActivePlaylists.Insert(i, p);
                        break;
                    }
                    else if (hasPlayBefore && n == ActivePlaylists[i].playBefore)
                    {
                        Playlist p = ActivePlaylists[i];
                        ActivePlaylists.RemoveAt(i);
                        int insertAt = j - 1 >= 0 ? j - 1 : 0;
                        ActivePlaylists.Insert(insertAt, p);
                        break;
                    }
                }
            }
        }

        public List<Playlist> GetValidPlaylists()
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
                /*else
                    Utils.Log("Playlist " + p.name + " is disabled");*/
            }

            // TODO: Select an appropriate playlist.
            // Merge all valid playlists?
            if (validPlaylists.Count > 0)
                return validPlaylists;
            else
            {
                //Utils.Log("No valid playlists found!");
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

        public void PlayPreviousTrack()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.tracks.Count == 0)
            {
                CurrentClip = null;
                return;
            }

            Utils.Log("Playing previous track");
            CurrentPlaylist.trackIndex--;
            if (CurrentPlaylist.trackIndex < 0)
                CurrentPlaylist.trackIndex = 0;
            PlayClip(CurrentPlaylist.tracks[CurrentPlaylist.trackIndex]);
        }

        public void PlayNextTrack()
        {
            PlayNextTrack(CurrentPlaylist);
        }

        public void PlayNextTrack(Playlist p)
        {
            if (p == null)
            {
                CurrentClip = null;
                //Debug.LogError("PlayNextTrack: Playlist was null");
                return;
            }
            if (p.tracks.Count == 0)
            {
                // Empty playlist.
                if (ActivePlaylists.Count > 1)
                {
                    int i = ActivePlaylists.IndexOf(CurrentPlaylist) + 1;
                    if (i < ActivePlaylists.Count)
                    {
                        SwitchToPlaylist(ActivePlaylists[i]);
                        return;
                    }
                    Utils.Log("PlayNextTrack: No other playlists found");
                }
                CurrentClip = null;
                Utils.Log("PlayNextTrack: No tracks found");
                return;
            }

            Utils.Log("Playing next track");
            p.trackIndex++;
            if (p.trackIndex >= p.tracks.Count)
            {
                // Check if we have any other appropriate playlists
                if (ActivePlaylists.Count > 1)
                {
                    int i = ActivePlaylists.IndexOf(CurrentPlaylist) + 1;
                    if (i < ActivePlaylists.Count)
                    {
                        SwitchToPlaylist(ActivePlaylists[i]);
                        return;
                    }
                }
                if (!p.loop) // TODO: should loop have priority over playNext?
                {
                    Utils.Log("PlayNextTrack: All tracks played (" + p.trackIndex + " >= " + p.tracks.Count + ")");
                    p.trackIndex--;

                    if (!string.IsNullOrEmpty(p.playNext))
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
                    return; // No more tracks to play.
                }
                else
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
                AudioClip clip;
                if (CurrentClip != null && CurrentClip.name == clipName)
                    clip = CurrentClip;
                else
                    clip = AudioLoader.GetAudioClip(clipName);

                PlayClip(clip);
            }
        }

        public AudioClip LoadingClip { get; private set; }

        public void PlayClip(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("[STED] PlayClip: Unabled to load clip");
                return;
            }
            if (clip.loadState != AudioDataLoadState.Loaded)
            {
                Utils.Log("Loading audio for " + clip.name + "...");
                LoadingClip = clip;
                LoadingClip.LoadAudioData();
                return;
            }
            else
                LoadingClip = null;
            if (clip.length == 0)
            {
                Debug.LogError("[STED] PlayClip: Clip was empty " + clip.name);
                return;
            }

            /*if (CurrentClip != null)
            {
                Utils.Log("PlayClip: Unloading previous clip");
                //AudioClip.DestroyImmediate(CurrentClip, true);
            }*/
            Utils.Log("Now Playing: " + clip.name);
            CurrentClip = clip;
            Speaker.clip = clip;
            Speaker.time = 0;

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
            PlaybackPaused = false;
        }

        private bool _trackPausedBeforeGamePause = false;
        public void OnGamePause()
        {
            if (PlaybackPaused)
                _trackPausedBeforeGamePause = true;
            else
                PausePlayback();
        }

        public void OnGameUnpause()
        {
            if (!_trackPausedBeforeGamePause)
                ResumePlayback();
            _trackPausedBeforeGamePause = false;
        }

        public bool PlaybackPaused { get; private set; }

        public void PausePlayback()
        {
            if (Speaker.isPlaying)
            {
                Speaker.Pause();
                PlaybackPaused = true;
            }
        }

        public void ResumePlayback()
        {
            if (PlaybackPaused)
            {
                Speaker.Play();
                PlaybackPaused = false;
            }
        }

        private int _skipTime = 10;
        public void FastForward()
        {
            if (Speaker.clip != null)
            {
                float t = Speaker.time + _skipTime;
                Speaker.time = t >= Speaker.clip.length ? Speaker.clip.length - 0.5f : t;
            }
        }

        public void Rewind()
        {
            if (Speaker.clip != null)
            {
                float t = Speaker.time - _skipTime;
                Speaker.time = t > 0 ? t : 0;
            }
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

        #endregion Playback management

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
     * Ability to play track on first discovery only.
     * Ability to queue a track to play immediately after the current one ends.
     */
}
