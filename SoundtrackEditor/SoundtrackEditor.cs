using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

/* Plans
 * 
 * Fixes:
 * Linux/Mac support
 * Memory usage
 * Loading times
 * 
 * Features:
 * Allow playlists for current single-track items.
 * Situational music
 */

namespace SoundtrackEditor
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class SoundtrackEditor : MonoBehaviour
	{
		// TODO: Find time of day, play night sounds.
		//SunLight sl = (SunLight)FindObjectOfType(typeof(SunLight));
		//float x = sl.timeOfTheDay;
		MusicLogic music;
		ConfigNode config;
		string configSavePath = "GameData/SoundtrackEditor/settings.cfg";
		List<string> unusedTracks = new List<string>();
		// Create a new empty audio clip to replace the stock ones when they are disabled.
		AudioClip emptyTrack = AudioClip.Create("none", 44100, 1, 44100, false, true);
		private string _musicPath;
		public static Playlist.Prerequisites CurrentSituation = new Playlist.Prerequisites() { scene = Scene.Loading };
		public List<Playlist> Playlists = new List<Playlist>();
		public Playlist CurrentPlaylist;
		public AudioSource Speaker;
		public AudioClip CurrentClip;
		public AudioClip PreloadClip;
		public static bool InitialLoadingComplete = false; // Whether we have reached the main menu yet during this session.
		
		private System.Timers.Timer fadeOutTimer = new System.Timers.Timer();
		private System.Timers.Timer preloadTimer = new System.Timers.Timer();

		public string MusicPath
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
					Debug.Log("Music path is: " + _musicPath);
				}
				return _musicPath;
			}

		}

		public void Start()
		{
			music = MusicLogic.fetch;

			fadeOutTimer.Elapsed += new System.Timers.ElapsedEventHandler(fadeOutTimer_Elapsed);
			preloadTimer.Elapsed += new System.Timers.ElapsedEventHandler(preloadTimer_Elapsed);

			//Debug.Log("# OS Version: " + Environment.OSVersion + ", Platform: " + Environment.OSVersion.Platform);
			//DeleteAllStock();

			DontDestroyOnLoad(gameObject);

			/*Load();
			UnloadUnusedTracks();

			// Remove positional effects.
			music.audio1.panLevel = 0;
			music.audio1.dopplerLevel = 0;
			music.audio2.panLevel = 0;
			music.audio2.dopplerLevel = 0;*/

			UnloadStockMusicPlayer();

			// Set up the main audio source.
			Speaker = gameObject.AddComponent<AudioSource>();
			// Disable positional effects.
			Speaker.panLevel = 0;
			Speaker.dopplerLevel = 0;
			Speaker.loop = false;
			Speaker.volume = GameSettings.MUSIC_VOLUME;

			// Set up test playlists.
			Playlists.Add(new Playlist
			{
				name = "Main menu",
				loop = false,
				fade = new Playlist.Fade { fadeIn = 5, fadeOut = 5 },
				preloadTime = 5,
				tracks = new List<string> {
					"KSP_MainTheme",
					"KSP_MenuAmbience"
				},
				playWhen = new Playlist.Prerequisites
				{
					scene = Scene.MainMenu
				}
			});

			Playlists.Add(new Playlist
			{
				name = "Space centre",
				loop = true,
				preloadTime = 5,
				tracks = new List<string> {
					"dobroide-forest",
					"KSP_SpaceCenterAmbience"
				},
				playWhen = new Playlist.Prerequisites
				{
					scene = Scene.SpaceCentre
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
			VABAmbience = KSP_VABAmbience*/


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
					scene = Scene.VAB | Scene.SPH
				}
			});

			Playlists.Add(new Playlist
			{
				name = "Space",
				loop = true,
				shuffle = true,
				preloadTime = 5,
				trackFade = new Playlist.Fade { fadeIn = 5, fadeOut = 5 },
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
					scene = Scene.Flight,
					inAtmosphere = Selector.No
				}
			});

			Playlists.Add(new Playlist
			{
				name = "Atmosphere",
				loop = true,
				shuffle = true,
				tracks = new List<string> {
					"05 Dayvan Cowboy"
				},
				playWhen = new Playlist.Prerequisites
				{
					scene = Scene.Flight,
					inAtmosphere = Selector.Yes
				}
			});

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


			// TODO: Change volume on unpause or main menu.
			GameEvents.onActiveJointNeedUpdate.Add(onActiveJointNeedUpdate);
			GameEvents.onCollision.Add(onCollision);
			GameEvents.onCrash.Add(onCrash);
			GameEvents.onCrashSplashdown.Add(onCrashSplashdown);
			GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
			GameEvents.onCrewKilled.Add(onCrewKilled);
			GameEvents.onCrewOnEva.Add(onCrewOnEva);
			GameEvents.onDominantBodyChange.Add(onDominantBodyChange);
			GameEvents.onFlagSelect.Add(onFlagSelect);
			GameEvents.onFlightReady.Add(onFlightReady);
			GameEvents.onFloatingOriginShift.Add(onFloatingOriginShift);
			GameEvents.onGamePause.Add(onGamePause);
			GameEvents.onGameSceneLoadRequested.Add(OnSceneLoad);
			GameEvents.onGameStateCreated.Add(onGameStateCreated);
			GameEvents.onGameStateSaved.Add(onGameStateSaved);
			GameEvents.onGameUnpause.Add(onGameUnpause);
			GameEvents.onGUIAstronautComplexDespawn.Add(onGUIAstronautComplexDespawn);
			GameEvents.onGUIAstronautComplexSpawn.Add(onGUIAstronautComplexSpawn);
			GameEvents.onGUILaunchScreenDespawn.Add(onGUILaunchScreenDespawn);
			GameEvents.onGUILaunchScreenSpawn.Add(onGUILaunchScreenSpawn);
			GameEvents.onGUIRecoveryDialogDespawn.Add(onGUIRecoveryDialogDespawn);
			GameEvents.onGUIRecoveryDialogSpawn.Add(onGUIRecoveryDialogSpawn);
			GameEvents.onGUIRnDComplexDespawn.Add(onGUIRnDComplexDespawn);
			GameEvents.onGUIRnDComplexSpawn.Add(onGUIRnDComplexSpawn);
			GameEvents.onHideUI.Add(onHideUI);
			GameEvents.onInputLocksModified.Add(onInputLocksModified);
			GameEvents.onJointBreak.Add(onJointBreak);
			GameEvents.onKnowledgeChanged.Add(onKnowledgeChanged);
			GameEvents.onKrakensbaneDisengage.Add(onKrakensbaneDisengage);
			GameEvents.onKrakensbaneEngage.Add(onKrakensbaneEngage);
			GameEvents.onLaunch.Add(onLaunch);
			GameEvents.onMissionFlagSelect.Add(onMissionFlagSelect);
			GameEvents.onNewVesselCreated.Add(onNewVesselCreated);
			GameEvents.onOverheat.Add(onOverheat);
			GameEvents.onPartActionUICreate.Add(onPartActionUICreate);
			GameEvents.onPartActionUIDismiss.Add(onPartActionUIDismiss);
			GameEvents.onPartAttach.Add(onPartAttach);
			GameEvents.onPartCouple.Add(onPartCouple);
			//GameEvents.onPartDestroyed.Add(onPartDestroyed);
			GameEvents.onPartDie.Add(onPartDie);
			GameEvents.onPartExplode.Add(onPartExplode);
			GameEvents.onPartJointBreak.Add(onPartJointBreak);
			GameEvents.onPartPack.Add(onPartPack);
			GameEvents.OnPartPurchased.Add(OnPartPurchased);
			GameEvents.onPartRemove.Add(onPartRemove);
			GameEvents.onPartUndock.Add(onPartUndock);
			GameEvents.onPartUnpack.Add(onPartUnpack);
			GameEvents.onPlanetariumTargetChanged.Add(onPlanetariumTargetChanged);
			GameEvents.OnProgressComplete.Add(OnProgressComplete);
			GameEvents.OnProgressReached.Add(OnProgressReached);
			GameEvents.onRotatingFrameTransition.Add(onRotatingFrameTransition);
			GameEvents.onSameVesselDock.Add(onSameVesselDock);
			GameEvents.onSameVesselUndock.Add(onSameVesselUndock);
			GameEvents.onShowUI.Add(onShowUI);
			GameEvents.onSplashDamage.Add(onSplashDamage);
			GameEvents.onStageActivate.Add(onStageActivate);
			GameEvents.onStageSeparation.Add(onStageSeparation);
			GameEvents.OnTechnologyResearched.Add(OnTechnologyResearched);
			GameEvents.onTimeWarpRateChanged.Add(onTimeWarpRateChanged);
			GameEvents.onUndock.Add(onUndock);
			GameEvents.onVesselChange.Add(onVesselChange);
			GameEvents.onVesselCreate.Add(onVesselCreate);
			GameEvents.onVesselDestroy.Add(onVesselDestroy);
			GameEvents.onVesselGoOffRails.Add(onVesselGoOffRails);
			GameEvents.onVesselGoOnRails.Add(onVesselGoOnRails);
			GameEvents.onVesselLoaded.Add(onVesselLoaded);
			GameEvents.onVesselOrbitClosed.Add(onVesselOrbitClosed);
			GameEvents.onVesselOrbitEscaped.Add(onVesselOrbitEscaped);
			GameEvents.onVesselRecovered.Add(onVesselRecovered);
			GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);
			GameEvents.onVesselRename.Add(onVesselRename);
			GameEvents.onVesselSituationChange.Add(onVesselSituationChange);
			GameEvents.onVesselSOIChanged.Add(onVesselSOIChanged);
			GameEvents.onVesselTerminated.Add(onVesselTerminated);
			GameEvents.onVesselWasModified.Add(onVesselWasModified);
			GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);
		}

		private void onActiveJointNeedUpdate(Vessel v) { Debug.Log("#onActiveJointNeedUpdate"); }
		private void onCollision(EventReport e) { Debug.Log("#onActiveJointNeedUpdate"); }
		private void onCrash(EventReport e) { Debug.Log("#onCrash"); }
		private void onCrashSplashdown(EventReport e) { Debug.Log("#onCrashSplashdown"); }
		private void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> a) { Debug.Log("#onCrewBoardVessel"); }
		private void onCrewKilled(EventReport e) { Debug.Log("#onCrewKilled"); }
		private void onCrewOnEva(GameEvents.FromToAction<Part, Part> a) { Debug.Log("#onCrewOnEva"); }
		private void onDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> bodyChange)
		{
			Debug.Log("#Body change from " + bodyChange.from + " to " + bodyChange.to);
			CurrentSituation.bodyName = bodyChange.to.name;
			OnSituationChanged();
		}
		private void onFlagSelect(string f) { Debug.Log("#onFlagSelect"); }
		private void onFlightReady() { Debug.Log("#onFlightReady"); }
		private void onFloatingOriginShift(Vector3d s) { Debug.Log("#onFloatingOriginShift"); }
		private void onGamePause() { Debug.Log("#onGamePause"); }

		bool _guiSetupComplete = false;
		private void OnSceneLoad(GameScenes scene)
		{
			Debug.Log("#Changing scene: " + scene);
			if (!InitialLoadingComplete && scene.Equals(GameScenes.MAINMENU))
				InitialLoadingComplete = true;

			CurrentSituation.scene = ConvertScene(scene);
			OnSituationChanged();


			if (!_guiSetupComplete && scene.Equals(GameScenes.SPACECENTER))
			{
				Debug.Log("STED - Adding Draw GUI");
				_guiSetupComplete = true;
				RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
			}
		}
		private void onGameStateCreated(Game g) { Debug.Log("#onGameStateCreated"); }
		private void onGameStateSaved(Game g) { Debug.Log("#onGameStateSaved"); }
		private void onGameUnpause() { Debug.Log("#onGameUnpause"); }
		private void onGUIAstronautComplexDespawn() { Debug.Log("#onGUIAstronautComplexDespawn"); }
		private void onGUIAstronautComplexSpawn() { Debug.Log("#onGUIAstronautComplexSpawn"); }
		private void onGUILaunchScreenDespawn() { Debug.Log("#onGUILaunchScreenDespawn"); }
		private void onGUILaunchScreenSpawn(GameEvents.VesselSpawnInfo i) { Debug.Log("#onGUILaunchScreenSpawn"); }
		private void onGUIRecoveryDialogDespawn(ExperimentsRecoveryDialog d) { Debug.Log("#onGUIRecoveryDialogDespawn"); }
		private void onGUIRecoveryDialogSpawn(ExperimentsRecoveryDialog d) { Debug.Log("#onGUIRecoveryDialogSpawn"); }
		private void onGUIRnDComplexDespawn() { Debug.Log("#onGUIRnDComplexDespawn"); }
		private void onGUIRnDComplexSpawn() { Debug.Log("#onGUIRnDComplexSpawn"); }
		private void onHideUI() { Debug.Log("#onHideUI"); }
		private void onInputLocksModified(GameEvents.FromToAction<ControlTypes, ControlTypes> a) { Debug.Log("#onInputLocksModified"); }
		private void onJointBreak(EventReport r) { Debug.Log("#onJointBreak"); }
		private void onKnowledgeChanged(GameEvents.HostedFromToAction<IDiscoverable, DiscoveryLevels> a) { Debug.Log("#onKnowledgeChanged"); }
		private void onKrakensbaneDisengage(Vector3d v) { Debug.Log("#onKrakensbaneDisengage"); }
		private void onKrakensbaneEngage(Vector3d v) { Debug.Log("#onKrakensbaneEngage"); }
		private void onLaunch(EventReport r) { Debug.Log("#onLaunch"); }
		private void onMissionFlagSelect(string f) { Debug.Log("#onMissionFlagSelect"); }
		private void onNewVesselCreated(Vessel v) { Debug.Log("#onNewVesselCreated"); }
		private void onOverheat(EventReport r) { Debug.Log("#onOverheat"); }
		private void onPartActionUICreate(Part p) { Debug.Log("#onPartActionUICreate"); }
		private void onPartActionUIDismiss(Part p) { Debug.Log("#onPartActionUIDismiss"); }
		private void onPartAttach(GameEvents.HostTargetAction<Part, Part> a) { Debug.Log("#onPartAttach"); }
		private void onPartCouple(GameEvents.FromToAction<Part, Part> a) { Debug.Log("#onPartCouple"); }
		//private void onPartDestroyed(Part p) { Debug.Log("#onPartDestroyed"); }
		private void onPartDie(Part p) { Debug.Log("#onPartDie"); }
		private void onPartExplode(GameEvents.ExplosionReaction e) { Debug.Log("#onPartExplode"); }
		private void onPartJointBreak(PartJoint j) { Debug.Log("#onPartJointBreak"); }
		private void onPartPack(Part p) { Debug.Log("#onPartPack"); }
		private void OnPartPurchased(AvailablePart p) { Debug.Log("#OnPartPurchased"); }
		private void onPartRemove(GameEvents.HostTargetAction<Part, Part> a) { Debug.Log("#onPartRemove"); }
		private void onPartUndock(Part p) { Debug.Log("#onPartUndock"); }
		private void onPartUnpack(Part p) { Debug.Log("#onPartUnpack"); }
		private void onPlanetariumTargetChanged(MapObject o) { Debug.Log("#onPlanetariumTargetChanged"); }
		private void OnProgressComplete(ProgressNode n) { Debug.Log("#OnProgressComplete"); }
		private void OnProgressReached(ProgressNode n) { Debug.Log("#OnProgressReached"); }
		private void onRotatingFrameTransition(GameEvents.HostTargetAction<CelestialBody, bool> a) { Debug.Log("#onRotatingFrameTransition"); }
		private void onSameVesselDock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> a) { Debug.Log("#onSameVesselDock"); }
		private void onSameVesselUndock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> a) { Debug.Log("#onSameVesselUndock"); }
		private void onShowUI() { Debug.Log("#onShowUI"); }
		private void onSplashDamage(EventReport r) { Debug.Log("#onSplashDamage"); }
		private void onStageActivate(int s) { Debug.Log("#onStageActivate"); }
		private void onStageSeparation(EventReport r) { Debug.Log("#onStageSeparation"); }
		private void OnTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> a) { Debug.Log("#OnTechnologyResearched"); }
		private void onTimeWarpRateChanged() { Debug.Log("#onTimeWarpRateChanged"); }
		private void onUndock(EventReport r) { Debug.Log("#onUndock"); }
		private void onVesselChange(Vessel v) { Debug.Log("#onVesselChange"); }
		private void onVesselCreate(Vessel v) { Debug.Log("#onVesselCreate"); }
		private void onVesselDestroy(Vessel v) { Debug.Log("#onVesselDestroy"); }
		private void onVesselGoOffRails(Vessel v) { Debug.Log("#onVesselGoOffRails"); }
		private void onVesselGoOnRails(Vessel v) { Debug.Log("#onVesselGoOnRails"); }
		private void onVesselLoaded(Vessel v) { Debug.Log("#onVesselLoaded"); }
		private void onVesselOrbitClosed(Vessel v) { Debug.Log("#onVesselOrbitClosed"); }
		private void onVesselOrbitEscaped(Vessel v) { Debug.Log("#onVesselOrbitEscaped"); }
		private void onVesselRecovered(ProtoVessel p) { Debug.Log("#onVesselRecovered"); }
		private void OnVesselRecoveryRequested(Vessel v) { Debug.Log("#OnVesselRecoveryRequested"); }
		private void onVesselRename(GameEvents.HostedFromToAction<Vessel, string> a) { Debug.Log("#onVesselRename"); }
		private void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> a) {
			Debug.Log("# Vessel situation changed");
			// TODO: Verify operation with active and non-active vessels.
			CurrentSituation.situation = a.to;
			OnSituationChanged();
		}
		private void onVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> a) { Debug.Log("#onVesselSOIChanged"); }
		private void onVesselTerminated(ProtoVessel p) { Debug.Log("#onVesselTerminated"); }
		private void onVesselWasModified(Vessel v) { Debug.Log("#onVesselWasModified"); }
		private void onVesselWillDestroy(Vessel v) { Debug.Log("#onVesselWillDestroy"); }

		/*private void Load()
		{
			try
			{
				AudioClip clip = null;
				string track = String.Empty;
				unusedTracks.Clear();

				config = ConfigNode.Load(configSavePath);
				if (config == null)
				{
					Debug.LogWarning("STED -- No config file present.");
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
									Debug.Log("Adding " + clip + " to SpacePlaylist");
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
									Debug.Log("Adding " + clip + " to ConstructionPlaylist");
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
						Debug.Log("AstroComplexAmbience: Disabling track");
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
						Debug.Log("credits: Disabling track");
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
						Debug.Log("menuAmbience: Disabling track");
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
						Debug.Log("menuTheme: Disabling track");
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
						Debug.Log("researchComplexAmbience: Disabling track");
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
						Debug.Log("spaceCenterAmbience: Disabling track");
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
						Debug.Log("SPHAmbience: Disabling track");
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
				Debug.LogError("STED Load error: " + ex.Message);
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
				Debug.LogError("STED Save error: " + ex.Message);
			}
		}

		private List<string> GetUserTrackNames()
		{
			List<string> clipNames = new List<string>();
			foreach (string file in Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories))
			{
				if (Path.GetExtension(file).Equals(".wav", StringComparison.InvariantCultureIgnoreCase) ||
					Path.GetExtension(file).Equals(".ogg", StringComparison.InvariantCultureIgnoreCase) ||
					Path.GetExtension(file).Equals(".mp3", StringComparison.InvariantCultureIgnoreCase))
					clipNames.Add(Path.GetFileNameWithoutExtension(file));
			}
			return clipNames;

			//List<AudioClip> userTracks = GameDatabase.Instance.databaseAudio.FindAll(c => c.ToString().StartsWith("SoundtrackEditor/"));
			//return userTracks.ConvertAll(c => c.name);
		}

		private List<string> GetAllTrackNames()
		{
			List<string> trackNames = new List<string>();
			trackNames.AddRange(GetUserTrackNames());
			trackNames.AddRange(music.constructionPlaylist.ConvertAll(t => t.name));
			trackNames.AddRange(music.spacePlaylist.ConvertAll(t => t.name));
			trackNames.Add(music.astroComplexAmbience.name);
			trackNames.Add(music.credits.name);
			trackNames.Add(music.menuAmbience.name);
			trackNames.Add(music.menuTheme.name);
			trackNames.Add(music.researchComplexAmbience.name);
			trackNames.Add(music.spaceCenterAmbience.name);
			trackNames.Add(music.SPHAmbience.name);
			trackNames.Add(music.trackingAmbience.name);
			trackNames.Add(music.VABAmbience.name);

			return trackNames;
		}

		private static bool IsRunningOnLinux()
		{
			// see http://mono-project.com/FAQ%3a_Technical#Mono_Platforms
			var p = (int)Environment.OSVersion.Platform;
			return (p == 4) || (p == 6) || (p == 128);
		}*/

		// TODO:
		//    [x] Get a list of all the files we have
		//    [x] Import the ones selected.
		//    [ ] Unload unused stock tracks
		//    [ ] Only load tracks when they are being played:    GameEvents.onGameSceneLoadRequested.Add(MyMethod); //(don't forget destroy)
		//    [ ] Unload them when done.

		public AudioClip GetAudioClip(string name)
		{
			//DirectoryInfo modDir = Directory.GetParent(AssemblyDirectory);
			//modDir.Parent = GameData
			//modDir.Parent.Parent = KSP root
			//Debug.Log("#Mod dir: " + modDir);
			//Debug.Log("#Grandparent: " + modDir.Parent.Parent.FullName);
			//string path = Path.Combine(modDir.Parent.Parent.FullName, "Music") + Path.DirectorySeparatorChar;

			foreach (string file in Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories))
			{
				if (name.Equals(Path.GetFileNameWithoutExtension(file)))
				{
					string ext = Path.GetExtension(file);
					Debug.Log("#Found " + name + ", with extension" + ext);
					switch (ext.ToUpperInvariant())
					{
						case ".WAV":
						case ".OGG":
							return LoadUnityAudioClip(file);
						case ".MP3":
							return LoadMp3Clip(file);
						default:
							Debug.Log("#Unknown extension found:" + ext);
							break;
					}
				}
			}
			// Failed to find the clip in the music folder. Check the game database instead.
			Debug.Log("#Attempting to load game database file: " + name);
			return GameDatabase.Instance.GetAudioClip(name);
		}

		private AudioClip LoadUnityAudioClip(string filePath)
		{
			// Load the audio clip into memory.
			WWW www = new WWW("file://" + filePath);
			AudioClip clip = www.audioClip;
			clip.name = Path.GetFileNameWithoutExtension(filePath);
			return clip;
		}

		private AudioClip LoadMp3Clip(string filePath)
		{
			MP3Import importer = new MP3Import();
			AudioClip clip = importer.StartImport(filePath);
			clip.name = Path.GetFileNameWithoutExtension(filePath);
			return clip;
		}

		private void UnloadUnusedTracks()
		{
			try
			{
				Debug.Log("Unloading Tracks...");
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
				Debug.Log(i + " tracks unloaded.");
			}
			catch (Exception ex)
			{
				Debug.LogError("STED Unload Tracks error: " + ex.Message);
			}
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
				Debug.LogError("STED: Load Audio Files error: " + ex.Message);
			}
		}*/

		protected Rect windowPos = new Rect(Screen.width / 10, Screen.height / 10, 10f, 10f);

		protected void drawGUI()
		{
			windowPos = GUILayout.Window(-5236628, windowPos, mainGUI, "Soundtrack Editor", GUILayout.Width(300), GUILayout.Height(50));
		}

		private void mainGUI(int windowID)
		{
			// TODO: GUI gets killed between scene changes.

			GUILayout.BeginVertical();
			GUILayout.Label("Now Playing:");
			
			if (CurrentClip != null)
				GUILayout.Label(CurrentClip.name);
			if (GUILayout.Button("Skip"))
				PlayNextTrack();
			if (GUILayout.Button("Unload"))
				AudioClip.DestroyImmediate(CurrentClip, true);
			if (GUILayout.Button("Unload stock"))
				UnloadStockMusicPlayer();
			if (CurrentPlaylist != null)
			{
				if (GUILayout.Button("Shuffle"))
					CurrentPlaylist.Shuffle();
				if (CurrentPlaylist.tracks != null)
				{
				foreach (var t in CurrentPlaylist.tracks)
				{
					if (CurrentPlaylist.tracks[CurrentPlaylist.trackIndex].Equals(t))
						GUILayout.Label("> " + t);
					else
						GUILayout.Label(t);
				}
					}
			}
			GUILayout.EndVertical();

			GUI.DragWindow();

			/* Old gui
			if (deleted)
			{
				GUILayout.Label("Deleted");
			}
			else
			{
				GUILayout.BeginHorizontal();

				GUILayout.BeginVertical();
				GUILayout.Label("Construction Playlist:");
				foreach (var clip in music.constructionPlaylist)
				{
					GUILayout.Label(clip.name);
				}
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				GUILayout.Label("Space Playlist:");
				foreach (var clip in music.spacePlaylist)
				{
					GUILayout.Label(clip.name);
				}
				GUILayout.EndVertical();

				GUILayout.BeginVertical();
				GUILayout.Label("astroComplexAmbience: " + music.astroComplexAmbience.name);
				GUILayout.Label("credits: " + music.credits.name);
				GUILayout.Label("menuAmbience: " + music.menuAmbience.name);
				GUILayout.Label("menuTheme: " + music.menuTheme.name);
				GUILayout.Label("researchComplexAmbience: " + music.researchComplexAmbience.name);
				GUILayout.Label("spaceCenterAmbience: " + music.spaceCenterAmbience.name);
				GUILayout.Label("SPHAmbience: " + music.SPHAmbience.name);
				GUILayout.Label("trackingAmbience: " + music.trackingAmbience.name);
				GUILayout.Label("VABAmbience: " + music.VABAmbience.name);
				if (GUILayout.Button("Delete All"))
					DeleteAllStock();

				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
			}
			GUI.DragWindow();*/
		}

		private void UnloadStockMusicPlayer()
		{
			// Add the stock sounds to the database so we can access them later.
			GameDatabase.Instance.databaseAudio.AddRange(music.constructionPlaylist);
			GameDatabase.Instance.databaseAudio.AddRange(music.spacePlaylist);
			GameDatabase.Instance.databaseAudio.Add(music.astroComplexAmbience);
			GameDatabase.Instance.databaseAudio.Add(music.credits);
			GameDatabase.Instance.databaseAudio.Add(music.menuAmbience);
			GameDatabase.Instance.databaseAudio.Add(music.menuTheme);
			GameDatabase.Instance.databaseAudio.Add(music.researchComplexAmbience);
			GameDatabase.Instance.databaseAudio.Add(music.spaceCenterAmbience);
			GameDatabase.Instance.databaseAudio.Add(music.SPHAmbience);
			GameDatabase.Instance.databaseAudio.Add(music.trackingAmbience);
			GameDatabase.Instance.databaseAudio.Add(music.VABAmbience);

			music.audio1.Stop();
			music.audio1.volume = 0;
			music.audio1.clip = emptyTrack;
			music.audio2.Stop();
			music.audio2.volume = 0;
			music.audio2.clip = emptyTrack;

			music.constructionPlaylist.Clear();
			music.spacePlaylist.Clear();
			music.astroComplexAmbience = emptyTrack;
			music.credits = emptyTrack;
			music.menuAmbience = emptyTrack;
			music.menuTheme = emptyTrack;
			music.researchComplexAmbience = emptyTrack;
			music.spaceCenterAmbience = emptyTrack;
			music.SPHAmbience = emptyTrack;
			music.trackingAmbience = emptyTrack;
			music.VABAmbience = emptyTrack;
		}

		private void DeleteAllStock()
		{
			List<AudioClip> allTracks = new List<AudioClip>{
				music.astroComplexAmbience,
				music.credits,
				music.menuAmbience,
				music.menuTheme,
				music.researchComplexAmbience,
				music.spaceCenterAmbience,
				music.SPHAmbience,
				music.trackingAmbience,
				music.VABAmbience
			};
			allTracks.AddRange(music.constructionPlaylist);
			allTracks.AddRange(music.spacePlaylist);

			foreach (var clip in allTracks)
			{
				//GameDatabase.Instance.RemoveAudioClip(clip.name);
				AudioClip.DestroyImmediate(clip, true);
			}

			//deleted = true;
		}

		private bool _loading = false;
		public void Update()
		{
			UpdateSituation();
			DoFade();
		}

		public void UpdateSituation()
		{
			bool changed = false;

			// Scene: Handled by events.
			// Body: Handled by events. TODO: Not suitable?
			// Situation: Handled by events.
			// Camera mode

			// Throws exceptions before the initial loading screen is completed.
			if (CurrentSituation.scene == Scene.Flight)
			{
				Vessel v = SoundtrackEditor.InitialLoadingComplete ? FlightGlobals.ActiveVessel : null;

				if (v != null)
				{
					if (!FlightGlobals.currentMainBody.name.Equals(CurrentSituation.bodyName))
					{
						CurrentSituation.bodyName = FlightGlobals.currentMainBody.name;
						changed = true;
					}

					Selector inAtmosphere = v.atmDensity > 0 ? Selector.Yes : Selector.No;
					if (CurrentSituation.inAtmosphere != inAtmosphere)
						changed = true;
					CurrentSituation.inAtmosphere = inAtmosphere;

					//FlightGlobals.currentMainBody.maxAtmosphereAltitude
					//v.atmDensity

					if (CameraManager.Instance != null)
					{
						changed |= CurrentSituation.cameraMode == ConvertCameraMode(CameraManager.Instance.currentCameraMode);
						CurrentSituation.cameraMode = ConvertCameraMode(CameraManager.Instance.currentCameraMode);
					}
				}
			}


			// TODO:
			// Velocity

			if (changed)
				OnSituationChanged();
		}

		public void OnSituationChanged()
		{
			// TODO: Pre-emtptively load tracks when near the end of the current one.

			Playlist p = SelectPlaylist();
			if (p == null)
			{
				StopPlayback();
				return;
			}

			if (!p.Equals(CurrentPlaylist))
			{
				Debug.Log("Switching to playlist " + p.name);
				SwitchToPlaylist(p);
			}

			if (Speaker.clip == null)
			{
				Debug.Log("Loading initial track");
				if (p != null && p.tracks.Count > 0)
				{
					PlayNextTrack(p);
				}
			}
			else if (!Speaker.isPlaying)
			{
				if (!Speaker.clip.isReadyToPlay)
				{
					Debug.Log("Loading...");
					_loading = true;
				}
				else
				{
					if (_loading)
					{
						_loading = false;
						Speaker.Play();
					}
					else
						PlayNextTrack(p);
				}
			}
			// Else loaded and playing.
		}

		public Playlist SelectPlaylist()
		{
			var validPlaylists = new List<Playlist>();
			foreach (Playlist p in Playlists)
			{
				if (p.enabled)
				{
					if (p.playWhen.PrerequisitesMet())
						validPlaylists.Add(p);
					/*else
						Debug.Log("Playlist " + p.name + " failed prereqs");*/
				}
				else
					Debug.Log("Playlist " + p.name + " is disabled");
			}

			// TODO: Select an appropriate playlist.
			// Merge all valid playlists?
			if (validPlaylists.Count > 0)
				return validPlaylists[0];
			else
			{
				Debug.Log("No valid playlists found!");
				return null;
			}
		}

		public void SwitchToPlaylist(Playlist p)
		{
			// TODO: If the new playlist contains the current track, continue playing it.
			Debug.Log("STED - Changing playlist to " +  p.name);
			CurrentPlaylist = p;
			p.trackIndex = 0;

			if (p.shuffle) p.Shuffle();

			if (p.tracks.Count > 0)
				PlayClip(p.tracks[p.trackIndex]);
			else
				Debug.Log("Playlist was empty: " + p.name);
		}

		public void PlayNextTrack()
		{
			PlayNextTrack(CurrentPlaylist);
		}

		public void PlayNextTrack(Playlist p)
		{
			Debug.Log("Playing next track");
			if (p == null)
			{
				Debug.LogError("PlayNextTrack: Playlist was null");
				return;
			}
			if (p.tracks.Count == 0)
			{
				// TODO: Play next playlist.
				Debug.Log("PlayNextTrack: No tracks found");
				return;
			}

			p.trackIndex++;
			if (p.trackIndex >= p.tracks.Count)
			{
				if (!p.loop)
				{
					// TODO: Play next playlist.
					Debug.Log("PlayNextTrack: All tracks played (" + p.trackIndex + " >= " + p.tracks.Count + ")");
					p.trackIndex--;
					return;
				}
				p.trackIndex = 0;
			}

			PlayClip(p.tracks[p.trackIndex]);
		}

		public void PlayClip(string clipName)
		{
			if (PreloadClip != null && PreloadClip.name.Equals(clipName))
				PlayClip(PreloadClip);
			else
			{
				AudioClip clip = GetAudioClip(clipName);
				if (clip == null)
				{
					Debug.LogError("PlayClip: Unabled to load clip " + clipName);
					return;
				}
				PlayClip(clip);
			}
		}

		public void PlayClip(AudioClip clip)
		{
			if (CurrentClip != null)
			{
				Debug.Log("PlayClip: Unloading previous clip");
				//AudioClip.DestroyImmediate(CurrentClip, true);
			}
			Debug.Log("[STED] Now Playing: " + clip.name);
			CurrentClip = clip;
			Speaker.clip = clip;

			if (CurrentPlaylist.fade.fadeIn > 0)
			{
				// TODO: Shorten the fade-in if the track is too short.
				_fadingIn = true;
				_fadeInStart = Time.realtimeSinceStartup;
				_fadeInEnd = _fadeInStart + CurrentPlaylist.fade.fadeIn;
				Speaker.volume = 0;
				Debug.Log("Begining fade in playlist");
			}
			else if (CurrentPlaylist.trackFade.fadeIn > 0) // Playlist fade has precedence over track fade.
			{
				_fadingIn = true;
				_fadeInEnd = Time.realtimeSinceStartup + CurrentPlaylist.trackFade.fadeIn;
				Speaker.volume = 0;
				Debug.Log("Begining fade in track");
			}

			if (CurrentPlaylist.fade.fadeOut > 0)
			{
				double time = (Speaker.clip.length - CurrentPlaylist.fade.fadeOut) * 1000;
				if (time > 0)
				{
					fadeOutTimer.Interval = time;
					_fadeOutEnd = CurrentPlaylist.fade.fadeOut;
					fadeOutTimer.Start();
				}
				else
					Debug.LogError("[STED] Invalid fadeOut playlist time: Clip " + Speaker.clip.name +
						" of length " + Speaker.clip.length + " was shorter than the fade out time of " + CurrentPlaylist.fade.fadeOut);
			}
			else if (CurrentPlaylist.trackFade.fadeOut > 0)
			{
				double time = (Speaker.clip.length - CurrentPlaylist.fade.fadeOut) * 1000;
				if (time > 0)
				{
					fadeOutTimer.Interval = time;
					_fadeOutEnd = CurrentPlaylist.trackFade.fadeOut;
					fadeOutTimer.Start();
				}
				else
					Debug.LogError("[STED] Invalid fadeOut clip time: Clip " + Speaker.clip.name +
						" of length " + Speaker.clip.length + " was shorter than the fade out time of " + CurrentPlaylist.fade.fadeOut);
			}

			if (CurrentPlaylist.preloadTime > 0)
			{
				if (CurrentPlaylist.trackIndex < CurrentPlaylist.tracks.Count - 1)
					preloadTimer.Interval = Math.Max((Speaker.clip.length - CurrentPlaylist.preloadTime) * 1000, 0);
			}

			Speaker.Play();
		}

		public void StopPlayback()
		{
			Speaker.Stop();

			fadeOutTimer.Stop();
		}

		public void preloadTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			preloadTimer.Stop();
			if (CurrentPlaylist.trackIndex + 1 < CurrentPlaylist.tracks.Count)
			{
				Debug.Log("Beginning preload");
				PreloadClip = GetAudioClip(CurrentPlaylist.tracks[CurrentPlaylist.trackIndex + 1]);
			}
		}

		private bool _fadingIn = false;
		private float _fadeInStart = 0;
		private float _fadeInEnd = 0;

		private bool _fadingOut = false;
		private float _fadeOutStart = 0;
		private float _fadeOutEnd = 0;

		public void fadeOutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			fadeOutTimer.Stop();
			_fadingOut = true;
			_fadeOutEnd += Time.realtimeSinceStartup;
			Debug.Log("Begining fade out");
		}

		private bool _wasFadingOut = false;
		private bool _wasFadingIn = false;
		public void DoFade()
		{
			float time = Time.realtimeSinceStartup;
			if (_fadingOut)
			{
				if (time >= _fadeOutEnd)
				{
					_fadingOut = false;
					Speaker.volume = GameSettings.MUSIC_VOLUME;
				}
				else
				{
					if (!_wasFadingOut)
						_fadeOutStart = Time.realtimeSinceStartup;

					Debug.Log("Fading out to " + GameSettings.MUSIC_VOLUME +
						" from " + _fadeOutStart + " to " + _fadeOutEnd + " at " + time + " = " + Mathf.InverseLerp(_fadeOutEnd, _fadeOutStart, time));
					Speaker.volume = Mathf.InverseLerp(_fadeOutEnd, _fadeOutStart, time) * GameSettings.MUSIC_VOLUME;
				}
				_wasFadingOut = true;
			}
			else
				_wasFadingOut = false;

			// TODO: Deal with simultaneous fading out and in.
			if (_fadingIn)
			{
				if (time >= _fadeInEnd)
				{
					_fadingIn = false;
					Speaker.volume = GameSettings.MUSIC_VOLUME;
				}
				else
				{
					if (!_wasFadingIn)
						_fadeInStart = Time.realtimeSinceStartup;

					Debug.Log("Fading in to " + GameSettings.MUSIC_VOLUME +
						" from " + _fadeInStart + " to " + _fadeInEnd + " at " + time + " = " + Mathf.InverseLerp(_fadeInStart, _fadeInEnd, time));
					Speaker.volume = Mathf.InverseLerp(_fadeInStart, _fadeInEnd, time) * GameSettings.MUSIC_VOLUME;
				}
				_wasFadingIn = true;
			}
			else
				_wasFadingIn = false;


			/*/ TODO: Preload
			if (CurrentPlaylist != null)
			{
				float preloadTime = CurrentPlaylist.preloadTime + CurrentPlaylist.trackFade.fadeOut;
			}*/
		}

		#region Enums
		public enum Selector
		{
			Either = -1,
			No = 0,
			Yes = 1
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

		[Flags]
		public enum Scene
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
			Any = 0xFFF
		}

		public static Scene ConvertScene(GameScenes scene)
		{
			switch (scene)
			{
				case GameScenes.LOADING:
					{
						return Scene.Loading;
					}
				case GameScenes.LOADINGBUFFER:
					{
						return Scene.LoadingBuffer;
					}
				case GameScenes.MAINMENU:
					{
						return Scene.MainMenu;
					}
				case GameScenes.SETTINGS:
					{
						return Scene.Settings;
					}
				case GameScenes.CREDITS:
					{
						return Scene.Credits;
					}
				case GameScenes.SPACECENTER:
					{
						return Scene.SpaceCentre;
					}
				case GameScenes.EDITOR:
					{
						return Scene.VAB;
					}
				case GameScenes.FLIGHT:
					{
						return Scene.Flight;
					}
				case GameScenes.TRACKSTATION:
					{
						return Scene.TrackingStation;
					}
				case GameScenes.SPH:
					{
						return Scene.SPH;
					}
				case GameScenes.PSYSTEM:
					{
						return Scene.PSystem;
					}
				default:
					{
						throw new ArgumentException("Soundtrack Editor: Invalid scene: " + scene +
							"\nCheck for an updated version of Soundtrack Editor.");
					}
			}
		}
		#endregion Enums
	} // End of class.


	/* Tasks:
	 * Create your own AudioSource, make sure it stays alive in all scenes.
	 * Method of loading a specific playlist using a set of prerequisite flags as a key.
	 * 
	 * Playlist object, defined by flags. e.g. planet=kerbin, situation:orbitHigh, sunRise
	 * Prerequisite
	 * 
	 * Sound areas:
	 *   Scenes - scene change
	 *   Bodies 
	 *     Surface
	 *     Atmosphere
	 *       Aerobraking
	 *       Flight
	 *     Orbit
	 *       Low
	 *       High
	 *     Flyby
	 *     Each biome
	 *   Targeted craft
	 *   
	 *   Vessel Situations
			LANDED = 0,
			SPLASHED = 1,
			PRELAUNCH = 2,
			FLYING = 3,
			SUB_ORBITAL = 4,
			ORBITING = 5,
			ESCAPING = 6,
			DOCKED = 7,
	 *   
	 * Ability to play track on first discovery only.
	 * Ability to queue a track to play immediately after the current one ends.
	 */
}
