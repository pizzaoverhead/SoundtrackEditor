using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundtrackEditor
{
    public static class Enums
    {
        [Flags]
        public enum Scenes
        {
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
            AstronautComplex = 0x800,
            KSPedia = 0x1000,
            MissionControl = 0x2000,
            RnDComplex = 0x4000,
            AdminFacility = 0x8000,
            Any = Loading | LoadingBuffer | MainMenu | Settings | Credits | SpaceCentre | VAB | SPH | TrackingStation |
                Flight | PSystem | AstronautComplex | KSPedia | MissionControl | RnDComplex | AdminFacility
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
        public enum CameraModes
        {
            Flight = 0x1,
            Map = 0x2,
            External = 0x4,
            IVA = 0x8,
            Internal = 0x10,
            Any = Flight | Map | External | IVA | Internal
        }

        public static CameraModes ConvertCameraMode(CameraManager.CameraMode mode)
        {
            switch (mode)
            {
                case CameraManager.CameraMode.External:
                    {
                        return CameraModes.External;
                    }
                case CameraManager.CameraMode.Flight:
                    {
                        return CameraModes.Flight;
                    }
                case CameraManager.CameraMode.Internal:
                    {
                        return CameraModes.Internal;
                    }
                case CameraManager.CameraMode.IVA:
                    {
                        return CameraModes.IVA;
                    }
                case CameraManager.CameraMode.Map:
                    {
                        return CameraModes.Map;
                    }
                default:
                    {
                        throw new ArgumentException("Soundtrack Editor: Invalid camera mode: " + mode +
                            "\nCheck for an updated version of Soundtrack Editor.");
                    }
            }
        }

        public static Vessel.Situations AnyVesselSituation = Vessel.Situations.DOCKED | Vessel.Situations.ESCAPING | Vessel.Situations.FLYING |
            Vessel.Situations.LANDED | Vessel.Situations.ORBITING | Vessel.Situations.PRELAUNCH | Vessel.Situations.SPLASHED | Vessel.Situations.SUB_ORBITAL;

        public enum Channel
        {
            Ship = 0,
	        Voice = 1,
	        Ambient = 2,
            Music = 3,
            UI = 4,
            Channel5 = 5,
            Channel6 = 6,
            Channel7 = 7
        }

        [Flags]
        public enum TimesOfDay
        {
            NightAM = 0x1,
            TwilightAM = 0x2,
            DayAM = 0x4,
            DayPM = 0x8,
            TwilightPM = 0x10,
            NightPM = 0x20,
            Any = NightAM | TwilightAM | DayAM | DayPM | TwilightPM | NightPM
        }

        public static Enums.TimesOfDay TimeToTimeOfDay(double time)
        {
            if (time < 0.2228) // Stars disappear
                return Enums.TimesOfDay.NightAM;
            if (time < 0.2439) // Sunflare appears
                return Enums.TimesOfDay.TwilightAM;
            if (time < 0.5) // Mid-day
                return Enums.TimesOfDay.DayAM;
            if (time < 0.756) // Sunflare disappears
                return Enums.TimesOfDay.DayPM;
            if (time < 0.7776) // Stars appear
                return Enums.TimesOfDay.TwilightPM;
            return Enums.TimesOfDay.NightPM;
        }

        [Flags]
        public enum VesselState
        {
            Inactive = 0x1,
            Active = 0x2,
            Dead = 0x4,
            Any = Inactive | Active | Dead
        }

        public static Enums.VesselState ConvertVesselState(Vessel.State state)
        {
            switch (state)
            {
                case Vessel.State.INACTIVE:
                    {
                        return Enums.VesselState.Inactive;
                    }
                case Vessel.State.ACTIVE:
                    {
                        return Enums.VesselState.Active;
                    }
                case Vessel.State.DEAD:
                    {
                        return Enums.VesselState.Dead;
                    }
                default:
                    {
                        throw new ArgumentException("Soundtrack Editor: Invalid vessel state: " + state +
                            "\nCheck for an updated version of Soundtrack Editor.");
                    }
            }
        }

        public static Enums.Scenes ConvertScene(GameScenes scene)
        {
            switch (scene)
            {
                case GameScenes.LOADING:
                    {
                        return Enums.Scenes.Loading;
                    }
                case GameScenes.LOADINGBUFFER:
                    {
                        return Enums.Scenes.LoadingBuffer;
                    }
                case GameScenes.MAINMENU:
                    {
                        return Enums.Scenes.MainMenu;
                    }
                case GameScenes.SETTINGS:
                    {
                        return Enums.Scenes.Settings;
                    }
                case GameScenes.CREDITS:
                    {
                        return Enums.Scenes.Credits;
                    }
                case GameScenes.SPACECENTER:
                    {
                        return Enums.Scenes.SpaceCentre;
                    }
                case GameScenes.EDITOR:
                    {
                        return Enums.Scenes.VAB;
                    }
                case GameScenes.FLIGHT:
                    {
                        return Enums.Scenes.Flight;
                    }
                case GameScenes.TRACKSTATION:
                    {
                        return Enums.Scenes.TrackingStation;
                    }
                /*case GameScenes.SPH:
                    {
                        return Enums.Scene.SPH;
                    }*/
                case GameScenes.PSYSTEM:
                    {
                        return Enums.Scenes.PSystem;
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
            // TODO: Disallow & in some cases.

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
