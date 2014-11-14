using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SoundtrackEditor
{
    public class OnceOff
    {
        // Event driven.

        // Mixing:
        // Skip / temporarily quieten / silence the current audio and play this track.
    }


    public class Playlist
    {
        public string name = "NewPlaylist";
        public bool enabled = true;
        public Prerequisites playWhen;
        public bool loop = true;
        public bool shuffle = false;
        public bool disableAfterPlay = false; // Play once
        public string playNext = String.Empty;
        public List<string> tracks = new List<string>();
        public int channel = 0; // TODO
        public Fade fade = new Fade();
        public Fade trackFade = new Fade();
        public float preloadTime = 5;
        // public bool warpMusic = false;
        // public string preloadThisPlaylistOnCompletion;
        // TODO: Make sure any new fields are added to Equals.

        // TODO: Allow overrides: "Disallow other playlists from being mixed with this one"

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
            // Need to be able to OR these in the cfg.
            public Enums.Scene scene = Enums.Scene.Any;

            // TODO: Make this an OR'd list.
            public string bodyName = String.Empty;
            public CelestialBody body
            {
                get; // TODO: Get this using the bodyName string.
                set;
            }

            public Vessel.Situations situation = (Vessel.Situations)0xFFF;
            public Enums.CameraMode cameraMode = Enums.CameraMode.Any;

            public Enums.Selector inAtmosphere = Enums.Selector.Either;

            public float surfaceVelocity = -1;
            public float surfaceVelocityBelow = -1;
            public float surfaceVelocityAbove = -1;

            public float velocityOrbital = -1;
            public float maxVelocityOrbital = -1;
            public float minVelocityOrbital = -1;
            // TODO: Make sure any new fields are added to Equals and the persistor.

            // TODO: Altitude max/min


            //public Playlist playAfter = null;
            //public Playlist playNext = null;

            // TODO
            //
            // Is in sunlight?
            // Is reentering?
            // Play while scene is loading?
            // IVA, internal view, map view
            // Altitude
            // Vessel icon
            // Craft dead
            // Craft name
            // List<string> These playlists must already played
            // Don't change from this until track complete/playlist complete
            // Fade out, fade in, crossfade
            //
            // Unload when the situation is no longer correct.
            //


            public bool PrerequisitesMet()
            {
                // Scene
                //Scene curScene = ConvertScene(GameInfo.gameScene);
                Enums.Scene curScene = SoundtrackEditor.CurrentSituation.scene;
                if ((curScene & scene) != curScene)
                {
                    /*Utils.Log("Prereq failed: Expected scene " + scene + ", but was " + curScene +
                        ", " + (int)scene + " & " + (int)curScene + " = " + (int)(scene & curScene));*/
                    return false;
                }

                // Camera mode
                if (CameraManager.Instance != null)
                {
                    var curMode = Enums.ConvertCameraMode(CameraManager.Instance.currentCameraMode);
                    if ((curMode & cameraMode) != curMode)
                    {
                        Utils.Log("Prereq failed: Expected camMode " + cameraMode + ", but was " + curMode);
                        return false;
                    }
                }

                // TODO - Throws exceptions before the initial loading screen is completed.
                Vessel v = SoundtrackEditor.InitialLoadingComplete ? FlightGlobals.ActiveVessel : null;

                if (v != null)
                {
                    // Body name
                    if (bodyName.Length > 0 && !bodyName.Equals(v.mainBody.name))
                    {
                        Utils.Log("Prereq failed: Expected bodyName " + bodyName + ", but was " + v.mainBody.name);
                        return false;
                    }

                    // Situation
                    if ((v.situation & situation) != v.situation)
                    {
                        Utils.Log("Prereq failed: Expected situation " + situation + ", but was " + v.situation);
                        return false;
                    }

                    double orbitalVelocity = v.obt_velocity.magnitude;
                    if (maxVelocityOrbital != -1 && maxVelocityOrbital < orbitalVelocity) return false;
                    if (minVelocityOrbital != -1 && minVelocityOrbital > orbitalVelocity) return false;
                    double surfaceVelocity = v.srf_velocity.magnitude;
                    if (surfaceVelocityBelow != -1 && surfaceVelocityBelow < orbitalVelocity) return false;
                    if (surfaceVelocityAbove != -1 && surfaceVelocityAbove > orbitalVelocity) return false;
                    //if (p.playAfter
                    //if (p.playNext

                    if (inAtmosphere != Enums.Selector.Either)
                    {
                        bool inAtm = v.atmDensity > 0;
                        if (inAtmosphere == Enums.Selector.Yes && !inAtm ||
                            inAtmosphere == Enums.Selector.No && inAtm)
                        {
                            Utils.Log("Prereq failed: " + (inAtm ? "In " : "Not in ") + "atmosphere");
                            return false;
                        }
                    }
                }

                return true;
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

                return (this.inAtmosphere == p.inAtmosphere) &&
                        (this.surfaceVelocity == p.surfaceVelocity) &&
                        (this.surfaceVelocityBelow == p.surfaceVelocityBelow) &&
                        (this.surfaceVelocityAbove == p.surfaceVelocityAbove) &&
                        (this.velocityOrbital == p.velocityOrbital) &&
                        (this.maxVelocityOrbital == p.maxVelocityOrbital) &&
                        (this.minVelocityOrbital == p.minVelocityOrbital) &&
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

                return (this.inAtmosphere == p.inAtmosphere) &&
                        (this.surfaceVelocity == p.surfaceVelocity) &&
                        (this.surfaceVelocityBelow == p.surfaceVelocityBelow) &&
                        (this.surfaceVelocityAbove == p.surfaceVelocityAbove) &&
                        (this.velocityOrbital == p.velocityOrbital) &&
                        (this.maxVelocityOrbital == p.maxVelocityOrbital) &&
                        (this.minVelocityOrbital == p.minVelocityOrbital) &&
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
                    A = inAtmosphere,
                    B = surfaceVelocity,
                    C = surfaceVelocityBelow,
                    D = surfaceVelocityAbove,
                    E = velocityOrbital,
                    F = maxVelocityOrbital,
                    G = minVelocityOrbital,
                    H = scene,
                    I = situation,
                    J = cameraMode,
                    K = bodyName
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
                    (this.disableAfterPlay == p.disableAfterPlay) &&
                    (this.channel == p.channel) &&
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
                    (this.disableAfterPlay == p.disableAfterPlay) &&
                    (this.channel == p.channel) &&
                    (this.name.Equals(p.name)) &&
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
                D = disableAfterPlay,
                E = channel,
                F = name,
                G = tracks,
                H = playWhen,
                I = fade,
                J = trackFade,
                K = preloadTime
            }.GetHashCode();
        }
        #endregion Equality Operator
    } // End of class Playlist.
}
