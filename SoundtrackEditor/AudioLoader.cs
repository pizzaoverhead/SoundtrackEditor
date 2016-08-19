using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace SoundtrackEditor
{
    public class AudioLoader
    {
        private List<string> unusedTracks = new List<string>();

        /// <summary>
        /// Loads an audio clip from disk or the game database.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AudioClip GetAudioClip(string name)
        {
            //DirectoryInfo modDir = Directory.GetParent(AssemblyDirectory);
            //modDir.Parent = GameData
            //modDir.Parent.Parent = KSP root
            //Utils.Log("#Mod dir: " + modDir);
            //Utils.Log("#Grandparent: " + modDir.Parent.Parent.FullName);
            //string path = Path.Combine(modDir.Parent.Parent.FullName, "Music") + Path.DirectorySeparatorChar;

            // Ensure the Music directory exists.
            Directory.CreateDirectory(Utils.MusicPath);

            foreach (string file in Directory.GetFiles(Utils.MusicPath, "*", SearchOption.AllDirectories))
            {
                if (name.Equals(Path.GetFileNameWithoutExtension(file)))
                {
                    string ext = Path.GetExtension(file);
                    Utils.Log("Found " + name + ", with extension " + ext);
                    switch (ext.ToUpperInvariant())
                    {
                        case ".WAV":
                        case ".OGG":
                            return LoadUnityAudioClip(file);
                        case ".MP3":
                            return LoadMp3Clip(file);
                        default:
                            Utils.Log("Unknown extension found: " + ext);
                            break;
                    }
                }
            }
            // Failed to find the clip in the music folder. Check the game database instead.
            Utils.Log("Attempting to load game database file: " + name);
            /*foreach (var a in GameDatabase.Instance.databaseAudio)
            {
                Debug.Log("DBA: " + a.name);
            }*/
            return GameDatabase.Instance.GetAudioClip(name);
        }

        public static List<AudioFileInfo> GetAvailableFiles()
        {
            List<AudioFileInfo> files = new List<AudioFileInfo>();

            // Ensure the Music directory exists.
            Directory.CreateDirectory(Utils.MusicPath);

            foreach (string file in Directory.GetFiles(Utils.MusicPath, "*", SearchOption.AllDirectories))
            {
                AudioFileInfo fileInfo = new AudioFileInfo
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Path = file,
                    FileExtension = Path.GetExtension(file)
                };
                files.Add(fileInfo);
            }
            return files;
        }

        private static AudioClip LoadUnityAudioClip(string filePath)
        {
            try
            {
                Utils.Log("Loading Unity clip");
                // Load the audio clip into memory.
                WWW www = new WWW("file://" + filePath);
                AudioClip clip = www.audioClip;
                clip.name = Path.GetFileNameWithoutExtension(filePath);
                Utils.Log("Clip name: " + clip.name + ", load state: " + clip.loadState);
                return clip;
            }
            catch (Exception ex)
            {
                Debug.LogError("[STED] Error loading audio file " + filePath + ": " + ex.Message + "\r\n" + ex.StackTrace);
                return null;
            }
        }

        private static AudioClip LoadMp3Clip(string filePath)
        {
            try
            {
                Utils.Log("Loading MP3 clip");
                MP3Import importer = new MP3Import();
                AudioClip clip = importer.StartImport(filePath);
                clip.name = Path.GetFileNameWithoutExtension(filePath);
                return clip;
            }
            catch (Exception ex)
            {
                // TODO: Don't continually attempt to load failing tracks when they're the only one in the playlist.
                Debug.LogError("[STED] Error loading MP3 " + filePath + ": " + ex.Message + "\r\n" + ex.StackTrace);
                return null;
            }
        }

        private void UnloadUnusedTracks()
        {
            try
            {
                Utils.Log("Unloading Tracks...");
                int i = 0;
                foreach (string trackName in unusedTracks)
                {
                    //Destroy(GameDatabase.Instance.GetAudioClip(trackName));
                    //DestroyImmediate(GameDatabase.Instance.GetAudioClip(trackName), true);
                    //DestroyImmediate(GameDatabase.Instance.databaseAudio.Find(c => c.name.Equals(trackName)));
                    GameDatabase.Instance.RemoveAudioClip(trackName);
                    i++;
                }
                Resources.UnloadUnusedAssets();
                Utils.Log(i + " tracks unloaded.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[STED] Unload Tracks error: " + ex.Message);
            }
        }

        public struct AudioFileInfo
        {
            public string Name;
            public string Path;
            public string FileExtension;
        }

        /*// <summary>
        /// Adds any MP3s in the SoundtrackEditory\Music\ directory to the GameDatabase.
        /// </summary>
        /// <remarks>
        /// Unity no longer supports loading MP3 tracks, giving the error:
        ///    "Streaming of 'mp3' on this platform is not supported"
        /// Instead, this is achieved using the MPG123 utility to load the MP3 
        /// data unto a Unity AudioClip. For details, see here:
        /// http://answers.unity3d.com/questions/380838/is-there-any-converter-mp3-to-ogg-.html
        /// </remarks>
        private void LoadAudioFiles()
        {
            try
            {
                // Check all files in the Music directory; find the mp3s.
                /* GameData directory path.
                 * DirectoryInfo modDir = Directory.GetParent(AssemblyDirectory);
                string path = Path.Combine(modDir.FullName, "Music") + Path.DirectorySeparatorChar;*
                string path = MusicPath;
                foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    string ext = Path.GetExtension(file);
                    if (!String.IsNullOrEmpty(ext))
                    {
                        switch (ext.ToLowerInvariant())
                        {
                            case ".aif":
                            case ".ogg":
                            case ".wav":
                                {
                                    WWW www = new WWW("file://" + file);
                                    AudioClip clip = www.GetAudioClip(false);
                                    GameDatabase.Instance.databaseAudio.Add(clip);
                                    break;
                                }
                            case ".mp3":
                                {
                                    MP3Import importer = new MP3Import();
                                    AudioClip clip = importer.StartImport(file);

                                    /* Set the clip name to match the format used for clips in the GameDatabase.
                                    string clipShortPath = Path.GetFileName(modDir.FullName) +
                                        file.Substring(modDir.FullName.Length, file.Length - modDir.FullName.Length - ".mp3".Length);
                                    if (Path.DirectorySeparatorChar == '\\') // Change Windows path separators to match the GameDatabase.
                                        clipShortPath = clipShortPath.Replace('\\', '/');
                                    clip.name = clipShortPath;*
                                    GameDatabase.Instance.databaseAudio.Add(clip);
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError("STED: Load Audio Files error: " + ex.Message);
            }
        }*/
    }
}
