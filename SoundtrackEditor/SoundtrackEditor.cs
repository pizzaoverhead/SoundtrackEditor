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


        public void Start()
        {
            music = MusicLogic.fetch;

            Load();
            UnloadUnusedTracks();

            // Remove positional effects.
            music.audio1.panLevel = 0;
            music.audio1.dopplerLevel = 0;
            music.audio2.panLevel = 0;
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
                    // TODO: The unusedTracks list is incorrect.
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
                        foreach (string t in spaceTracks)
                        {
                            clip = GameDatabase.Instance.GetAudioClip(t);
                            if (clip != null)
                            {
                                Debug.Log("STED: Adding space track " + clip.name);
                                music.spacePlaylist.Add(clip);
                                unusedTracks.Remove(clip.name);
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

                if (config.HasValue("astroComplexAmbience"))
                {
                    track = config.GetValue("astroComplexAmbience");
                    clip = GameDatabase.Instance.GetAudioClip(track);
                    if (clip != null)
                    {
                        music.astroComplexAmbience = clip;
                        unusedTracks.Remove(clip.name);
                    }
                }
                if (config.HasValue("credits"))
                {
                    track = config.GetValue("credits");
                    clip = GameDatabase.Instance.GetAudioClip(track);
                    if (clip != null)
                    {
                        music.credits = clip;
                        unusedTracks.Remove(clip.name);
                    }
                }
                if (config.HasValue("menuAmbience"))
                {
                    track = config.GetValue("menuAmbience");
                    clip = GameDatabase.Instance.GetAudioClip(track);
                    if (clip != null)
                    {
                        music.menuAmbience = clip;
                        unusedTracks.Remove(clip.name);
                    }
                }
                if (config.HasValue("menuTheme"))
                {
                    track = config.GetValue("menuTheme");
                    clip = GameDatabase.Instance.GetAudioClip(track);
                    if (clip != null)
                    {
                        music.menuTheme = clip;
                        unusedTracks.Remove(clip.name);
                    }
                }
                if (config.HasValue("researchComplexAmbience"))
                {
                    track = config.GetValue("researchComplexAmbience");
                    clip = GameDatabase.Instance.GetAudioClip(track);
                    if (clip != null)
                    {
                        music.researchComplexAmbience = clip;
                        unusedTracks.Remove(clip.name);
                    }
                }
                if (config.HasValue("spaceCenterAmbience"))
                {
                    track = config.GetValue("spaceCenterAmbience");
                    clip = GameDatabase.Instance.GetAudioClip(track);
                    if (clip != null)
                    {
                        music.spaceCenterAmbience = clip;
                        unusedTracks.Remove(clip.name);
                    }
                }
                if (config.HasValue("SPHAmbience"))
                {
                    track = config.GetValue("SPHAmbience");
                    clip = GameDatabase.Instance.GetAudioClip(track);
                    if (clip != null)
                    {
                        music.SPHAmbience = clip;
                        unusedTracks.Remove(clip.name);
                    }
                }
                if (config.HasValue("trackingAmbience"))
                {
                    track = config.GetValue("trackingAmbience");
                    clip = GameDatabase.Instance.GetAudioClip(track);
                    if (clip != null)
                    {
                        music.trackingAmbience = clip;
                        unusedTracks.Remove(clip.name);
                    }
                }
                if (config.HasValue("VABAmbience"))
                {
                    track = config.GetValue("VABAmbience");
                    clip = GameDatabase.Instance.GetAudioClip(track);
                    if (clip != null)
                    {
                        music.VABAmbience = clip;
                        unusedTracks.Remove(clip.name);
                    }
                }

                // Write out the current settings for future user editing.
                Save();
                LoadMp3s();
            }
            catch (Exception ex)
            {
                Debug.LogError("STED error: " + ex.Message);
            }
        }

        private void Save()
        {
            config = new ConfigNode();
            ConfigNode spacePlaylist = new ConfigNode("SpacePlaylist");
            for (int i = 0; i < music.spacePlaylist.Count; i++)
            {
                spacePlaylist.AddValue("track", music.spacePlaylist[i].name);
            }
            config.AddNode(spacePlaylist);
            ConfigNode constructionPlaylist = new ConfigNode("ConstructionPlaylist");
            for (int i = 0; i < music.constructionPlaylist.Count; i++)
            {
                constructionPlaylist.AddValue("track", music.constructionPlaylist[i].name);
            }
            config.AddNode(constructionPlaylist);
            config.AddValue("astroComplexAmbience", music.astroComplexAmbience.name);
            config.AddValue("credits", music.credits.name);
            config.AddValue("menuAmbience", music.menuAmbience.name);
            config.AddValue("menuTheme", music.menuTheme.name);
            config.AddValue("researchComplexAmbience", music.researchComplexAmbience.name);
            config.AddValue("spaceCenterAmbience", music.spaceCenterAmbience.name);
            config.AddValue("SPHAmbience", music.SPHAmbience.name);
            config.AddValue("trackingAmbience", music.trackingAmbience.name);
            config.AddValue("VABAmbience", music.VABAmbience.name);

            ConfigNode unusedTrackNode = new ConfigNode("UnusedTracks");
            foreach (string t in unusedTracks)
                unusedTrackNode.AddValue("track", t);
            config.AddNode(unusedTrackNode);

            config.Save(configSavePath);
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
            foreach (string trackName in unusedTracks)
            {
                GameDatabase.Instance.RemoveAudioClip(trackName);
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
            // Check all files in the Music directory; find the mp3s.
            DirectoryInfo modDir = Directory.GetParent(AssemblyDirectory);
            string path = modDir.FullName + @"\Music\";
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(file);
                if (!String.IsNullOrEmpty(ext) && ext.Equals(".mp3", StringComparison.InvariantCultureIgnoreCase))
                {
                    Debug.Log("STED: Running MP3 importer...");
                    MP3Import importer = new MP3Import();
                    GameDatabase.Instance.databaseAudio.Add(importer.StartImport(file));
                    Debug.Log("STED: Finished importing MP3.");
                }
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
