using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace SoundtrackEditor
{
    class Utils
    {
        public static void Log(string message)
        {
            Debug.Log("[STED] " + message);
        }

        public static string AssemblyDirectory
        {
            // See: http://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in/283917#283917
            get
            {
                var x = Directory.GetParent(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Path))).Parent.Parent;
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase; // file:///E:/Games/Kerbal Space Program/GameData/SoundtrackEditor/Plugins/SoundtrackEditor.dll
                UriBuilder uri = new UriBuilder(codeBase); // file://E:/Games/Kerbal%20Space%20Program/GameData/SoundtrackEditor/Plugins/SoundtrackEditor.dll
                string path = Uri.UnescapeDataString(uri.Path); // E:/Games/Kerbal Space Program/GameData/SoundtrackEditor/Plugins/SoundtrackEditor.dll
                return Path.GetDirectoryName(path); // E:/Games/Kerbal Space Program/GameData/SoundtrackEditor/Plugins
            }
        }

        private static string _musicPath;
        public static string MusicPath
        {
            get
            {
                if (String.IsNullOrEmpty(_musicPath))
                {
                    // GameData:
                    // C:\Games\Kerbal Space Program\GameData\SoundtrackEditor\Music
                    //_musicPath = Path.GetFullPath(new Uri(Path.Combine(GameDatabase.Instance.PluginDataFolder, "Music") + Path.DirectorySeparatorChar).LocalPath);

                    // Root:
                    // KSPUtil.ApplicationRootPath = "C:\\Games\\Kerbal Space Program\\KSP_Data\\..\\"
                    // "C:/Games/Kerbal Space Program/Music"
                    _musicPath = Directory.GetParent(KSPUtil.ApplicationRootPath).FullName.ToString().Replace("\\", "/") + "/Music";
                    Utils.Log("Music path is: " + _musicPath);
                }
                return _musicPath;
            }
        }

        /// <summary>
        /// "Removes wrapping quotation marks."
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveQuotes(string s)
        {
            if (s.Length < 2) return s;
            if (s[0] == '"' && s[s.Length - 1] == '"')
                return s.Substring(1, s.Length - 2);
            return s;
        }
    }
}
