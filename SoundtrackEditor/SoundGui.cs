using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SoundtrackEditor
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class GuiManager : MonoBehaviour
    {// ☰ ∞ 
        // GUI window location and sizes.
        protected Rect mainWindowPos = new Rect(Screen.width / 10, Screen.height / 20f, 400f, 10f);
        protected Rect playbackWindowPos = new Rect((Screen.width / 2) - 115, Screen.height / 10, 230f, 10f);
        protected Rect playlistWindowPos = new Rect((Screen.width / 2) - 115, Screen.height / 20, 400f, 10f);
        protected Rect audioPreviewWindowPos = new Rect((Screen.width / 2) - 115, Screen.height / 20, 400f, 10f);
        protected Rect editorWindowPos = new Rect(Screen.width / 6, 50, (Screen.width / 1.5f), Screen.height / 1.15f);

        // GUI visibility toggles.
        private static bool _allUIHidden = false;
        private bool _mainWindowVisible = false; // TODO: Persist this value for startup
        private bool _situationVisible = false;
        private bool _playbackControlsVisible = false;
        private bool _playlistGuiVisible = false;
        private bool _audioPreviewGuiVisible = false;
        private bool _editorGuiVisible = false;

        // Addon toolbar
        private static string _appImageFile = "SoundtrackEditor/Images/sted";
        internal bool Tooltip = false;
        private static ApplicationLauncherButton appButton = null;

        private string _previousSeekPosition = string.Empty; // Last seek position of the audio player's seek control.
        private bool _muted = false;
        private bool _playerTracksExpanded = false;
        private Vector2 _playlistScrollPosition = Vector2.zero;
        private Vector2 _tracksScrollPosition = Vector2.zero;
        private Dictionary<string, bool> _expandedPlaylists = new Dictionary<string, bool>();
        private Vector2 _audioDBScrollPosition = Vector2.zero;
        private Dictionary<string, bool> _expandedDbItems = new Dictionary<string, bool>();
        private List<AudioLoader.AudioFileInfo> _audioFileList;
        public AudioSource PreviewSpeaker;

        #region KSP Events

        /// <summary>
        /// First code called at the time specified by the class' attribute.
        /// Instant (during KSP's initial load) and only once per game session.
        /// </summary>
        public void Start()
        {
            DontDestroyOnLoad(gameObject); // Survive hitting the main menu for the first time.
            GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnAppLauncherDestroyed);
            SetUpDbPlayer();
            _audioFileList = AudioLoader.GetAvailableFiles();
        }

        /// <summary>
        /// Addon button (toolbar on the right) ready(?).
        /// </summary>
        public void OnAppLauncherReady()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && appButton == null)
            {
                appButton = ApplicationLauncher.Instance.AddModApplication(OpenGuiAppLauncher, CloseGuiAppLauncher, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS,
                    GameDatabase.Instance.GetTexture(_appImageFile, false));
            }
        }

        /// <summary>
        /// Addon button (toolbar on the right) destroyed.
        /// </summary>
        public void OnAppLauncherDestroyed()
        {
            if (appButton != null)
            {
                CloseGuiAppLauncher();
                ApplicationLauncher.Instance.RemoveApplication(appButton);
            }
        }

        /// <summary>
        /// Toggled on F2.
        /// </summary>
        public static void OnHideUI()
        {
            _allUIHidden = true;
        }

        /// <summary>
        /// Toggled on F2.
        /// </summary>
        public static void OnShowUI()
        {
            _allUIHidden = false;
        }

        /// <summary>
        /// GUI draw event. Called (at least once) each frame.
        /// </summary>
        public void OnGUI()
        {
            /*GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(-5236628, windowPos, mainGUI, "Soundtrack Editor", GUILayout.Width(250), GUILayout.Height(50));*/
            GUI.skin = HighLogic.Skin;
            if (!_allUIHidden)
            {
                if (_mainWindowVisible)
                    mainWindowPos = GUILayout.Window(GetInstanceID(), mainWindowPos, MainGui, "Soundtrack Editor");
                if (_playbackControlsVisible)
                    playbackWindowPos = GUILayout.Window(GetInstanceID() + 1, playbackWindowPos, PlaybackGui, "Player");
                if (_playlistGuiVisible)
                    playlistWindowPos = GUILayout.Window(GetInstanceID() + 2, playlistWindowPos, PlaylistGui, "Playlists");
                if (_audioPreviewGuiVisible)
                    audioPreviewWindowPos = GUILayout.Window(GetInstanceID() + 3, audioPreviewWindowPos, AudioPreviewGui, "Audio Preview");
                if (_editorGuiVisible)
                    editorWindowPos = GUILayout.Window(GetInstanceID() + 4, editorWindowPos, EditorGui, "Playlist Editor");
            }
        }

        #endregion KSP Events

        /// <summary>
        /// Creates an AudioSource for use in previewing sound effects.
        /// </summary>
        private void SetUpDbPlayer()
        {
            PreviewSpeaker = gameObject.AddComponent<AudioSource>();
            // Disable positional effects.
            PreviewSpeaker.spatialBlend = 0;
            PreviewSpeaker.dopplerLevel = 0;
            PreviewSpeaker.loop = false;
            PreviewSpeaker.volume = GameSettings.MUSIC_VOLUME;
        }

        private void OpenGuiAppLauncher()
        {
            _mainWindowVisible = true;
        }

        private void CloseGuiAppLauncher()
        {
            _mainWindowVisible = false;
        }

        private void MainGui(int windowID)
        {
            SoundtrackEditor sted = SoundtrackEditor.Instance;

            if (!Utils.LibMpg123Installed)
            {
                GUILayout.Label("<b>Warning! The file libmpg123 was not installed correctly!\r\nMP3 support is not available.</b>");
            }

            if (GUILayout.Button((_playbackControlsVisible ? "Hide" : "Show") + " Player"))
                _playbackControlsVisible = !_playbackControlsVisible;

            if (GUILayout.Button((_playlistGuiVisible ? "Hide" : "Show") + " Playlists"))
                _playlistGuiVisible = !_playlistGuiVisible;

            if (GUILayout.Button((_audioPreviewGuiVisible ? "Hide" : "Show") + " Audio Preview"))
                _audioPreviewGuiVisible = !_audioPreviewGuiVisible;

            bool resizeWindow = false;
            if (GUILayout.Button((_situationVisible ? "Hide" : "Show") + " Situation"))
            {
                _situationVisible = !_situationVisible;
                if (!_situationVisible)
                    resizeWindow = true;
            }

            if (_situationVisible)
            {
                GUILayout.Label("<b>Current Situation</b>");
                GUILayout.Label(Playlist.Prerequisites.PrintSituation());

				// TODO: fix so absence of any other vessel doesn't prevent soundtrack playback
				/*
                Vessel v = Utils.GetNearestVessel();
                if (v != null)
                    GUILayout.Label("Nearest vessel: " + v);
				*/

                if (sted.LoadingClip != null)
                    GUILayout.Label("Preloading track " + sted.LoadingClip.name);
            }

            /*if (FlightCamera.fetch != null)
            {
                if (FlightCamera.fetch.cameras != null)
                {
                    if (FlightCamera.fetch.cameras.Length == 0)
                        GUILayout.Label("No cameras");
                    else
                    {
                        foreach (Camera c in FlightCamera.fetch.cameras)
                        {
                            GUILayout.Label(c.name);
                        }
                        if (fisheye != null)
                        {
                            GuiUtils.slider("Strength X", ref fisheye.strengthX, -1.5f, 1.5f);
                            GuiUtils.slider("Strength Y", ref fisheye.strengthY, -1.5f, 1.5f);
                        }
                        if (noiseAndGrain != null)
                        {
                            GuiUtils.slider("Strength", ref noiseAndGrain.strength, 0f, 1f);
                            GuiUtils.slider("Black Intensity", ref noiseAndGrain.blackIntensity, 0, 1f);
                            GuiUtils.slider("White Intensity", ref noiseAndGrain.whiteIntensity, 0, 1f);
                        }
                        if (blur != null)
                        {
                            GuiUtils.slider("Amount", ref blur.blurAmount, 0f, 10f);
                        }
                    }
                }
            }

            if (GUILayout.Button("Add Fisheye"))
            {
                fisheye = FlightCamera.fetch.cameras[0].gameObject.AddComponent<Fisheye>();
                fisheye.fishEyeShader = Shader.Find("Hidden/FisheyeShader");
                fisheye.enabled = true;
            }

            if (GUILayout.Button("Add Bloom"))
            {
                bloom = FlightCamera.fetch.cameras[0].gameObject.AddComponent<BloomAndFlares>();
                bloom.addAlphaHackShader = Shader.Find("Hidden/Bloom");
                bloom.addBrightStuffOneOneShader = Shader.Find("Hidden/Bloom");
                bloom.brightPassFilterShader = Shader.Find("Hidden/Bloom");
                bloom.hollywoodFlareBlurShader = Shader.Find("Hidden/Bloom");
                bloom.hollywoodFlareStretchShader = Shader.Find("Hidden/Bloom");
                bloom.lensFlareShader = Shader.Find("Hidden/Bloom");
                bloom.separableBlurShader = Shader.Find("Hidden/Bloom");
                bloom.vignetteShader = Shader.Find("Hidden/Vignetting");
                bloom.enabled = true;
            }
             
NullReferenceException: Object reference not set to an instance of an object
  at PostEffectsBase.CheckShader (UnityEngine.Shader s) [0x00000] in <filename unknown>:0 

  at BloomAndFlares.CreateMaterials () [0x00000] in <filename unknown>:0 

  at BloomAndFlares.OnRenderImage (UnityEngine.RenderTexture source, UnityEngine.RenderTexture destination) [0x00000] in <filename unknown>:0 
             
             

            if (GUILayout.Button("Add Noise and Grain"))
            {
                noiseAndGrain = FlightCamera.fetch.cameras[0].gameObject.AddComponent<NoiseAndGrain>();
                noiseAndGrain.noiseShader = Shader.Find("Hidden/NoiseAndGrain");
                //noiseAndGrain.noiseTexture = TODO
                noiseAndGrain.enabled = true;
            }

            if (GUILayout.Button("Add motion blur"))
            {
                blur = FlightCamera.fetch.cameras[0].gameObject.AddComponent<MotionBlur>();
                blur.enabled = true;
                blur.extraBlur = true;
            }*/

            // Collapsing the Current Situation text leaves the window overly-large.
            // Resize the main window to its previous size.
            if (resizeWindow)
                mainWindowPos = new Rect(mainWindowPos.x, mainWindowPos.y, 400f, 10f);

            GUI.DragWindow();
        }


        /*Fisheye fisheye = null;
        BloomAndFlares bloom = null;
        NoiseAndGrain noiseAndGrain = null;
        MotionBlur blur = null;*/

        /// <summary>
        /// Draws a mini audio player with playback controls and track listing.
        /// </summary>
        /// <param name="windowID"></param>
        private void PlaybackGui(int windowID)
        {
            SoundtrackEditor sted = SoundtrackEditor.Instance;
            AudioSource speaker = sted.Speaker;
            AudioClip currentClip = sted.CurrentClip;

            //
            // Now Playing
            //
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (currentClip == null || currentClip.name == null)
                GUILayout.Label("None");
            else
                GUILayout.Label(currentClip.name);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //
            // Seek control
            //
            if (currentClip != null)
            {
                GUILayout.BeginHorizontal();
                TimeSpan currentTime = TimeSpan.FromSeconds(speaker.time);
                string ct = currentTime.Hours > 0 ?
                    String.Format("{0:D2}:{1:D2}:{2:D2}", currentTime.Hours, currentTime.Minutes, currentTime.Seconds) :
                    String.Format("{0:D2}:{1:D2}", currentTime.Minutes, currentTime.Seconds);
                GUILayout.Label(ct);

                float newTime = GUILayout.HorizontalSlider(speaker.time, 0, currentClip.length);
                // Filter to prevent stuttering when not changing the time.
                if (currentClip.name != _previousSeekPosition)
                    _previousSeekPosition = currentClip.name;
                else
                {
                    if (newTime == currentClip.length)
                    {
                        // Prevents "An invalid seek position was passed to this function." error.
                        newTime -= 0.1f;
                    }
                    if (Math.Abs(speaker.time - newTime) > 1)
                        speaker.time = newTime;
                }

                TimeSpan endTime = TimeSpan.FromSeconds(currentClip.length);
                string et = endTime.Hours > 0 ?
                    string.Format("{0:D2}:{1:D2}:{2:D2}", endTime.Hours, endTime.Minutes, endTime.Seconds) :
                    string.Format("{0:D2}:{1:D2}", endTime.Minutes, endTime.Seconds);
                GUILayout.Label(et);
                GUILayout.EndHorizontal();
            }

            //
            // Plackback control
            //
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("|◄"))
            {
                sted.PlayPreviousTrack();
            }
            if (GUILayout.Button("◄◄"))
                sted.Rewind();
            if (GUILayout.Button("  ■  "))
                sted.StopPlayback();
            if (sted.PlaybackPaused)
            {
                if (GUILayout.Button("►"))
                    sted.ResumePlayback();
            }
            else
            {
                if (GUILayout.Button(" ▮▮ "))
                    sted.PausePlayback();
            }
            if (GUILayout.Button("►►"))
                sted.FastForward();
            if (GUILayout.Button("►|"))
                sted.PlayNextTrack();
            GUILayout.EndHorizontal();

            //
            // Volume control
            //
            GUILayout.BeginHorizontal();
            GUILayout.Label("0%");
            GameSettings.MUSIC_VOLUME = GUILayout.HorizontalSlider(GameSettings.MUSIC_VOLUME, 0, 1);
            GUILayout.Label("100%");
            GUILayout.EndHorizontal();

            //
            // Mute and Tracks buttons
            //
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_muted ? "Unmute" : "Mute")) _muted = !_muted;
            speaker.volume = _muted ? 0 : GameSettings.MUSIC_VOLUME;

            bool resizeWindow = false;
            if (GUILayout.Button((_playerTracksExpanded ? "Hide" : "Show") + " Tracks"))
            {
                if (_playerTracksExpanded)
                    resizeWindow = true;
                _playerTracksExpanded = !_playerTracksExpanded;
            }
            GUILayout.EndHorizontal();

            //
            // Tracks list
            //
            if (_playerTracksExpanded)
            {
                Playlist currentPlaylist = sted.CurrentPlaylist;
                GUILayout.Label("<b>Tracks</b>");
                if (currentPlaylist != null)
                {
                    _tracksScrollPosition = GUILayout.BeginScrollView(_tracksScrollPosition, GUILayout.MinHeight(200));
                    foreach (var t in currentPlaylist.tracks)
                    {
                        if (currentPlaylist.tracks[currentPlaylist.trackIndex].Equals(t))
                            GUILayout.Label("► " + t);
                        else
                            GUILayout.Label("    " + t);
                    }
                    GUILayout.EndScrollView();
                }
                else
                    GUILayout.Label("None");
            }

            // Force a resize if we've changed shape.
            if (resizeWindow)
                playbackWindowPos = new Rect(playbackWindowPos.x, playbackWindowPos.y, 230f, 10f);

            GUI.DragWindow();
        }

        /// <summary>
        /// Draws a playlist viewer and editor GUI.
        /// </summary>
        /// <param name="windowID"></param>
        public void PlaylistGui(int windowID)
        {
            _playlistScrollPosition = GUILayout.BeginScrollView(_playlistScrollPosition, GUILayout.MinHeight(Screen.height / 1.25f));

            SoundtrackEditor sted = SoundtrackEditor.Instance;
            List<Playlist> playlists = sted.Playlists;

            for (int i = 0; i < playlists.Count; i++)
            {
                string playlistName = playlists[i].name;

                GUILayout.BeginHorizontal();

                if (!_expandedPlaylists.ContainsKey(playlistName))
                    _expandedPlaylists.Add(playlistName, false);
                bool isExpanded = _expandedPlaylists[playlistName];
                if (GUILayout.Button(isExpanded ? " - " : " + "))
                    _expandedPlaylists[playlistName] = !isExpanded;

                GUILayout.Label("<b>" + playlistName + (sted.CurrentPlaylist == playlists[i] ? " ►" : "") + " </b>");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Edit"))
                {
                    _editorGuiVisible = true;
                    _editingPlaylistOriginal = playlists[i];
                    _editingPlaylist = new Playlist(playlists[i]);
                    _previousInAtmosphere = playlists[i].playWhen.inAtmosphere;
                    _previousTimeOfDay = playlists[i].playWhen.timeOfDay;
                    _previousScene = playlists[i].playWhen.scene;
                    _previousSituation = playlists[i].playWhen.situation;
                    _previousCameraMode = playlists[i].playWhen.cameraMode;
                }
                GUILayout.EndHorizontal();

                if (_expandedPlaylists[playlistName])
                {
                    for (int j = 0; j < playlists[i].tracks.Count; j++)
                    {
                        string prefix =
                            sted.CurrentPlaylist != null && sted.CurrentPlaylist.name == playlists[i].name &&
                            sted.CurrentClip != null && sted.CurrentClip.name == playlists[i].tracks[j] ?
                            " ►  " : "       "; // Indent. The '►' is very wide in KSP's font.
                        GUILayout.Label(prefix + playlists[i].tracks[j]);
                    }
                }
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(" New "))
            {
                _editorGuiVisible = true;
                _editingPlaylist = new Playlist();
                _previousInAtmosphere = _editingPlaylist.playWhen.inAtmosphere;
                _previousTimeOfDay = _editingPlaylist.playWhen.timeOfDay;
                _previousScene = _editingPlaylist.playWhen.scene;
                _previousSituation = _editingPlaylist.playWhen.situation;
                _previousCameraMode = _editingPlaylist.playWhen.cameraMode;
            }
            GUILayout.EndHorizontal();

            // Close button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(" Close "))
                _playlistGuiVisible = false;
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        /// <summary>
        /// Draws a tool to preview available AudioClips.
        /// </summary>
        /// <param name="windowID"></param>
        public void AudioPreviewGui(int windowID)
        {
            _audioDBScrollPosition = GUILayout.BeginScrollView(_audioDBScrollPosition, GUILayout.MinHeight(Screen.height / 1.25f));
            GUILayout.Label("<b>Game Database</b>");
            for (int i = 0; i < GameDatabase.Instance.databaseAudio.Count; i++)
            {
                AudioPreviewGui(GameDatabase.Instance.databaseAudio[i]);
            }

            GUILayout.Label("<b>Music</b>");
            for (int i = 0; i < _audioFileList.Count; i++)
            {
                AudioDBFileGui(_audioFileList[i]);
            }

            GUILayout.EndScrollView();

            // Close button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(" Close "))
                _audioPreviewGuiVisible = false;
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        public void AudioPreviewGui(AudioClip clip)
        {
            string clipName = clip.name;
            GUILayout.BeginHorizontal();
            GUILayout.Label("   " + clipName);
            GUILayout.FlexibleSpace();

            PreviewClipButton(clip);

            if (!_expandedDbItems.ContainsKey(clipName))
                _expandedDbItems.Add(clipName, false);
            if (GUILayout.Button(" ☰  "))
                _expandedDbItems[clipName] = !_expandedDbItems[clipName];

            GUILayout.EndHorizontal();

            if (_expandedDbItems[clipName])
            {
                GUILayout.Label("      Channels:\t" + clip.channels.ToString());
                GUILayout.Label("      Frequency:\t" + clip.frequency.ToString() + " Hz");
                TimeSpan clipLength = TimeSpan.FromSeconds(clip.length);
                GUILayout.Label("      Length:\t" + string.Format("{0:D2}:{1:D2}", clipLength.Minutes, clipLength.Seconds));
                GUILayout.Label("      Load State:\t" + clip.loadState.ToString());
                GUILayout.Label("      Load Type:\t" + clip.loadType.ToString());
                GUILayout.Label("      Samples:\t" + clip.samples.ToString());
            }
        }

        public void PreviewClipButton(AudioClip clip)
        {
            string buttonText = " ► ";
            if (PreviewSpeaker.clip != null && PreviewSpeaker.clip.name == clip.name)
            {
                if (PreviewSpeaker.isPlaying)
                    buttonText = "  ■  ";
                else if (PreviewSpeaker.clip.loadState == AudioDataLoadState.Loading)
                    buttonText = "...";
            }

            if (GUILayout.Button(buttonText))
            {
                if (PreviewSpeaker.isPlaying && PreviewSpeaker.clip.name == clip.name)
                    PreviewSpeaker.Stop();
                else
                {
                    PreviewSpeaker.clip = clip;
                    PreviewSpeaker.Play();
                }
            }
        }

        private Dictionary<string, MP3Import.TrackInfo> LoadedMP3Info = new Dictionary<string, MP3Import.TrackInfo>();
        public void AudioDBFileGui(AudioLoader.AudioFileInfo audioFile)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("   " + audioFile.Name);
            GUILayout.FlexibleSpace();
            //string buttonText = " ► ";
            //if (DbPlayer.clip != null && DbPlayer.clip.name == clipName)
            //{
            //if (DbPlayer.isPlaying)
            //buttonText = "  ■  ";
            //else if (DbPlayer.clip.loadState == AudioDataLoadState.Loading)
            //buttonText = "...";
            //}

            //if (GUILayout.Button(buttonText))
            //{
            //if (DbPlayer.isPlaying && DbPlayer.clip.name == clipName)
            //DbPlayer.Stop();
            //else
            //{
            //DbPlayer.clip = clip;
            //DbPlayer.Play();
            //}
            //}
            if (!_expandedDbItems.ContainsKey(audioFile.Name))
                _expandedDbItems.Add(audioFile.Name, false);
            if (GUILayout.Button(" ☰  "))
                _expandedDbItems[audioFile.Name] = !_expandedDbItems[audioFile.Name];

            GUILayout.EndHorizontal();

            if (_expandedDbItems[audioFile.Name])
            {
                GUILayout.Label("   Path:\t" + audioFile.Path);
                GUILayout.Label("   File type:\t" + audioFile.FileExtension);
                if (audioFile.FileExtension.ToUpper() == ".MP3")
                {
                    if (Utils.LibMpg123Installed)
                    {
                        MP3Import.TrackInfo trackInfo;
                        LoadedMP3Info.TryGetValue(audioFile.Path, out trackInfo);
                        if (!LoadedMP3Info.ContainsKey(audioFile.Path))
                        {
                            MP3Import importer = new MP3Import();
                            trackInfo = importer.GetTrackInfo(audioFile.Path);
                            LoadedMP3Info.Add(audioFile.Path, trackInfo);
                        }
                        GUILayout.Label("      Title:\t\t" + trackInfo.Title);
                        GUILayout.Label("      Album:\t\t" + trackInfo.Album);
                        GUILayout.Label("      Artist:\t\t" + trackInfo.Artist);
                        GUILayout.Label("      Year:\t\t" + trackInfo.Year);
                        GUILayout.Label("      Genre:\t\t" + trackInfo.Genre);
                        GUILayout.Label("      Comment:\t" + trackInfo.Comment);
                        GUILayout.Label("      Tag:\t\t" + trackInfo.Tag);
                        GUILayout.Label("      Channels:\t" + trackInfo.Channels);
                        GUILayout.Label("      Encoding:\t" + trackInfo.Encoding);
                        GUILayout.Label("      Rate:\t\t" + trackInfo.Rate);
                    }
                    else
                        GUILayout.Label("MP3 support not installed");
                }
                //GUILayout.Label("   Channels:\t" + clip.channels.ToString());
                //GUILayout.Label("   Frequency:\t" + clip.frequency.ToString() + " Hz");
                //TimeSpan clipLength = TimeSpan.FromSeconds(clip.length);
                //GUILayout.Label("   Length:\t\t" + string.Format("{0:D2}:{1:D2}", clipLength.Minutes, clipLength.Seconds));
                //GUILayout.Label("   Load State:\t" + clip.loadState.ToString());
                //GUILayout.Label("   Load Type:\t" + clip.loadType.ToString());
                //GUILayout.Label("   Samples:\t" + clip.samples.ToString());
            }
        }

        private Playlist _editingPlaylist;
        private Playlist _editingPlaylistOriginal;
        private Vector2 _playlistPrereqsScrollPosition = Vector2.zero;
        private Vector2 _playlistTrackScrollPosition = Vector2.zero;
        private string _preloadTimeText = "";
        private string _maxSrfVelText = "";
        private string _minSrfVelText = "";
        private string _maxOrbVelText = "";
        private string _minOrbVelText = "";
        private string _maxAltitudeText = "";
        private string _minAltitudeText = "";
        public void EditorGui(int windowId)
        {
            // TODO: Save off original values.

            // Close the originating playlists window to cut down on clutter. We'll reopen it later.
            _playlistGuiVisible = false;

            //_editingPlaylist.name = GuiUtils.editString("Playlist name", _editingPlaylist.name);

            GUILayout.BeginHorizontal(); // Title
            GUILayout.Label("Playlist Name:");
            GUILayout.Space(10);
            _editingPlaylist.name = GUILayout.TextField(_editingPlaylist.name);
            GUILayout.EndHorizontal(); // Title

            GUILayout.Space(5);

            GUILayout.BeginHorizontal(); // Columns

            //
            // Settings
            //
            GUILayout.BeginVertical(); // Prereqs column
            // Header
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("<b>Settings</b>");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _playlistPrereqsScrollPosition = GUILayout.BeginScrollView(_playlistPrereqsScrollPosition, GUILayout.MinWidth(Screen.width / 3));
            _editingPlaylist.enabled = GUILayout.Toggle(_editingPlaylist.enabled, "Enabled");
            _editingPlaylist.loop = GUILayout.Toggle(_editingPlaylist.loop, "Loop");
            _editingPlaylist.shuffle = GUILayout.Toggle(_editingPlaylist.shuffle, "Shuffle");
            _editingPlaylist.pauseOnGamePause = GUILayout.Toggle(_editingPlaylist.pauseOnGamePause, "Pause On Game Pause");
            _editingPlaylist.disableAfterPlay = GUILayout.Toggle(_editingPlaylist.disableAfterPlay, "Disable Once Played");

            // Not yet implemented.
            //FadeEditor();

            // Play Next
            PlayNextPicker();
            PlayBeforePicker();
            PlayAfterPicker();
            // TODO: Channel
            _preloadTimeText = GuiUtils.editFloat("Preload Time (s):", _preloadTimeText, out _editingPlaylist.preloadTime);
            // TODO: Paused

            // TODO: Modify playlists in memory
            InAtmospherePicker();
            TimeOfDayPicker();
            ScenePicker();
            SituationPicker();
            CameraModePicker();
            _editingPlaylist.playWhen.bodyName = GuiUtils.editString("Body Name:", _editingPlaylist.playWhen.bodyName);
            // TODO: "Clear" buttons.

            _maxSrfVelText = GuiUtils.editFloat("Max Surface Velocity:", _maxSrfVelText, out _editingPlaylist.playWhen.maxVelocitySurface);
            _minSrfVelText = GuiUtils.editFloat("Min Surface Velocity:", _minSrfVelText, out _editingPlaylist.playWhen.minVelocitySurface);
            _maxOrbVelText = GuiUtils.editFloat("Max Orbital Velocity:", _maxOrbVelText, out _editingPlaylist.playWhen.maxVelocityOrbital);
            _minOrbVelText = GuiUtils.editFloat("Min Orbital Velocity:", _minOrbVelText, out _editingPlaylist.playWhen.minVelocityOrbital);
            _maxAltitudeText = GuiUtils.editFloat("Max Altitude:", _maxAltitudeText, out _editingPlaylist.playWhen.maxAltitude);
            _minAltitudeText = GuiUtils.editFloat("Min Altitude:", _minAltitudeText, out _editingPlaylist.playWhen.minAltitude);
            GUILayout.EndScrollView(); // _playlistPrereqsScrollPosition
            GUILayout.EndVertical(); // Prereqs column

            //
            // Tracks
            //
            GUILayout.BeginVertical(); // Tracks column
            // Header
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("<b>Tracks</b>");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _playlistTrackScrollPosition = GUILayout.BeginScrollView(_playlistTrackScrollPosition, GUILayout.MinWidth(Screen.width / 3));
            int removeTrack = -1;
            int moveUpTrack = -1;
            int moveDownTrack = -1;
            if (!_trackPickerVisible)
            {
                for (int i = 0; i < _editingPlaylist.tracks.Count; i++)
                {
                    GUILayout.BeginHorizontal(); // Track/Remove
                    GUILayout.Label(_editingPlaylist.tracks[i]);
                    GUILayout.FlexibleSpace();

                    if (i != 0 && GUILayout.Button(" ↑ "))
                        moveUpTrack = i;
                    if (i < _editingPlaylist.tracks.Count - 1)
                    {
                        if (GUILayout.Button(" ↓ "))
                            moveDownTrack = i;
                    }
                    else
                        GUILayout.Space(34);
                    if (GUILayout.Button(" Remove "))
                        removeTrack = i;
                    GUILayout.EndHorizontal(); // Track/Remove
                }
                if (removeTrack != -1)
                    _editingPlaylist.tracks.RemoveAt(removeTrack);
                if (moveUpTrack != -1)
                {
                    string t = _editingPlaylist.tracks[moveUpTrack];
                    _editingPlaylist.tracks.RemoveAt(moveUpTrack);
                    _editingPlaylist.tracks.Insert(moveUpTrack - 1, t);
                }
                if (moveDownTrack != -1)
                {
                    string t = _editingPlaylist.tracks[moveDownTrack];
                    _editingPlaylist.tracks.RemoveAt(moveDownTrack);
                    _editingPlaylist.tracks.Insert(moveDownTrack + 1, t);
                }
            }
            if (_trackPickerVisible)
                TrackPicker();
            GUILayout.EndScrollView(); // _playlistTrackScrollPosition
            if (_trackPickerVisible)
            {
                if (GUILayout.Button("Done"))
                    _trackPickerVisible = false;
            }
            else
            {
                if (GUILayout.Button(" Add Tracks "))
                    _trackPickerVisible = true;
            }
            GUILayout.EndVertical(); // Tracks column
            GUILayout.EndHorizontal(); // Columns

            // Footer
            GUILayout.BeginVertical(); // Footer
            GUILayout.Space(20); // Vertical filler space.
            GUILayout.BeginHorizontal(); // Buttons
            if (GUILayout.Button(" Delete Playlist "))
            {
                SoundtrackEditor sted = SoundtrackEditor.Instance;
                if (_editingPlaylistOriginal != null)
                    sted.Playlists.Remove(_editingPlaylistOriginal);
                _editorGuiVisible = false;
                _trackPickerVisible = false;
                _playlistGuiVisible = true;
                Persistor.SavePlaylists(sted.Playlists); // Kill it good.
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("  Save  "))
            {
                _editorGuiVisible = false;
                _trackPickerVisible = false;
                _playlistGuiVisible = true;

                SoundtrackEditor sted = SoundtrackEditor.Instance;
                if (_editingPlaylistOriginal != null)
                {
                    int idx = sted.Playlists.IndexOf(_editingPlaylistOriginal);
                    sted.Playlists.RemoveAt(idx);
                    _editingPlaylistOriginal = null;
                    sted.Playlists.Insert(idx, _editingPlaylist);
                }
                else
                    sted.Playlists.Add(_editingPlaylist);
                _editingPlaylist = null;
                Persistor.SavePlaylists(sted.Playlists);
            }
            if (GUILayout.Button(" Cancel "))
            {
                _playlistGuiVisible = true;

                _editorGuiVisible = false;
                // Collapse sub-components.
                _scenesExpanded = false;
                _cameraModeExpanded = false;
                _situationExpanded = false;
                _timeOfDayExpanded = false;
                _inAtmosphereExpanded = false;
                _fadeEditorVisible = false;
                _trackPickerVisible = false;
                _playNextPickerVisible = false;
                _playBeforePickerVisible = false;
                _playAfterPickerVisible = false;
            }
            GUILayout.EndHorizontal(); // Buttons
            GUILayout.EndVertical(); // Footer

            GUI.DragWindow();
        }

        public bool PickerGuiCollapsed(string name, string selection, bool sectionExpanded)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name + ":");
            GUILayout.FlexibleSpace();

            if (selection != String.Empty)
            {
                GUILayout.Label(" " + selection + " ");
            }

            if (GUILayout.Button("  ...  "))
                sectionExpanded = true;
            GUILayout.EndHorizontal();
            return sectionExpanded;
        }

        private bool _scenesExpanded = false;
        private Enums.Scenes _previousScene = 0;
        private void ScenePicker()
        {
            if (_scenesExpanded)
            {
                GUILayout.Label("<b>Scene</b>");
                GUILayout.BeginVertical();
                foreach (var e in Enum.GetValues(typeof(Enums.Scenes)))
                {
                    string val = e.ToString();
                    Enums.Scenes scene = (Enums.Scenes)e;

                    bool isSelected = (_editingPlaylist.playWhen.scene & scene) == scene;
                    if (GUILayout.Toggle(isSelected, val) != isSelected)
                    {
                        if (val == "Any")
                        {
                            if (_editingPlaylist.playWhen.scene != Enums.Scenes.Any)
                                _editingPlaylist.playWhen.scene = Enums.Scenes.Any;
                            else
                                _editingPlaylist.playWhen.scene = 0;
                        }
                        else
                            _editingPlaylist.playWhen.scene = _editingPlaylist.playWhen.scene ^ scene;
                    }
                }

                // Footer
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(" OK "))
                    _scenesExpanded = false;
                if (GUILayout.Button("Cancel"))
                {
                    _scenesExpanded = false;
                    _editingPlaylist.playWhen.scene = _previousScene;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.EndVertical();
            }
            else
            {
                string selection;
                if (_editingPlaylist.playWhen.scene == 0)
                    selection = "None";
                else
                    selection = _editingPlaylist.playWhen.scene.ToString();

                _scenesExpanded = PickerGuiCollapsed("Scene", selection, _scenesExpanded);
            }
        }

        private bool _cameraModeExpanded = false;
        private Enums.CameraModes _previousCameraMode = 0;
        private void CameraModePicker()
        {
            if (_cameraModeExpanded)
            {
                GUILayout.Label("<b>Camera Mode</b>");
                GUILayout.BeginVertical();
                foreach (var e in Enum.GetValues(typeof(Enums.CameraModes)))
                {
                    string val = e.ToString();
                    Enums.CameraModes cameraMode = (Enums.CameraModes)e;

                    bool isSelected = (_editingPlaylist.playWhen.cameraMode & cameraMode) == cameraMode;
                    if (GUILayout.Toggle(isSelected, val) != isSelected)
                    {
                        if (val == "Any")
                        {
                            if ((int)_editingPlaylist.playWhen.cameraMode != 0)
                                _editingPlaylist.playWhen.cameraMode = Enums.CameraModes.Any;
                            else
                                _editingPlaylist.playWhen.cameraMode = 0;
                        }
                        else
                            _editingPlaylist.playWhen.cameraMode = _editingPlaylist.playWhen.cameraMode ^ cameraMode;
                    }
                }

                // Footer
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(" OK "))
                    _cameraModeExpanded = false;
                if (GUILayout.Button("Cancel"))
                {
                    _cameraModeExpanded = false;
                    _editingPlaylist.playWhen.cameraMode = _previousCameraMode;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.EndVertical();
            }
            else
            {

                string selection;
                if (_editingPlaylist.playWhen.cameraMode == 0)
                    selection = "None";
                else
                    selection = _editingPlaylist.playWhen.cameraMode.ToString();

                _cameraModeExpanded = PickerGuiCollapsed("Camera Mode", selection, _cameraModeExpanded);
            }
        }

        private bool _situationExpanded = false;
        private Vessel.Situations _previousSituation = 0;
        private void SituationPicker()
        {
            if (_situationExpanded)
            {
                GUILayout.Label("<b>Situation</b>");
                GUILayout.BeginVertical();
                foreach (var e in Enum.GetValues(typeof(Vessel.Situations)))
                {
                    bool isSelected = (_editingPlaylist.playWhen.situation & (Vessel.Situations)e) == (Vessel.Situations)e;
                    if (GUILayout.Toggle(isSelected, e.ToString()) != isSelected)
                        _editingPlaylist.playWhen.situation = _editingPlaylist.playWhen.situation ^ (Vessel.Situations)e;
                }

                // Footer
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(" OK "))
                    _situationExpanded = false;
                if (GUILayout.Button("Cancel"))
                {
                    _situationExpanded = false;
                    _editingPlaylist.playWhen.situation = _previousSituation;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.EndVertical();
            }
            else
            {
                string selection;
                if ((int)_editingPlaylist.playWhen.situation == 0xFF)
                    selection = "Any";
                else
                    selection = _editingPlaylist.playWhen.situation.ToString();
                _situationExpanded = PickerGuiCollapsed("Situation", selection, _situationExpanded);
            }
        }

        private bool _timeOfDayExpanded = false;
        private Enums.TimesOfDay _previousTimeOfDay = 0;
        private void TimeOfDayPicker()
        {
            if (_timeOfDayExpanded)
            {
                GUILayout.Label("<b>Time of Day (KSC)</b>");
                GUILayout.BeginVertical();
                foreach (var e in Enum.GetValues(typeof(Enums.TimesOfDay)))
                {
                    bool isSelected = (_editingPlaylist.playWhen.timeOfDay & (Enums.TimesOfDay)e) == (Enums.TimesOfDay)e;
                    string val = e.ToString();
                    if (GUILayout.Toggle(isSelected, val) != isSelected)
                    {
                        if (val == "Any")
                        {
                            if ((int)_editingPlaylist.playWhen.timeOfDay != 0)
                                _editingPlaylist.playWhen.timeOfDay = Enums.TimesOfDay.Any;
                            else
                                _editingPlaylist.playWhen.timeOfDay = 0;
                        }
                        else
                            _editingPlaylist.playWhen.timeOfDay = _editingPlaylist.playWhen.timeOfDay ^ (Enums.TimesOfDay)e;
                    }
                }

                // Footer
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(" OK "))
                    _timeOfDayExpanded = false;
                if (GUILayout.Button("Cancel"))
                {
                    _timeOfDayExpanded = false;
                    _editingPlaylist.playWhen.timeOfDay = _previousTimeOfDay;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.EndVertical();
            }
            else
                _timeOfDayExpanded = PickerGuiCollapsed("Time of Day (KSC)", _editingPlaylist.playWhen.timeOfDay.ToString(), _timeOfDayExpanded);
        }

        private bool _inAtmosphereExpanded = false;
        private Enums.Selector _previousInAtmosphere = 0;
        private void InAtmospherePicker()
        {
            if (_inAtmosphereExpanded)
            {
                GUILayout.Label("<b>In Atmosphere</b>");
                GUILayout.BeginVertical();

                if (GUILayout.Toggle(_editingPlaylist.playWhen.inAtmosphere == Enums.Selector.No, "No"))
                    _editingPlaylist.playWhen.inAtmosphere = Enums.Selector.No;
                if (GUILayout.Toggle(_editingPlaylist.playWhen.inAtmosphere == Enums.Selector.Yes, "Yes"))
                    _editingPlaylist.playWhen.inAtmosphere = Enums.Selector.Yes;
                if (GUILayout.Toggle(_editingPlaylist.playWhen.inAtmosphere == Enums.Selector.Either, "Either"))
                    _editingPlaylist.playWhen.inAtmosphere = Enums.Selector.Either;

                // Footer
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(" OK "))
                    _inAtmosphereExpanded = false;
                if (GUILayout.Button("Cancel"))
                {
                    _inAtmosphereExpanded = false;
                    _editingPlaylist.playWhen.inAtmosphere = _previousInAtmosphere;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.EndVertical();
            }
            else
                _inAtmosphereExpanded = PickerGuiCollapsed("In Atmosphere", _editingPlaylist.playWhen.inAtmosphere.ToString(), _inAtmosphereExpanded);
        }


        private bool _fadeEditorVisible = false;
        private bool _crossfade = false;
        private float _fadeIn = 0;
        private string _fadeInText = "";
        private float _fadeOut = 0;
        private string _fadeOutText = "";
        private void FadeEditor()
        {
            if (_fadeEditorVisible)
            {
                GUILayout.Label("<b>Fading</b>");
                GUILayout.BeginVertical();
                _fadeInText = GuiUtils.editFloat("Fade in time (s):", _fadeInText, out _fadeIn);
                _fadeOutText = GuiUtils.editFloat("Fade out time (s):", _fadeOutText, out _fadeOut);
                _crossfade = GUILayout.Toggle(_crossfade, "Crossfade");

                // Footer
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(" OK "))
                {
                    _editingPlaylist.fade.crossfade = _crossfade;
                    _editingPlaylist.fade.fadeIn = _fadeIn;
                    _editingPlaylist.fade.fadeOut = _fadeOut;
                    _fadeEditorVisible = false;
                }
                if (GUILayout.Button("Cancel"))
                {
                    _fadeEditorVisible = false;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Fading");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("  ...  "))
                {
                    _fadeEditorVisible = true;
                    _crossfade = _editingPlaylist.fade.crossfade;
                    _fadeIn = _editingPlaylist.fade.fadeIn;
                    _fadeOut = _editingPlaylist.fade.fadeOut;
                }
                GUILayout.EndHorizontal();
            }
        }

        private bool _trackPickerVisible = false;
        private void TrackPicker()
        {
            for (int i = 0; i < GameDatabase.Instance.databaseAudio.Count; i++)
            {
                TrackPickerGui(GameDatabase.Instance.databaseAudio[i].name, GameDatabase.Instance.databaseAudio[i]);
            }

            for (int i = 0; i < _audioFileList.Count; i++)
            {
                TrackPickerGui(_audioFileList[i].Name, null);
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void TrackPickerGui(string trackName, AudioClip clip)
        {
            GUILayout.BeginHorizontal();

            string labelText = trackName;
            if (_editingPlaylist.tracks.Contains(trackName))
                labelText = "<b>" + labelText + "</b>";
            GUILayout.Label(labelText);
            GUILayout.FlexibleSpace();
            if (clip != null)
                PreviewClipButton(clip);
            if (GUILayout.Button("   Add   "))
                _editingPlaylist.tracks.Add(trackName);
            GUILayout.EndHorizontal();
        }

        private bool _playNextPickerVisible = false;
        private void PlayNextPicker()
        {
            if (_playNextPickerVisible)
            {
                GUILayout.Label("<b>Next Playlist</b>");
                _playNextPickerVisible = PlaylistPicker();
                _editingPlaylist.playNext = _chosenPlaylist == null ? "" : _chosenPlaylist.name;
            }
            else
            {
                _playNextPickerVisible = PickerGuiCollapsed("Next Playlist",
                    String.IsNullOrEmpty(_editingPlaylist.playNext) ? "None" : _editingPlaylist.playNext, _playNextPickerVisible);
            }
        }

        private bool _playBeforePickerVisible = false;
        private void PlayBeforePicker()
        {
            if (_playBeforePickerVisible)
            {
                GUILayout.Label("<b>Sort This Playlist Before</b>");
                _playBeforePickerVisible = PlaylistPicker();
                _editingPlaylist.playBefore = _chosenPlaylist == null ? "" : _chosenPlaylist.name;
            }
            else
            {
                _playBeforePickerVisible = PickerGuiCollapsed("Sort This Playlist Before",
                    String.IsNullOrEmpty(_editingPlaylist.playBefore) ? "None" : _editingPlaylist.playBefore, _playBeforePickerVisible);
            }
        }

        private bool _playAfterPickerVisible = false;
        private void PlayAfterPicker()
        {
            if (_playAfterPickerVisible)
            {
                GUILayout.Label("<b>Sort This Playlist After</b>");
                _playAfterPickerVisible = PlaylistPicker();
                _editingPlaylist.playAfter = _chosenPlaylist == null ? "" : _chosenPlaylist.name;
            }
            else
            {
                _playAfterPickerVisible = PickerGuiCollapsed("Sort This Playlist After",
                    String.IsNullOrEmpty(_editingPlaylist.playAfter) ? "None" : _editingPlaylist.playAfter, _playAfterPickerVisible);
            }
        }

        private Playlist _chosenPlaylist = null;
        private bool PlaylistPicker()
        {
            bool isVisible = true;
            SoundtrackEditor sted = SoundtrackEditor.Instance;
            for (int i = 0; i < sted.Playlists.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Playlist currentPlaylist = sted.Playlists[i];
                GUILayout.Label("   " + currentPlaylist.name);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("  Select  "))
                {
                    _chosenPlaylist = currentPlaylist;
                    isVisible = false;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("   None  "))
            {
                _chosenPlaylist = null;
                isVisible = false;
            }
            if (GUILayout.Button("   Cancel  "))
                isVisible = false;
            GUILayout.EndHorizontal();
            return isVisible;
        }

    } // End of class
}
