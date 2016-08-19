using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundtrackEditor
{
    public class SoundTest
    {
        public static void CreatePlaylists(List<Playlist> Playlists)
        {
            Utils.Log("Creating test playlists");
            Playlists.Add(new Playlist
            {
                name = "Main menu",
                loop = false,
                //fade = new Playlist.Fade { fadeIn = 5, fadeOut = 5 },
                preloadTime = 5,
                tracks = new List<string> {
                    "KSP_MainTheme",
                    "KSP_MenuAmbience"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.MainMenu
                }
            });

            Playlists.Add(new Playlist
            {
                name = "Space centre",
                loop = true,
                preloadTime = 5,
                tracks = new List<string> {
                    "dobroide-forest"/*,
                    "KSP_SpaceCenterAmbience"*/
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scenes.SpaceCentre
                }
            });

            /* TODO
            Playlist astroComplexAmbience = new Playlist
            {
                name = "Astronaut Complex",
                loop = true,
                tracks = new List<string> {
                    "KSP_AstronautComplexAmbience"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Playlist.Prerequisites.Scene. AstronautComplex
                }
            };
            Playlists.Add(astroComplexAmbience);


            credits = KSP_Credits
            researchComplexAmbience = KSP_ResearchAndDevelopment
            spaceCenterAmbience = SoundtrackEditor/Music/dobroide-forest
            SPHAmbience = KSP_SPHAmbience
            trackingAmbience = KSP_TrackingStation
            VABAmbience = KSP_VABAmbience* /


            Playlists.Add(new Playlist
            {
                name = "Editor",
                loop = true,
                shuffle = true,
                preloadTime = 5,
                tracks = new List<string> {
                    "KSP_Construction01",
                    "KSP_Construction02",
                    "KSP_Construction03",
                    "KSP - VAB_SPH_SneakyAdventure",
                    "Groove Grove",
                    "Brittle Rille"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scene.VAB | Enums.Scene.SPH
                }
            });

            Playlists.Add(new Playlist
            {
                name = "Space",
                loop = true,
                shuffle = true,
                preloadTime = 5,
                //trackFade = new Playlist.Fade { fadeIn = 5, fadeOut = 5 },
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

            /*Playlists.Add(new Playlist
            {
                name = "Atmosphere",
                loop = true,
                shuffle = true,
                tracks = new List<string> {
                    "05 Dayvan Cowboy"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Enums.Scene.Flight,
                    inAtmosphere = Enums.Selector.Yes
                }
            });*/

            /*Playlist any = new Playlist
            {
                name = "Any",
                loop = false,
                tracks = new List<string> {
                    "Mysterioso March",
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = Scene.Any
                }
            };
            Playlists.Add(any);*/

            /*
                astroComplexAmbience = KSP_AstronautComplexAmbience
                credits = KSP_Credits
                menuAmbience = KSP_MenuAmbience
                menuTheme = SoundtrackEditor/Music/Space/Peaceful Desolation
                researchComplexAmbience = KSP_ResearchAndDevelopment
                spaceCenterAmbience = SoundtrackEditor/Music/dobroide-forest
                SPHAmbience = KSP_SPHAmbience
                trackingAmbience = KSP_TrackingStation
                VABAmbience = KSP_VABAmbience
             */

            /*Playlist p1 = new Playlist
            {
                disableAfterPlay = false,
                enabled = true,
                loop = true,
                name = "Test Playlist",
                shuffle = false,
                tracks = new List<string> {
                    "Arcadia",
                    "Bathed in the Light",
                    "Dreamy Flashback",
                    "Frost Waltz",
                    "Frost Waltz (Alternate)",
                    "Frozen Star",
                    "Impact Lento",
                    "Wizardtorium",
                    "KSP_MainTheme",
                    "KSP_SpaceCenterAmbience"
                    //"Darkest Child",
                    //"Dragon and Toast",
                    //"Fairytale Waltz",
                    //"Martian Cowboy",
                    //"Mysterioso March",
                    //"Numinous Shine",
                    //"On The Shore",
                    //"Peaceful Desolation",
                    //"The Other Side of the Door",
                    //"Stand Up For Rock N Roll"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = GameScenes.MAINMENU
                }
            };
            Playlists.Add(p1);
            CurrentPlaylist = p1;*/

            /*Playlist p2 = new Playlist
            {
                disableAfterPlay = false,
                enabled = true,
                loop = true,
                name = "Test Playlist 2",
                shuffle = false,
                tracks = new List<string> {
                    "KSP_SpaceAmbience01",
                    "KSP_SpaceAmbience02",
                    "KSP_SpaceAmbience03",
                    "KSP_SpaceAmbience04"
                },
                playWhen = new Playlist.Prerequisites
                {
                    scene = GameScenes.SPACECENTER
                }
            };
            Playlists.Add(p2);*/
        }

    }
}
