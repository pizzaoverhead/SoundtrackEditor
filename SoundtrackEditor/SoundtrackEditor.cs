using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SoundtrackEditor
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class SoundtrackEditor : MonoBehaviour
    {
        // TODO: Find time of day, play night sounds.
        //SunLight sl = (SunLight)FindObjectOfType(typeof(SunLight));
        //float x = sl.timeOfTheDay;
        MusicLogic music;
        ConfigNode config;
        string configSavePath = "GameData/SoundtrackEditor/settings.cfg";
        List<string> unusedTracks = new List<string>();
        // Create a new empty audio clip to replace the stock ones when they are disabled.
        AudioClip emptyTrack = AudioClip.Create("none", 44100, 1, 44100, true);

        public void Start()
        {
            music = MusicLogic.fetch;

            Load();
            UnloadUnusedTracks();

            // Remove positional effects.
            //music.audio1.panLevel = 0;
            music.audio1.dopplerLevel = 0;
            //music.audio2.panLevel = 0;
            music.audio2.dopplerLevel = 0;
        }

        private void Load()
        {
            try
            {
                LoadMp3s();

                AudioClip clip = null;
                string track = String.Empty;
                unusedTracks.Clear();

                config = ConfigNode.Load(configSavePath);
                if (config == null)
                {
                    Debug.LogWarning("STED -- No config file present.");
                    unusedTracks.AddRange(GetUserTrackNames());
                    Save();
                    return;
                }

                unusedTracks.AddRange(GetAllTrackNames());

                // Add the stock sounds to the database so we can access them later.
                GameDatabase.Instance.databaseAudio.AddRange(music.constructionPlaylist);
                GameDatabase.Instance.databaseAudio.AddRange(music.spacePlaylist);
                GameDatabase.Instance.databaseAudio.Add(music.astroComplexAmbience);
                GameDatabase.Instance.databaseAudio.Add(music.credits);
                GameDatabase.Instance.databaseAudio.Add(music.menuAmbience);
                GameDatabase.Instance.databaseAudio.Add(music.menuTheme);
                GameDatabase.Instance.databaseAudio.Add(music.researchComplexAmbience);
                GameDatabase.Instance.databaseAudio.Add(music.spaceCenterAmbience);
                GameDatabase.Instance.databaseAudio.Add(music.SPHAmbience);
                GameDatabase.Instance.databaseAudio.Add(music.trackingAmbience);
                GameDatabase.Instance.databaseAudio.Add(music.VABAmbience);

                if (config.HasNode("SpacePlaylist"))
                {
                    ConfigNode spacePlaylist = config.GetNode("SpacePlaylist");
                    string[] spaceTracks = spacePlaylist.GetValues("track");
                    if (spaceTracks.Length > 0)
                    {
                        music.spacePlaylist.Clear();

                        if (spaceTracks.Length != 1 &&
                            !spaceTracks[0].Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (string t in spaceTracks)
                            {
                                clip = GameDatabase.Instance.GetAudioClip(t);
                                if (clip != null)
                                {
                                    music.spacePlaylist.Add(clip);
                                    unusedTracks.Remove(clip.name);
                                }
                            }
                        }
                    }
                }
                if (config.HasNode("ConstructionPlaylist"))
                {
                    ConfigNode constructionPlaylist = config.GetNode("ConstructionPlaylist");
                    string[] constructionTracks = constructionPlaylist.GetValues("track");
                    if (constructionTracks.Length > 0)
                    {
                        music.constructionPlaylist.Clear();

                        if (constructionTracks.Length != 1 &&
                            !constructionTracks[0].Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (string t in constructionTracks)
                            {
                                clip = GameDatabase.Instance.GetAudioClip(t);
                                if (clip != null)
                                {
                                    music.constructionPlaylist.Add(clip);
                                    unusedTracks.Remove(clip.name);
                                }
                            }
                        }
                    }
                }

                if (config.HasValue("astroComplexAmbience"))
                {
                    track = config.GetValue("astroComplexAmbience");
                    if (track.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        music.astroComplexAmbience = emptyTrack;
                    else
                    {
                        clip = GameDatabase.Instance.GetAudioClip(track);
                        if (clip != null)
                        {
                            music.astroComplexAmbience = clip;
                            unusedTracks.Remove(clip.name);
                        }
                    }
                }
                if (config.HasValue("credits"))
                {
                    track = config.GetValue("credits");
                    if (track.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        music.credits = emptyTrack;
                    else
                    {
                        clip = GameDatabase.Instance.GetAudioClip(track);
                        if (clip != null)
                        {
                            music.credits = clip;
                            unusedTracks.Remove(clip.name);
                        }
                    }
                }
                if (config.HasValue("menuAmbience"))
                {
                    track = config.GetValue("menuAmbience");
                    if (track.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        music.menuAmbience = emptyTrack;
                    else
                    {
                        clip = GameDatabase.Instance.GetAudioClip(track);
                        if (clip != null)
                        {
                            music.menuAmbience = clip;
                            unusedTracks.Remove(clip.name);
                        }
                    }
                }
                if (config.HasValue("menuTheme"))
                {
                    track = config.GetValue("menuTheme");
                    if (track.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        music.menuTheme = emptyTrack;
                    else
                    {
                        clip = GameDatabase.Instance.GetAudioClip(track);
                        if (clip != null)
                        {
                            music.menuTheme = clip;
                            unusedTracks.Remove(clip.name);
                        }
                    }
                }
                if (config.HasValue("researchComplexAmbience"))
                {
                    track = config.GetValue("researchComplexAmbience");
                    if (track.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        music.researchComplexAmbience = emptyTrack;
                    else
                    {
                        clip = GameDatabase.Instance.GetAudioClip(track);
                        if (clip != null)
                        {
                            music.researchComplexAmbience = clip;
                            unusedTracks.Remove(clip.name);
                        }
                    }
                }
                if (config.HasValue("spaceCenterAmbience"))
                {
                    track = config.GetValue("spaceCenterAmbience");
                    if (track.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        music.spaceCenterAmbience = emptyTrack;
                    else
                    {
                        clip = GameDatabase.Instance.GetAudioClip(track);
                        if (clip != null)
                        {
                            music.spaceCenterAmbience = clip;
                            unusedTracks.Remove(clip.name);
                        }
                    }
                }
                if (config.HasValue("SPHAmbience"))
                {
                    track = config.GetValue("SPHAmbience");
                    if (track.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        music.SPHAmbience = emptyTrack;
                    else
                    {
                        clip = GameDatabase.Instance.GetAudioClip(track);
                        if (clip != null)
                        {
                            music.SPHAmbience = clip;
                            unusedTracks.Remove(clip.name);
                        }
                    }
                }
                if (config.HasValue("trackingAmbience"))
                {
                    track = config.GetValue("trackingAmbience");
                    if (track.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        music.trackingAmbience = emptyTrack;
                    else
                    {
                        clip = GameDatabase.Instance.GetAudioClip(track);
                        if (clip != null)
                        {
                            music.trackingAmbience = clip;
                            unusedTracks.Remove(clip.name);
                        }
                    }
                }
                if (config.HasValue("VABAmbience"))
                {
                    track = config.GetValue("VABAmbience");
                    if (track.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        music.VABAmbience = emptyTrack;
                    else
                    {
                        clip = GameDatabase.Instance.GetAudioClip(track);
                        if (clip != null)
                        {
                            music.VABAmbience = clip;
                            unusedTracks.Remove(clip.name);
                        }
                    }
                }

                // Write out the current settings for future user editing.
                Save();
            }
            catch (Exception ex)
            {
                Debug.LogError("STED Load error: " + ex.Message);
            }
        }

        private void Save()
        {
            try
            {
                config = new ConfigNode();

                ConfigNode spacePlaylist = new ConfigNode("SpacePlaylist");
                if (music.spacePlaylist.Count > 0)
                {
                    for (int i = 0; i < music.spacePlaylist.Count; i++)
                    {
                        spacePlaylist.AddValue("track", music.spacePlaylist[i].name);
                    }
                }
                else
                    spacePlaylist.AddValue("track", "none");
                config.AddNode(spacePlaylist);

                ConfigNode constructionPlaylist = new ConfigNode("ConstructionPlaylist");
                if (music.constructionPlaylist.Count > 0)
                {
                    for (int i = 0; i < music.constructionPlaylist.Count; i++)
                    {
                        constructionPlaylist.AddValue("track", music.constructionPlaylist[i].name);
                    }
                }
                else
                    constructionPlaylist.AddValue("track", "none");
                config.AddNode(constructionPlaylist);

                // Set the track names, or return "none" if they are null.
                string trackName = String.Empty;
                if (music.astroComplexAmbience != null)
                    trackName = music.astroComplexAmbience.name ?? "none";
                else
                    trackName = "none";
                config.AddValue("astroComplexAmbience", trackName);

                if (music.credits != null)
                    trackName = music.credits.name ?? "none";
                else
                    trackName = "none";
                config.AddValue("credits", trackName);

                if (music.menuAmbience != null)
                    trackName = music.menuAmbience.name ?? "none";
                else
                    trackName = "none";
                config.AddValue("menuAmbience", trackName);

                if (music.menuTheme != null)
                    trackName = music.menuTheme.name ?? "none";
                else
                    trackName = "none";
                config.AddValue("menuTheme", trackName);

                if (music.researchComplexAmbience != null)
                    trackName = music.researchComplexAmbience.name ?? "none";
                else
                    trackName = "none";
                config.AddValue("researchComplexAmbience", trackName);

                if (music.spaceCenterAmbience != null)
                    trackName = music.spaceCenterAmbience.name ?? "none";
                else
                    trackName = "none";
                config.AddValue("spaceCenterAmbience", trackName);

                if (music.SPHAmbience != null)
                    trackName = music.SPHAmbience.name ?? "none";
                else
                    trackName = "none";
                config.AddValue("SPHAmbience", trackName);

                if (music.trackingAmbience != null)
                    trackName = music.trackingAmbience.name ?? "none";
                else
                    trackName = "none";
                config.AddValue("trackingAmbience", trackName);

                if (music.VABAmbience != null)
                    trackName = music.VABAmbience.name ?? "none";
                else
                    trackName = "none";
                config.AddValue("VABAmbience", trackName);

                ConfigNode unusedTrackNode = new ConfigNode("UnusedTracks");
                foreach (string t in unusedTracks)
                {
                    if (!t.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                        unusedTrackNode.AddValue("track", t);
                }
                config.AddNode(unusedTrackNode);

                config.Save(configSavePath);
            }
            catch (Exception ex)
            {
                Debug.LogError("STED Save error: " + ex.Message);
            }
        }

        private List<string> GetUserTrackNames()
        {
            List<AudioClip> userTracks = GameDatabase.Instance.databaseAudio.FindAll(c => c.ToString().StartsWith("SoundtrackEditor/"));
            return userTracks.ConvertAll(c => c.name);
        }

        private List<string> GetAllTrackNames()
        {
            List<string> trackNames = new List<string>();
            trackNames.AddRange(GetUserTrackNames());
            trackNames.AddRange(music.constructionPlaylist.ConvertAll(t => t.name));
            trackNames.AddRange(music.spacePlaylist.ConvertAll(t => t.name));
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

        private void UnloadUnusedTracks()
        {
            try
            {
                foreach (string trackName in unusedTracks)
                {
                    GameDatabase.Instance.RemoveAudioClip(trackName);
                }

                Resources.UnloadUnusedAssets();
            }
            catch (Exception ex)
            {
                Debug.LogError("STED Unload Tracks error: " + ex.Message);
            }
        }

        public static string AssemblyDirectory
        {
            // See: http://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in/283917#283917
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase; // file:///E:/Games/Kerbal Space Program/GameData/SoundtrackEditor/Plugins/SoundtrackEditor.dll
                UriBuilder uri = new UriBuilder(codeBase); // file://E:/Games/Kerbal%20Space%20Program/GameData/SoundtrackEditor/Plugins/SoundtrackEditor.dll
                string path = Uri.UnescapeDataString(uri.Path); // E:/Games/Kerbal Space Program/GameData/SoundtrackEditor/Plugins/SoundtrackEditor.dll
                return Path.GetDirectoryName(path); // E:/Games/Kerbal Space Program/GameData/SoundtrackEditor/Plugins
            }
        }

        /// <summary>
        /// Adds any MP3s in the SoundtrackEditory\Music\ directory to the GameDatabase.
        /// </summary>
        /// <remarks>
        /// Unity no longer supports loading MP3 tracks, giving the error:
        ///    "Streaming of 'mp3' on this platform is not supported"
        /// Instead, this is achieved using the MPG123 utility to load the MP3 
        /// data unto a Unity AudioClip. For details, see here:
        /// http://answers.unity3d.com/questions/380838/is-there-any-converter-mp3-to-ogg-.html
        /// </remarks>
        private void LoadMp3s()
        {
            try
            {
                // Check all files in the Music directory; find the mp3s.
                Debug.Log("STED: Running MP3 importer...");
                DirectoryInfo modDir = Directory.GetParent(AssemblyDirectory);
                string path = Path.Combine(modDir.FullName, "Music") + Path.DirectorySeparatorChar;
                foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    string ext = Path.GetExtension(file);
                    if (!String.IsNullOrEmpty(ext) && ext.Equals(".mp3", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MP3Import importer = new MP3Import();
                        AudioClip clip = importer.StartImport(file);

                        // Set the clip name to match the format used for clips in the GameDatabase.
                        string clipShortPath = Path.GetFileName(modDir.FullName) +
                            file.Substring(modDir.FullName.Length, file.Length - modDir.FullName.Length - ".mp3".Length);
                        if (Path.DirectorySeparatorChar == '\\') // Change Windows path separators to match the GameDatabase.
                            clipShortPath = clipShortPath.Replace('\\', '/');


                        clip.name = clipShortPath;
                        GameDatabase.Instance.databaseAudio.Add(clip);
                    }
                }
                Debug.Log("STED: Finished importing MP3s.");
            }
            catch (Exception ex)
            {
                Debug.LogError("STED MP3 Import error: " + ex.Message);
            }
        }
    }

    /*public class Soundtrack
    {
        public List<string> SpaceTracks { get; set; }
        public List<string> ConstructionTracks { get; set; }
        public string AstroComplexAmbience { get; set; }
        public string Credits { get; set; }
        public string MenuAmbience { get; set; }
        public string MenuTheme { get; set; }
        public string ResearchComplexAmbience { get; set; }
        public string SpaceCenterAmbience { get; set; }
        public string SPHAmbience { get; set; }
        public string TrackingAmbience { get; set; }
        public string VABAmbience { get; set; }

        public Soundtrack()
        {
            SpaceTracks = new List<string>();
            ConstructionTracks = new List<string>();
            AstroComplexAmbience = String.Empty;
            Credits = String.Empty;
            MenuAmbience = String.Empty;
            MenuTheme = String.Empty;
            ResearchComplexAmbience = String.Empty;
            SpaceCenterAmbience = String.Empty;
            SPHAmbience = String.Empty;
            TrackingAmbience = String.Empty;
            VABAmbience = String.Empty;
        }
    }*/

    /* Sound areas:
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
     */
}
