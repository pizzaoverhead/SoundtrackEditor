using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SoundtrackEditor
{
    public class Persistor
    {
        private static ConfigNode _config;
        private static string _configSavePath = "Playlists/playlists.cfg";
        //private static string _configSavePath2 = "GameData/SoundtrackEditor/PluginData/settings2.cfg";
        public static List<Playlist> LoadPlaylists()
        {
            try
            {
                List<Playlist> playlists = new List<Playlist>();
                _config = ConfigNode.Load(_configSavePath);
                if (_config == null)
                {
                    Debug.LogWarning("[STED] No config file present at " + _configSavePath + ". Loading stock tracks.");
                    return LoadStockPlaylist();
                }
                Utils.Log("Loading playlists: " + _configSavePath);

                foreach (ConfigNode node in _config.nodes)
                {
                    if (node.name.Equals("playlist"))
                    {
                        Playlist p = new Playlist();
                        if (!node.HasValue("name")) continue;
                        p.name = node.GetValue("name");

                        Utils.Log("Loading playlist " + p.name);

                        if (node.HasValue("enabled"))
                            p.enabled = bool.Parse(node.GetValue("enabled"));
                        if (node.HasValue("loop"))
                            p.loop = bool.Parse(node.GetValue("loop"));
                        if (node.HasValue("shuffle"))
                            p.shuffle = bool.Parse(node.GetValue("shuffle"));
                        if (node.HasValue("pauseOnGamePause"))
                        {
                            p.pauseOnGamePause = bool.Parse(node.GetValue("pauseOnGamePause"));
                            if (p.pauseOnGamePause)
                                EventManager.Instance.MonitorPause = true;
                        }
                        if (node.HasValue("disableAfterPlay"))
                            p.disableAfterPlay = bool.Parse(node.GetValue("disableAfterPlay"));
                        if (node.HasValue("playNext"))
                            p.playNext = node.GetValue("playNext");
                        if (node.HasValue("playBefore"))
                            p.playBefore = node.GetValue("playBefore");
                        if (node.HasValue("playAfter"))
                            p.playAfter = node.GetValue("playAfter");
                        if (node.HasValue("channel"))
                            p.channel = int.Parse(node.GetValue("channel"));
                        if (node.HasValue("preloadTime"))
                            p.preloadTime = float.Parse(node.GetValue("preloadTime"));

                        /* Not yet implemented.
                        if (node.HasNode("fade"))
                        {
                            p.fade = new Playlist.Fade();
                            ConfigNode fade = node.GetNode("fade");
                            if (fade.HasNode("fadeIn"))
                            {
                                p.fade.fadeIn = float.Parse(node.GetValue("fadeIn"));
                            }
                            if (fade.HasNode("fadeOut"))
                                p.fade.fadeOut = float.Parse(node.GetValue("fadeOut"));
                            if (fade.HasNode("crossfade"))
                                p.fade.crossfade = bool.Parse(node.GetValue("crossfade"));
                        }
                        if (node.HasValue("trackFade"))
                        {
                            p.trackFade = new Playlist.Fade();
                            ConfigNode trackFade = node.GetNode("trackFade");
                            if (trackFade.HasNode("fadeIn"))
                                p.trackFade.fadeIn = float.Parse(node.GetValue("fadeIn"));
                            if (trackFade.HasNode("fadeOut"))
                                p.trackFade.fadeOut = float.Parse(node.GetValue("fadeOut"));
                            if (trackFade.HasNode("crossfade"))
                                p.trackFade.crossfade = bool.Parse(node.GetValue("crossfade"));
                        }*/

                        if (node.HasNode("tracks"))
                        {
                            ConfigNode tracks = node.GetNode("tracks");
                            foreach (string t in tracks.GetValues("track"))
                            {
                                p.tracks.Add(Utils.RemoveQuotes(t));
                            }
                        }
                        if (node.HasNode("playWhen"))
                        {
                            p.playWhen = new Playlist.Prerequisites();
                            ConfigNode playWhen = node.GetNode("playWhen");

                            if (playWhen.HasValue("paused"))
                            {
                                p.playWhen.paused =  Enums.Parse<Enums.Selector>(playWhen.GetValue("paused"));
                            }
                            if (playWhen.HasValue("inAtmosphere"))
                            {
                                p.playWhen.inAtmosphere = Enums.Parse<Enums.Selector>(playWhen.GetValue("inAtmosphere"));
                            }
                            if (playWhen.HasValue("timeOfDay"))
                            {
                                p.playWhen.timeOfDay = Enums.Parse<Enums.TimesOfDay>(playWhen.GetValue("timeOfDay"));
                            }
                            if (playWhen.HasValue("scene"))
                            {
                                p.playWhen.scene = Enums.Parse<Enums.Scenes>(playWhen.GetValue("scene"));
                            }
                            if (playWhen.HasValue("situation"))
                            {
                                p.playWhen.situation = Enums.Parse<Vessel.Situations>(playWhen.GetValue("situation"));
                            }
                            if (playWhen.HasValue("cameraMode"))
                            {
                                p.playWhen.cameraMode = Enums.Parse<Enums.CameraModes>(playWhen.GetValue("cameraMode"));
                            }
                            if (playWhen.HasValue("bodyName"))
                            {
                                p.playWhen.bodyName = playWhen.GetValue("bodyName");
                            }
                            if (playWhen.HasValue("maxVelocitySurface"))
                            {
                                p.playWhen.maxVelocitySurface = float.Parse(playWhen.GetValue("maxVelocitySurface"));
                            }
                            if (playWhen.HasValue("minVelocitySurface"))
                            {
                                p.playWhen.minVelocitySurface = float.Parse(playWhen.GetValue("minVelocitySurface"));
                            }
                            if (playWhen.HasValue("maxVelocityOrbital"))
                            {
                                p.playWhen.maxVelocityOrbital = float.Parse(playWhen.GetValue("maxVelocityOrbital"));
                            }
                            if (playWhen.HasValue("minVelocityOrbital"))
                            {
                                p.playWhen.minVelocityOrbital = float.Parse(playWhen.GetValue("minVelocityOrbital"));
                            }
                            if (playWhen.HasValue("maxAltitude"))
                            {
                                p.playWhen.maxAltitude = float.Parse(playWhen.GetValue("maxAltitude"));
                            }
                            if (playWhen.HasValue("minAltitude"))
                            {
                                p.playWhen.minAltitude = float.Parse(playWhen.GetValue("minAltitude"));
                            }
                            if (playWhen.HasValue("maxVesselDistance"))
                            {
                                p.playWhen.maxVesselDistance = float.Parse(playWhen.GetValue("maxVesselDistance"));
                            }
                            if (playWhen.HasValue("minVesselDistance"))
                            {
                                p.playWhen.minVesselDistance = float.Parse(playWhen.GetValue("minAltitude"));
                            }
                            if (playWhen.HasValue("vesselState"))
                            {
                                p.playWhen.vesselState = Enums.Parse<Enums.VesselState>(playWhen.GetValue("vesselState"));
                            }
                        }

                        playlists.Add(p);
                        EventManager.Instance.TrackEventsForPlaylist(p);
                    }
                }

                Utils.Log("Done loading playlists.");

                return playlists;
            }
            catch (Exception ex)
            {
                Debug.LogError("[STED] LoadPlaylists Error: " + ex.Message);
                return LoadStockPlaylist();
            }
        }

        private static List<Playlist> LoadStockPlaylist()
        {
            Utils.Log("Loading stock playlists.");
            List<Playlist> playlists = new List<Playlist>();

            playlists.Add(new Playlist
            {
                name = "Construction",
                loop = true,
                shuffle = true,
                preloadTime = 5,
                tracks = new List<string> {
                    "KSP_Construction01",
                    "KSP_Construction02",
                    "KSP_Construction03",
                    "Groove Grove",
                    "Brittle Rille"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.VAB | Enums.Scenes.SPH
                }
            });

            playlists.Add(new Playlist
            {
                name = "Space",
                loop = true,
                shuffle = true,
                preloadTime = 5,
                tracks = new List<string> {
                    "KSP_SpaceAmbience01",
                    "KSP_SpaceAmbience02",
                    "KSP_SpaceAmbience03",
                    "KSP_SpaceAmbience04",
                    "Arcadia",
                    "Bathed in the Light",
                    "Dreamy Flashback",
                    "Frost Waltz",
                    "Frost Waltz (Alternate)",
                    "Frozen Star",
                    "Impact Lento",
                    "Wizardtorium",
                    "KSP_MainTheme",
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.Flight,
                    inAtmosphere = Enums.Selector.False
                }
            });

            playlists.Add(new Playlist
            {
                name = "Astronaut Complex",
                loop = true,
                tracks = new List<string> {
                    "KSP_AstronautComplexAmbience"
                },
                playWhen = new Playlist.Prerequisites
                {
                    //scene = Enums.Scene.AstronautComplex TODO
                    scene = Enums.Scenes.PSystem
                }
            });

            playlists.Add(new Playlist
            {
                name = "Credits",
                loop = true,
                tracks = new List<string> {
                    "KSP_Credits"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.Credits
                }
            });

            playlists.Add(new Playlist
            {
                name = "Menu ambience",
                loop = true,
                preloadTime = 5,
                enabled = false,
                tracks = new List<string> {
                    "KSP_MenuAmbience"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.MainMenu
                }
            });

            Playlist spaceCentreAmbience = new Playlist
            {
                name = "Space centre",
                loop = true,
                preloadTime = 5,
                tracks = new List<string> {
                    "KSP_SpaceCenterAmbience"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.SpaceCentre
                }
            };
            playlists.Add(spaceCentreAmbience);

            playlists.Add(new Playlist
            {
                name = "Menu theme",
                loop = false,
                preloadTime = 5,
                disableAfterPlay = true,
                playNext = "Menu ambience",
                tracks = new List<string> {
                    "KSP_MainTheme"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.MainMenu
                }
            });

            playlists.Add(new Playlist
            {
                name = "Mission control ambience",
                loop = true,
                tracks = new List<string> {
                    "KSP_MissionControlAmbience" // TODO - Verify this
                },
                playWhen = new Playlist.Prerequisites
                {
                    // scene = Enums.Scene.MissionControl TODO
                    scene = Enums.Scenes.Loading
                }
            });

            playlists.Add(new Playlist
            {
                name = "Research complex ambience",
                loop = true,
                tracks = new List<string> {
                    "KSP_ResearchAndDevelopment" // TODO - Verify this
                },
                playWhen = new Playlist.Prerequisites
                {
                    //scene = Enums.Scene.ResearchComplex TODO
                    scene = Enums.Scenes.LoadingBuffer
                }
            });

            playlists.Add(new Playlist
            {
                name = "Space center ambience",
                loop = true,
                tracks = new List<string> {
                    "KSP_SpaceCenterAmbience" // TODO - Verify this
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.SpaceCentre
                }
            });

            playlists.Add(new Playlist
            {
                name = "SPH ambience",
                loop = true,
                tracks = new List<string> {
                    "KSP_SPHAmbience"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.SPH
                }
            });

            playlists.Add(new Playlist
            {
                name = "Tracking station ambience",
                loop = true,
                tracks = new List<string> {
                    "KSP_TrackingStation"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.TrackingStation
                }
            });

            playlists.Add(new Playlist
            {
                name = "VAB ambience",
                loop = true,
                tracks = new List<string> {
                    "KSP_VABAmbience"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.VAB
                }
            });

            return playlists;
        }

        public static void SavePlaylists(List<Playlist> playlists)
        {
            try
            {
                ConfigNode settings = new ConfigNode();

                foreach (Playlist pl in playlists)
                {
                    ConfigNode n = settings.AddNode("playlist");

                    n.AddValue("name", pl.name);
                    if (!pl.enabled)
                        n.AddValue("enabled", pl.enabled);
                    n.AddValue("loop", pl.loop);
                    n.AddValue("shuffle", pl.shuffle);
                    if (pl.preloadTime > 0 && pl.preloadTime != float.MaxValue)
                        n.AddValue("preloadTime", pl.preloadTime);
                    n.AddValue("pauseOnGamePause", pl.pauseOnGamePause);
                    n.AddValue("disableAfterPlay", pl.disableAfterPlay);
                    if (!string.IsNullOrEmpty(pl.playNext))
                        n.AddValue("playNext", pl.playNext);
                    if (!string.IsNullOrEmpty(pl.playBefore))
                        n.AddValue("playBefore", pl.playBefore);
                    if (!string.IsNullOrEmpty(pl.playAfter))
                        n.AddValue("playAfter", pl.playAfter);
                    if (pl.channel != 0)
                        n.AddValue("channel", pl.channel);

                    /* Not yet implemented.
                    ConfigNode fade = n.AddNode("fade");
                    fade.AddValue("fadeIn", pl.fade.fadeIn);
                    fade.AddValue("fadeOut", pl.fade.fadeOut);
                    fade.AddValue("crossfade", pl.fade.crossfade);
                    ConfigNode trackFade = n.AddNode("trackFade");
                    trackFade.AddValue("fadeIn", pl.trackFade.fadeIn);
                    trackFade.AddValue("fadeOut", pl.trackFade.fadeOut);
                    trackFade.AddValue("crossfade", pl.trackFade.crossfade);*/

                    ConfigNode trackNode = n.AddNode("tracks");
                    foreach (string trackName in pl.tracks)
                        trackNode.AddValue("track", trackName);

                    ConfigNode preReq = n.AddNode("playWhen");
                    if (pl.playWhen.paused != Enums.Selector.Either)
                        preReq.AddValue("paused", pl.playWhen.paused);
                    if (pl.playWhen.inAtmosphere != Enums.Selector.Either)
                        preReq.AddValue("inAtmosphere", pl.playWhen.inAtmosphere);
                    if (pl.playWhen.timeOfDay != Enums.TimesOfDay.Any)
                        preReq.AddValue("timeOfDay", pl.playWhen.timeOfDay.ToString().Replace(", ", " | "));
                    if (pl.playWhen.maxVelocitySurface != float.MaxValue)
                        preReq.AddValue("maxVelocitySurface", pl.playWhen.maxVelocitySurface);
                    if (pl.playWhen.minVelocitySurface != float.MinValue && pl.playWhen.minVelocitySurface != float.MaxValue)
                        preReq.AddValue("minVelocitySurface", pl.playWhen.minVelocitySurface);
                    if (pl.playWhen.maxVelocityOrbital != float.MaxValue)
                        preReq.AddValue("maxVelocityOrbital", pl.playWhen.maxVelocityOrbital);
                    if (pl.playWhen.minVelocityOrbital != float.MinValue && pl.playWhen.minVelocityOrbital != float.MaxValue)
                        preReq.AddValue("minVelocityOrbital", pl.playWhen.minVelocityOrbital);
                    if (pl.playWhen.maxAltitude != float.MaxValue)
                        preReq.AddValue("maxAltitude", pl.playWhen.maxAltitude);
                    if (pl.playWhen.minAltitude != float.MinValue && pl.playWhen.minAltitude != float.MaxValue)
                        preReq.AddValue("minAltitude", pl.playWhen.minAltitude);
                    if (pl.playWhen.maxVesselDistance != float.MaxValue)
                        preReq.AddValue("maxVesselDistance", pl.playWhen.maxVesselDistance);
                    if (pl.playWhen.minVesselDistance != 0)
                        preReq.AddValue("minVesselDistance", pl.playWhen.minVesselDistance);
                    if (pl.playWhen.vesselState != Enums.VesselState.Any && pl.playWhen.vesselState != 0)
                        preReq.AddValue("vesselState", pl.playWhen.vesselState);
                    if (pl.playWhen.scene != Enums.Scenes.Any)
                        preReq.AddValue("scene", pl.playWhen.scene.ToString().Replace(", ", " | "));
                    if (pl.playWhen.situation != Enums.AnyVesselSituation)
                        preReq.AddValue("situation", pl.playWhen.situation);
                    if (pl.playWhen.cameraMode != Enums.CameraModes.Any)
                        preReq.AddValue("cameraMode", pl.playWhen.cameraMode.ToString().Replace(", ", " | "));
                    if (!string.IsNullOrEmpty(pl.playWhen.bodyName))
                        preReq.AddValue("bodyName", pl.playWhen.bodyName);
                }

                settings.Save(_configSavePath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[STED] SavePlaylists Error: " + ex.Message);
            }
        }

        /*public void Load()
        {
            try
            {
                AudioClip clip = null;
                string track = String.Empty;
                unusedTracks.Clear();

                config = ConfigNode.Load(configSavePath);
                if (config == null)
                {
                    Utils.LogWarning("STED -- No config file present.");
                    unusedTracks.AddRange(GetUserTrackNames());
                    Save();
                    return;
                }

                unusedTracks.AddRange(GetAllTrackNames());

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
                                clip = GetAudioClip(t);
                                if (clip != null)
                                {
                                    Utils.Log("Adding " + clip + " to SpacePlaylist");
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
                                clip = GetAudioClip(t);
                                if (clip != null)
                                {
                                    Utils.Log("Adding " + clip + " to ConstructionPlaylist");
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
                    {
                        Utils.Log("AstroComplexAmbience: Disabling track");
                        music.astroComplexAmbience = emptyTrack;
                    }
                    else
                    {
                        clip = GetAudioClip(track);
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
                    {
                        Utils.Log("credits: Disabling track");
                        music.credits = emptyTrack;
                    }
                    else
                    {
                        clip = GetAudioClip(track);
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
                    {
                        Utils.Log("menuAmbience: Disabling track");
                        music.menuAmbience = emptyTrack;
                    }
                    else
                    {
                        clip = GetAudioClip(track);
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
                    {
                        Utils.Log("menuTheme: Disabling track");
                        music.menuTheme = emptyTrack;
                    }
                    else
                    {
                        clip = GetAudioClip(track);
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
                    {
                        Utils.Log("researchComplexAmbience: Disabling track");
                        music.researchComplexAmbience = emptyTrack;
                    }
                    else
                    {
                        clip = GetAudioClip(track);
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
                    {
                        Utils.Log("spaceCenterAmbience: Disabling track");
                        music.spaceCenterAmbience = emptyTrack;
                    }
                    else
                    {
                        clip = GetAudioClip(track);
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
                    {
                        Utils.Log("SPHAmbience: Disabling track");
                        music.SPHAmbience = emptyTrack;
                    }
                    else
                    {
                        clip = GetAudioClip(track);
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
                        clip = GetAudioClip(track);
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
                        clip = GetAudioClip(track);
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
                Utils.LogError("STED Load error: " + ex.Message);
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
                Utils.LogError("STED Save error: " + ex.Message);
            }
        }*/
    }
}
