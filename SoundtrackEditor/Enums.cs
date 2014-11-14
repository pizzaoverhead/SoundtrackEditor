using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundtrackEditor
{
    public static class Enums
    {
        [Flags]
        public enum Scene
        {
            None = 0,
            Loading = 0x1,
            LoadingBuffer = 0x2,
            MainMenu = 0x4,
            Settings = 0x8,
            Credits = 0x10,
            SpaceCentre = 0x20,
            VAB = 0x40,
            SPH = 0x80,
            TrackingStation = 0x100,
            Flight = 0x200,
            PSystem = 0x400,
            Any = 0xFFF
        }

        public enum Selector
        {
            Either = -1,
            No = 0,
            False = 0,
            Yes = 1,
            True = 1
        }

        [Flags]
        public enum CameraMode
        {
            Flight = 0x1,
            Map = 0x2,
            External = 0x4,
            IVA = 0x8,
            Internal = 10,
            Any = 0xF
        }

        public static CameraMode ConvertCameraMode(CameraManager.CameraMode mode)
        {
            switch (mode)
            {
                case CameraManager.CameraMode.External:
                    {
                        return CameraMode.External;
                    }
                case CameraManager.CameraMode.Flight:
                    {
                        return CameraMode.Flight;
                    }
                case CameraManager.CameraMode.Internal:
                    {
                        return CameraMode.Internal;
                    }
                case CameraManager.CameraMode.IVA:
                    {
                        return CameraMode.IVA;
                    }
                case CameraManager.CameraMode.Map:
                    {
                        return CameraMode.Map;
                    }
                default:
                    {
                        throw new ArgumentException("Soundtrack Editor: Invalid camera mode: " + mode +
                            "\nCheck for an updated version of Soundtrack Editor.");
                    }
            }
        }

        public static Enums.Scene ConvertScene(GameScenes scene)
        {
            switch (scene)
            {
                case GameScenes.LOADING:
                    {
                        return Enums.Scene.Loading;
                    }
                case GameScenes.LOADINGBUFFER:
                    {
                        return Enums.Scene.LoadingBuffer;
                    }
                case GameScenes.MAINMENU:
                    {
                        return Enums.Scene.MainMenu;
                    }
                case GameScenes.SETTINGS:
                    {
                        return Enums.Scene.Settings;
                    }
                case GameScenes.CREDITS:
                    {
                        return Enums.Scene.Credits;
                    }
                case GameScenes.SPACECENTER:
                    {
                        return Enums.Scene.SpaceCentre;
                    }
                case GameScenes.EDITOR:
                    {
                        return Enums.Scene.VAB;
                    }
                case GameScenes.FLIGHT:
                    {
                        return Enums.Scene.Flight;
                    }
                case GameScenes.TRACKSTATION:
                    {
                        return Enums.Scene.TrackingStation;
                    }
                case GameScenes.SPH:
                    {
                        return Enums.Scene.SPH;
                    }
                case GameScenes.PSYSTEM:
                    {
                        return Enums.Scene.PSystem;
                    }
                default:
                    {
                        throw new ArgumentException("Soundtrack Editor: Invalid scene: " + scene +
                            "\nCheck for an updated version of Soundtrack Editor.");
                    }
            }
        }

        /// <summary>
        /// Converts a string to an enum. Allows for use of | and &.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Parse<T>(string value) where T : struct, IConvertible
        {
            Type genericType = typeof(T);
            if (!genericType.IsEnum)
                throw new ArgumentException("Type 'T' must be an enum");
            int result = 0;
            int lastIndex = 0;
            char op = ' ';
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '|' || value[i] == '&' || i + 1 == value.Length)
                {
                    string s = value.Substring(lastIndex,
                        (i + 1 == value.Length ? i + 1 : i) - lastIndex).Trim();
                    Enum parsedEnum = Enum.Parse(typeof(T), s, true) as Enum;
                    if (op == ' ')
                        result = Convert.ToInt32(parsedEnum);
                    else if (op == '|')
                        result |= Convert.ToInt32(parsedEnum);
                    else if (op == '&')
                        result &= Convert.ToInt32(parsedEnum);
                    op = value[i];
                    lastIndex = i + 1;
                }
            }
            return (T)Enum.ToObject(typeof(T), result);
        }
    }
}
