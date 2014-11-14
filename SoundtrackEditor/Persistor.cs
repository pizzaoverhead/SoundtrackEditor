using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SoundtrackEditor
{
    class Persistor
    {
        private ConfigNode _config;
        private string _configSavePath = "GameData/SoundtrackEditor/settings.cfg";
        public List<Playlist> LoadPlaylists()
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
                        if (node.HasValue("disableAfterPlay"))
                            p.disableAfterPlay = bool.Parse(node.GetValue("disableAfterPlay"));
                        if (node.HasValue("playNext"))
                            p.playNext = node.GetValue("playNext");
                        if (node.HasValue("channel"))
                            p.channel = int.Parse(node.GetValue("channel"));
                        if (node.HasValue("preloadTime"))
                            p.preloadTime = float.Parse(node.GetValue("preloadTime"));
                        //p.fade = config.GetValue("fade"); TODO
                        //p.trackFade = config.GetValue("trackFade"); TODO

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
                            if (playWhen.HasValue("scene"))
                                p.playWhen.scene = Enums.Parse<Enums.Scene>(playWhen.GetValue("scene"));
                            if (playWhen.HasValue("body"))
                                p.playWhen.bodyName = playWhen.GetValue("body");
                            if (playWhen.HasValue("situation"))
                                p.playWhen.situation = Enums.Parse<Vessel.Situations>(playWhen.GetValue("situation"));
                            if (playWhen.HasValue("cameraMode"))
                                p.playWhen.cameraMode = Enums.Parse<Enums.CameraMode>(playWhen.GetValue("cameraMode"));
                            if (playWhen.HasValue("inAtmosphere"))
                                p.playWhen.inAtmosphere = Enums.Parse<Enums.Selector>(playWhen.GetValue("inAtmosphere"));
                            if (playWhen.HasValue("surfaceVelocity"))
                                p.playWhen.surfaceVelocity = float.Parse(playWhen.GetValue("surfaceVelocity"));
                            if (playWhen.HasValue("surfaceVelocityBelow"))
                                p.playWhen.surfaceVelocity = float.Parse(playWhen.GetValue("surfaceVelocityBelow"));
                            if (playWhen.HasValue("surfaceVelocityAbove"))
                                p.playWhen.surfaceVelocity = float.Parse(playWhen.GetValue("surfaceVelocityAbove"));
                            if (playWhen.HasValue("velocityOrbital"))
                                p.playWhen.surfaceVelocity = float.Parse(playWhen.GetValue("velocityOrbital"));
                            if (playWhen.HasValue("maxVelocityOrbital"))
                                p.playWhen.surfaceVelocity = float.Parse(playWhen.GetValue("maxVelocityOrbital"));
                            if (playWhen.HasValue("minVelocityOrbital"))
                                p.playWhen.surfaceVelocity = float.Parse(playWhen.GetValue("minVelocityOrbital"));
                        }

                        playlists.Add(p);
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

        private List<Playlist> LoadStockPlaylist()
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
                    scene = Enums.Scene.VAB | Enums.Scene.SPH
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
                    scene = Enums.Scene.Flight,
                    inAtmosphere = Enums.Selector.No
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
                    scene = Enums.Scene.PSystem
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
                    scene = Enums.Scene.Credits
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
                    scene = Enums.Scene.MainMenu
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
                    scene = Enums.Scene.SpaceCentre
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
                    scene = Enums.Scene.MainMenu
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
                    scene = Enums.Scene.Loading
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
                    scene = Enums.Scene.LoadingBuffer
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
                    scene = Enums.Scene.SpaceCentre
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
                    scene = Enums.Scene.SPH
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
                    scene = Enums.Scene.TrackingStation
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
                    scene = Enums.Scene.VAB
                }
            });

            return playlists;
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
