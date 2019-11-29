using System;
using UnityEngine;
using System.IO;
using System.Diagnostics;

namespace SoundtrackEditor
{
    public static class Utils
    {
        public static double KscLatitude = -0.0917535863160035;
        public static double KscLongitude = 285.37030688110428;

        [ConditionalAttribute("DEBUG")]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log("[STED] " + message);
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

        private static bool _libmpg123Installed = false;
        private static bool _libmpg123Checked = false;
        public static bool LibMpg123Installed
        {
            get
            {
                if (_libmpg123Checked)
                    return _libmpg123Installed;
                //                             C:/KSP/KSP_Data                 /                             libmpg123-0 
                string libPath = Path.Combine(Application.dataPath, @"Mono") + Path.DirectorySeparatorChar + MPGImport.Mpg123Dll + ".dll";
                // e.g. C:/Games/Kerbal Space Program/KSP_Data\Mono\libmpg123-0.dll
                _libmpg123Installed = System.IO.File.Exists(libPath); // Works in spite of the mixed '/' '\' path.
                _libmpg123Checked = true;
                return _libmpg123Installed;
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

        public static string GetDllDirectoryPath()
        {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            //Debug.Log(string.Format("Current assembly path: {0}", Path.GetDirectoryName(path)));

            return Path.GetDirectoryName(path);
        }

        public static Vessel GetNearestVessel()
        {
            if (HighLogic.LoadedScene == GameScenes.LOADING)
                return null;
            return GetNearestVessel(0, float.MaxValue, FlightGlobals.ActiveVessel);
        }

        public static Vessel GetNearestVessel(float min, float max, Vessel v)
        {
            Vessel nearestVessel = null;
            double nearestRange = double.MaxValue;
            if (v && FlightGlobals.Vessels.Count > 1)
            {
                Vector3d vesselPos = v.GetWorldPos3D();
                for (int i = FlightGlobals.Vessels.Count - 1; i >= 0; --i)
                {
                    if (FlightGlobals.Vessels[i] == v)
                        continue;

                    double distance = (FlightGlobals.Vessels[i].GetWorldPos3D() - vesselPos).magnitude;
                    if (distance > min && distance < max && distance < nearestRange)
                    {
                        nearestRange = distance;
                        nearestVessel = FlightGlobals.Vessels[i];
                    }
                }
            }
            else
            {
                return v;
            }
            return nearestVessel;
        }

        /*public static Shader GetShader()
        {
            Shader shader;
            string path = Utils.GetDllDirectoryPath() + "/Shaders/FisheyeShader.shader";
            if (!File.Exists(path))
            {
                Debug.LogError("Error loading shader: File not found at " + path);
                return null;
            }
            StreamReader sr = new StreamReader(path);
            shader = new Material(sr.ReadToEnd()).shader;
            sr.Close();
            return shader;
        }*/
    }
}
