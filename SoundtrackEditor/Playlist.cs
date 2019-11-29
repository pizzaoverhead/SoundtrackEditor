using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Diagnostics;

namespace SoundtrackEditor
{
    public class OnceOff
    {
        // Event driven.

        // Mixing:
        // Skip / temporarily quieten / silence the current audio and play this track.
    }

    [DebuggerDisplay("Name = {name}")] // Show the playlist name instead of "SoundtrackEditor.Playlist" in Watch windows when debugging.
    public class Playlist
    {
        public string name = "New Playlist 1";
        public bool enabled = true;
        public Prerequisites playWhen;
        public bool loop = true;
        public bool shuffle = false;
        public bool pauseOnGamePause = true;
        public bool disableAfterPlay = false; // Play once
        public string playNext = String.Empty; // Playlist to play after this one
        public string playBefore = String.Empty; // Playlist that this playlist should be played before.
        public string playAfter = String.Empty; // Playlist that this playlist should be played after.

        public List<string> tracks = new List<string>(); // TODO: Full path? Duplicates!
        public int channel = 0; // TODO
        public Fade fade = new Fade();
        public Fade trackFade = new Fade();
        public float preloadTime = 5;
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // TODO: Make sure any new fields are added to Equals, the CTOR and the persistor.
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // public bool noInteruptions = false; // Play this playlist until it ends, no switching.
        // public bool noMerging = false; // Disallow other playlists from being mixed with this one
        // public bool warpMusic = false;
        // public string preloadThisPlaylistOnCompletion;
        // Resume from last playback position?
        // Has docking target?
        // KSC building levels?

        public Playlist()
        {
            name = GetNewPlaylistName();
            playWhen = new Prerequisites();
        }

        public Playlist(Playlist p)
        {
            name = p.name;
            enabled = p.enabled;
            playWhen = new Prerequisites(p.playWhen);
            loop = p.loop;
            shuffle = p.shuffle;
            pauseOnGamePause = p.pauseOnGamePause;
            disableAfterPlay = p.disableAfterPlay;
            playNext = p.playNext;
            playBefore = p.playBefore;
            playAfter = p.playAfter;
            tracks = new List<string>(p.tracks);
            channel = p.channel;
            fade = new Fade(p.fade);
            trackFade = new Fade(p.trackFade);
            preloadTime = p.preloadTime;
        }

        public static string GetNewPlaylistName()
        {
            int num = 1;
            string playlistName = "New Playlist " + num;
            SoundtrackEditor sted = SoundtrackEditor.Instance;
            if (sted == null || sted.Playlists == null)
                return playlistName;
            for (int i = 0; i < sted.Playlists.Count; i++)
            {
                if (sted.Playlists[i].name == playlistName)
                {
                    num++;
                    playlistName = "New Playlist " + num;
                    i = 0;
                }
            }
            return playlistName;
        }

        internal int trackIndex = -1;

        public void Shuffle()
        {
            // Fisher-Yates Shuffle.

            if (tracks.Count < 2) return;

            var sb = new StringBuilder();
            foreach (var t in tracks)
                sb.AppendLine(t);
            Utils.Log("Tracks before: " + sb.ToString());

            List<string> unshuffledTracks = new List<string>(tracks);
            List<string> shuffledTracks = new List<string>();
            for (int i = tracks.Count; i > 0; i--)
            {
                int selected = UnityEngine.Random.Range(0, i);
                //Utils.Log("Selected (" + selected + " from " + (i) + ") " + unshuffledTracks[selected]);
                shuffledTracks.Add(unshuffledTracks[selected]);
                unshuffledTracks.RemoveAt(selected);
            }
            // Add the remaining track.
            //shuffledTracks.Add(unshuffledTracks[0]);
            Utils.Log("Remaining tracks: " + unshuffledTracks.Count);

            tracks = shuffledTracks;
        }

        public class Fade
        {
            public float fadeIn = 0;
            public float fadeOut = 0;
            public bool crossfade = false;

            public Fade() {}
            public Fade(Fade f)
            {
                fadeIn = f.fadeIn;
                fadeOut = f.fadeOut;
                crossfade = f.crossfade;
            }

            #region Equality Operator
            public override bool Equals(System.Object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                Fade f = obj as Fade;
                if ((System.Object)f == null)
                {
                    return false;
                }

                return (this.fadeIn == f.fadeIn) &&
                        (this.fadeOut == f.fadeOut) &&
                        (this.crossfade == f.crossfade);
            }

            public bool Equals(Fade p)
            {
                if ((object)p == null)
                {
                    return false;
                }

                return (this.fadeIn == p.fadeIn) &&
                        (this.fadeOut == p.fadeOut) &&
                        (this.crossfade == p.crossfade);
            }

            public override int GetHashCode()
            {
                return new
                {
                    A = fadeIn,
                    B = fadeOut,
                    C = crossfade
                }.GetHashCode();
            }
            #endregion Equality Operator
        }

        public class Prerequisites
        {
            public Enums.Scenes scene = Enums.Scenes.Any;

            // TODO: Make this an OR'd list.
            public string bodyName = string.Empty;

            public Enums.Selector paused = Enums.Selector.Either;

            public Vessel.Situations situation = Enums.AnyVesselSituation;
            public Enums.CameraModes cameraMode = Enums.CameraModes.Any;
            public Enums.Selector inAtmosphere = Enums.Selector.Either;
            public Enums.TimesOfDay timeOfDay = Enums.TimesOfDay.Any;

            public float maxVelocitySurface = float.MaxValue;
            public float minVelocitySurface = float.MinValue;
            public float maxVelocityOrbital = float.MaxValue;
            public float minVelocityOrbital = float.MinValue;
            public float maxAltitude = float.MaxValue;
            public float minAltitude = float.MinValue;
            public float maxVesselDistance = float.MaxValue;
            public float minVesselDistance = 0f;
            public Enums.VesselState vesselState = Enums.VesselState.Any;

            public Prerequisites() {}
            public Prerequisites(Prerequisites p)
            {
                bodyName = p.bodyName;
                paused = p.paused;
                scene = p.scene;
                situation = p.situation;
                cameraMode = p.cameraMode;
                inAtmosphere = p.inAtmosphere;
                timeOfDay = p.timeOfDay;
                maxVelocitySurface = p.maxVelocitySurface;
                minVelocitySurface = p.minVelocitySurface;
                maxVelocityOrbital = p.maxVelocityOrbital;
                minVelocityOrbital = p.minVelocityOrbital;
                maxAltitude = p.maxAltitude;
                minAltitude = p.minAltitude;
                vesselState = p.vesselState;
            }

            // TODO: Make sure any new fields are added to Equals, the CTOR and the persistor.


            //public Playlist playAfter = null;
            //public Playlist playNext = null;

            // TODO
            //
            // Is in sunlight?
            // Is reentering? Detect when reentry is happening.
            // Play while scene is loading?
            // Ground contact?
            // Engines currently burning?
            // Craft dead?
            // hasTarget?
            // Biomes
            // Vessel icon
            // Craft name
            // Apoapsis max/min.
            // Periapsis max/min.
            // Altitude max/min.
            // Radar altitude (surface travel) max/min.
            // max/minDistanceTarget, max/minVelocityTarget
            //
            // Option to delay before switching? (hang time over jump between ground and flight for example)
            // Don't change from this until track complete/playlist complete
            //   Option to wait until end of current track before switching
            //   Option to wait until end of playlist before switching (sticky playlist)
            // List<string> These playlists must already played
            // Fade out, fade in, crossfade
            //
            // Unload when the situation is no longer correct.
            //

            public static string SituationToText(Vessel.Situations situation)
            {
                if (situation == Enums.AnyVesselSituation)
                    return "Any";

                string result = string.Empty;
                if ((situation & Vessel.Situations.DOCKED) == Vessel.Situations.DOCKED)
                    result += "DOCKED ";
                if ((situation & Vessel.Situations.ESCAPING) == Vessel.Situations.ESCAPING)
                    result += "ESCAPING ";
                if ((situation & Vessel.Situations.FLYING) == Vessel.Situations.FLYING)
                    result += "FLYING ";
                if ((situation & Vessel.Situations.LANDED) == Vessel.Situations.LANDED)
                    result += "LANDED ";
                if ((situation & Vessel.Situations.ORBITING) == Vessel.Situations.ORBITING)
                    result += "ORBITING ";
                if ((situation & Vessel.Situations.PRELAUNCH) == Vessel.Situations.PRELAUNCH)
                    result += "PRELAUNCH ";
                if ((situation & Vessel.Situations.SPLASHED) == Vessel.Situations.SPLASHED)
                    result += "SPLASHED ";
                if ((situation & Vessel.Situations.SUB_ORBITAL) == Vessel.Situations.SUB_ORBITAL)
                    result += "SUB_ORBITAL ";
                return result;
            }

            public string PrintPrerequisites()
            {
                return
                    "Paused:\t\t\t" + paused + "\r\n" +
                    "Scene:\t\t\t" + (SoundtrackEditor.CurrentSituation == null ? "None" : Enum.GetName(typeof(Enums.Scenes), SoundtrackEditor.CurrentSituation.scene)) + "\r\n" +
                    "Camera mode:\t\t" + Enum.GetName(typeof(Enums.CameraModes), cameraMode) + "\r\n" +
                    "Body name:\t\t" + (bodyName.Length > 0 ? bodyName : "Any") + "\r\n" +
                    "Situation:\t" + SituationToText(situation) + "\r\n" +
                    "Max Orbital Velocity:\t" + (maxVelocityOrbital == float.MaxValue ? "None" : maxVelocityOrbital.ToString()) + "\r\n" +
                    "Min Orbital Velocity:\t" + (minVelocityOrbital == float.MinValue ? "None" : minVelocityOrbital.ToString()) + "\r\n" +
                    "Max Surface Velocity:\t" + (maxVelocitySurface == float.MaxValue ? "None" : maxVelocitySurface.ToString()) + "\r\n" +
                    "Min Surface Velocity:\t" + (minVelocitySurface == float.MinValue ? "None" : minVelocitySurface.ToString()) + "\r\n" +
                    "Max Altitude:\t\t" + (maxAltitude == float.MaxValue ? "None" : maxAltitude.ToString()) + "\r\n" +
                    "Min Altitude:\t\t" + (minAltitude == float.MinValue ? "None" : minAltitude.ToString()) + "\r\n" +
                    "Vessel state:\t\t\t" + vesselState + "\r\n" +
                    "In Atmosphere:\t\t" + inAtmosphere + "\r\n" + 
                    "Time Of Day:\t\t" + timeOfDay;
            }

            private static StringBuilder situationSB = new StringBuilder();
            public static string PrintSituation()
            {
                situationSB.Length = 0; // StringBuilder.Clear() not available in .NET 3.5.
                situationSB.Append("Scene:\t\t\t").AppendLine(Enum.GetName(typeof(Enums.Scenes), SoundtrackEditor.CurrentSituation.scene))
                    .Append("Camera mode:\t\t").AppendLine(CameraManager.Instance == null ? "No camera" : CameraManager.Instance.currentCameraMode.ToString())
                    .Append("Paused:\t\t\t").AppendLine(SoundtrackEditor.CurrentSituation.paused.ToString());

                //if (SoundtrackEditor.CurrentSituation.scene == Enums.Scenes.SpaceCentre)
                situationSB.Append("Time Of Day:\t\t").AppendLine(SoundtrackEditor.CurrentSituation.timeOfDay.ToString());

                Vessel v = SoundtrackEditor.InitialLoadingComplete ? FlightGlobals.ActiveVessel : null;
                if (v != null)
                {
                    situationSB.Append("Body name:\t\t").AppendLine(v.mainBody.name)
                    .Append("Situation:\t\t\t").AppendLine(v.situation.ToString())
                    .Append("Orbital velocity:\t\t").AppendLine(v.obt_velocity.magnitude.ToString())
                    .Append("Surface velocity:\t\t").AppendLine(v.srf_velocity.magnitude.ToString())
                    .Append("Altitude:\t\t\t").AppendLine(v.altitude.ToString())
                    .Append("In Atmosphere:\t\t").AppendLine((v.atmDensity > 0).ToString())
                    .Append("State:\t\t").AppendLine(v.state.ToString());
                }
                else
                    situationSB.AppendLine("Vessel:\t\t\tNo vessel");

                return situationSB.ToString();
            }

            public bool CheckPaused()
            {
                Enums.Selector curPaused = SoundtrackEditor.CurrentSituation.paused;
                return (curPaused & paused) == curPaused;
            }

            public bool CheckScene()
            {
                Enums.Scenes curScene = SoundtrackEditor.CurrentSituation.scene;
                return (curScene & scene) == curScene;
            }

            public bool CheckCameraMode()
            {
                if (CameraManager.Instance != null)
                {
                    var curMode = Enums.ConvertCameraMode(CameraManager.Instance.currentCameraMode);
                    if ((curMode & cameraMode) != curMode)
                    {
                        //Utils.Log("Prereq failed: Expected camMode " + cameraMode + ", but was " + curMode);
                        return false;
                    }
                }
                return true;
            }

            public bool CheckBodyName(Vessel v)
            {
                // Body name
                if (bodyName.Length > 0 && !bodyName.Equals(v.mainBody.name))
                {
                    Utils.Log("Prereq failed: Expected bodyName " + bodyName + ", but was " + v.mainBody.name);
                    return false;
                }
                return true;
            }

            public bool CheckSituation(Vessel v)
            {
                if ((v.situation & situation) != v.situation)
                {
                    Utils.Log("Prereq failed: Expected situation " + situation + ", but was " + v.situation);
                    return false;
                }
                return true;
            }

            public bool CheckOrbitalVelocity(Vessel v)
            {
                return (maxVelocityOrbital >= v.obt_velocity.magnitude) && (minVelocityOrbital <= v.obt_velocity.magnitude);
            }

            public bool CheckSurfaceVelocity(Vessel v)
            {
                return (maxVelocitySurface >= v.srf_velocity.magnitude) && (minVelocitySurface <= v.srf_velocity.magnitude);
            }

            public bool CheckAltitude(Vessel v)
            {
                return (maxAltitude >= v.altitude) && (minAltitude <= v.altitude);
            }

            public bool CheckVesselDistance(Vessel v)
            {
                return Utils.GetNearestVessel(minVesselDistance, maxVesselDistance, v) != null;
            }

            public bool CheckInAtmosphere(Vessel v)
            {
                if (inAtmosphere != Enums.Selector.Either)
                {
                    bool inAtm = v.atmDensity > 0;
                    if (inAtmosphere == Enums.Selector.True && !inAtm ||
                        inAtmosphere == Enums.Selector.False && inAtm)
                        return false;
                }
                return true;
            }

            public bool CheckVesselState(Vessel v)
            {
                Enums.VesselState currentState = Enums.ConvertVesselState(v.state);
                if ((currentState & vesselState) != currentState)
                {
                    //Utils.Log("Prereq failed: Expected situation " + situation + ", but was " + v.situation);
                    return false;
                }
                return true;
            }

            private CelestialBody _homeBody;
            public bool CheckTimeOfDay()
            {
                if (_homeBody == null)
                    _homeBody = FlightGlobals.GetHomeBody();
                double localTime = Sun.Instance.GetLocalTimeAtPosition(Utils.KscLatitude, Utils.KscLongitude, _homeBody);
                Enums.TimesOfDay tod = Enums.TimeToTimeOfDay(localTime);

                if ((tod & timeOfDay) != tod)
                    return false;

                return true;
            }

            public bool PrerequisitesMet()
            {
                try
                {
                    if (!CheckPaused()) return false;
                    if (!CheckScene()) return false;
                    if (!CheckCameraMode()) return false;
                    if (!CheckTimeOfDay()) return false;

                    // TODO - Throws exceptions before the initial loading screen is completed.
                    Vessel v = SoundtrackEditor.InitialLoadingComplete ? FlightGlobals.ActiveVessel : null;

                    if (v != null)
                    {
                        if (!CheckBodyName(v)) return false;
                        if (!CheckSituation(v)) return false;

                        if (!CheckOrbitalVelocity(v)) return false;
                        if (!CheckSurfaceVelocity(v)) return false;
                        if (!CheckAltitude(v)) return false;
                        if (!CheckVesselDistance(v)) return false;
                        if (!CheckInAtmosphere(v)) return false;
                        if (!CheckVesselState(v)) return false;

                        //if (p.playAfter
                        //if (p.playNext
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("[STED] PrerequisitesMet Error: " + ex.Message);
                }
                return false;
            }

            #region Equality operator
            public override bool Equals(System.Object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                Prerequisites p = obj as Prerequisites;
                if ((System.Object)p == null)
                {
                    return false;
                }

                return (this.paused == p.paused) &&
                        (this.inAtmosphere == p.inAtmosphere) &&
                        (this.timeOfDay == p.timeOfDay) &&
                        (this.maxVelocitySurface == p.maxVelocitySurface) &&
                        (this.minVelocitySurface == p.minVelocitySurface) &&
                        (this.maxVelocityOrbital == p.maxVelocityOrbital) &&
                        (this.minVelocityOrbital == p.minVelocityOrbital) &&
                        (this.maxAltitude == p.maxAltitude) &&
                        (this.minAltitude == p.minAltitude) &&
                        (this.maxVesselDistance == p.maxVesselDistance) &&
                        (this.minVesselDistance == p.minVesselDistance) &&
                        (this.vesselState == p.vesselState) &&
                        (this.scene == p.scene) &&
                        (this.situation == p.situation) &&
                        (this.cameraMode == p.cameraMode) &&
                        (this.bodyName.Equals(p.bodyName));
                //Playlist playAfter = null;
                //Playlist playNext = null;
            }

            public bool Equals(Prerequisites p)
            {
                if ((object)p == null)
                {
                    return false;
                }

                return (this.paused == p.paused) &&
                        (this.inAtmosphere == p.inAtmosphere) &&
                        (this.timeOfDay == p.timeOfDay) &&
                        (this.maxVelocitySurface == p.maxVelocitySurface) &&
                        (this.minVelocitySurface == p.minVelocitySurface) &&
                        (this.maxVelocityOrbital == p.maxVelocityOrbital) &&
                        (this.minVelocityOrbital == p.minVelocityOrbital) &&
                        (this.maxAltitude == p.maxAltitude) &&
                        (this.minAltitude == p.minAltitude) &&
                        (this.maxVesselDistance == p.maxVesselDistance) &&
                        (this.minVesselDistance == p.minVesselDistance) &&
                        (this.vesselState == p.vesselState) &&
                        (this.scene == p.scene) &&
                        (this.situation == p.situation) &&
                        (this.cameraMode == p.cameraMode) &&
                        (this.bodyName.Equals(p.bodyName));
                //Playlist playAfter = null;
                //Playlist playNext = null;
            }

            public override int GetHashCode()
            {
                return new
                {
                    A = paused,
                    B = inAtmosphere,
                    C = timeOfDay,
                    D = maxVelocitySurface,
                    E = minVelocitySurface,
                    F = maxVelocityOrbital,
                    G = minVelocityOrbital,
                    H = maxAltitude,
                    I = minAltitude,
                    J = maxVesselDistance,
                    K = minVesselDistance,
                    L = vesselState,
                    M = scene,
                    N = situation,
                    O = cameraMode,
                    P = bodyName
                }.GetHashCode();
            }
            #endregion Equality operator
        } // End of class Prerequisites.

        #region Equality Operator
        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Playlist p = obj as Playlist;
            if ((System.Object)p == null)
            {
                return false;
            }

            return (this.enabled == p.enabled) &&
                    (this.loop == p.loop) &&
                    (this.shuffle == p.shuffle) &&
                    (this.pauseOnGamePause == p.pauseOnGamePause) &&
                    (this.disableAfterPlay == p.disableAfterPlay) &&
                    (this.channel == p.channel) &&
                    (this.playNext == p.playNext) &&
                    (this.playBefore == p.playBefore) &&
                    (this.playAfter == p.playAfter) &&
                    (this.name.Equals(p.name)) &&
                    (this.tracks.SequenceEqual(p.tracks)) &&
                    (this.playWhen.Equals(p.playWhen)) &&
                    (this.fade == p.fade) &&
                    (this.trackFade == p.trackFade) &&
                    (this.preloadTime == p.preloadTime);
        }

        public bool Equals(Playlist p)
        {
            if ((object)p == null)
            {
                return false;
            }

            return (this.enabled == p.enabled) &&
                    (this.loop == p.loop) &&
                    (this.shuffle == p.shuffle) &&
                    (this.pauseOnGamePause == p.pauseOnGamePause) &&
                    (this.disableAfterPlay == p.disableAfterPlay) &&
                    (this.channel == p.channel) &&
                    (this.playNext == p.playNext) &&
                    (this.playBefore == p.playBefore) &&
                    (this.playAfter == p.playAfter) &&
                    (this.tracks.SequenceEqual(p.tracks)) &&
                    (this.playWhen.Equals(p.playWhen)) &&
                    (this.fade == p.fade) &&
                    (this.trackFade == p.trackFade) &&
                    (this.preloadTime == p.preloadTime);
        }

        public override int GetHashCode()
        {
            return new
            {
                A = enabled,
                B = loop,
                C = shuffle,
                D = pauseOnGamePause,
                E = disableAfterPlay,
                F = channel,
                G = playNext,
                H = playBefore,
                I = playAfter,
                J = name,
                K = tracks,
                L = playWhen,
                M = fade,
                N = trackFade,
                O = preloadTime
            }.GetHashCode();
        }
        #endregion Equality Operator
    } // End of class Playlist.
}
