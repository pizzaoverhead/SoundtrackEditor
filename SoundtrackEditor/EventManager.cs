using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SoundtrackEditor
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class EventManager : MonoBehaviour
    {
        // TODO: These need updated when a playlist is saved.
        public bool MonitorPause {
            get;
            set;
        }
        public bool MonitorInAtmosphere { get; set; }
        public bool MonitorTimeOfDay { get; set; }
        public bool MonitorScene { get; set; }
        public bool MonitorBody { get; set; }
        public bool MonitorCameraMode { get; set; }
        public bool MonitorSituation { get; set; }

        // Surface velocity
        public bool MonitorSurfaceVelocity { get; private set; }
        private float _maxSrfVel = float.MaxValue;
        public void AddMaxSurfaceVelocity(float maxSrfVel)
        {
            if (maxSrfVel < float.MaxValue)
                MonitorSurfaceVelocity = true;
            _maxSrfVel = Math.Min(_maxSrfVel, maxSrfVel);
        }
        private float _minSrfVel = float.MinValue;
        public void AddMinSurfaceVelocity(float minSrfVel)
        {
            if (minSrfVel > float.MinValue)
                MonitorSurfaceVelocity = true;
            _minSrfVel = Math.Max(_minSrfVel, minSrfVel);
        }

        // Orbital velocity
        public bool MonitorOrbitalVelocity { get; private set; }
        private float _maxObtVel = float.MaxValue;
        public void AddMaxOrbitalVelocity(float maxObtVel)
        {
            if (maxObtVel < float.MaxValue)
                MonitorOrbitalVelocity = true;
            _maxObtVel = Math.Min(_maxObtVel, maxObtVel);
        }
        private float _minObtVel = float.MinValue;
        public void AddMinOrbitalVelocity(float minObtVel)
        {
            if (minObtVel > float.MinValue)
                MonitorOrbitalVelocity = true;
            _minObtVel = Math.Max(_minObtVel, minObtVel);
        }

        // Altitude
        public bool MonitorAltitude { get; private set; }
        private float _maxAlt = float.MaxValue;
        public void AddMaxAltitude(float maxAlt)
        {
            if (maxAlt < float.MaxValue)
                MonitorAltitude = true;
            _maxAlt = Math.Min(_maxAlt, maxAlt);
        }
        private float _minAlt = float.MinValue;
        public void AddMinAltitude(float minAlt)
        {
            if (minAlt > float.MinValue)
                MonitorAltitude = true;
            _minAlt = Math.Max(_minAlt, minAlt);
        }

        // Nearest vessel
        public Vessel NearestVessel;
        public bool MonitorNearestVessel { get; private set; }
        private float _maxVesselDist = float.MaxValue;
        public void AddMaxVesselDistance(float maxVesselDist)
        {
            if (maxVesselDist < float.MaxValue)
                MonitorNearestVessel = true;
            _maxVesselDist = Math.Min(_maxVesselDist, maxVesselDist);
        }
        private float _minVesselDist = float.MinValue;
        public void AddMinVesselDistance(float minVesselDist)
        {
            if (minVesselDist > float.MinValue)
                MonitorNearestVessel = true;
            _minVesselDist = Math.Max(_minVesselDist, minVesselDist);
        }

        // State: Active, Inactive, Dead
        public bool MonitorVesselState { get; set; }

        public void TrackEventsForPlaylist(Playlist p)
        {
            if (p.pauseOnGamePause == true)
                MonitorPause = true;
            if (p.playWhen.inAtmosphere != Enums.Selector.Either)
                MonitorInAtmosphere = true;
            if (p.playWhen.timeOfDay != Enums.TimesOfDay.Any)
                MonitorTimeOfDay = true;
            if (p.playWhen.scene != Enums.Scenes.Any)
                MonitorScene = true;
            if (p.playWhen.situation != Enums.AnyVesselSituation)
                MonitorSituation = true;
            if (p.playWhen.cameraMode != Enums.CameraModes.Any)
                MonitorCameraMode = true;
            if (p.playWhen.bodyName.Length > 0)
                MonitorBody = true;
            if (p.playWhen.maxVelocitySurface != float.MaxValue)
                AddMaxSurfaceVelocity(p.playWhen.maxVelocitySurface);
            if (p.playWhen.minVelocitySurface != float.MinValue)
                AddMinSurfaceVelocity(p.playWhen.minVelocitySurface);
            if (p.playWhen.maxVelocityOrbital != float.MaxValue)
                AddMaxOrbitalVelocity(p.playWhen.maxVelocityOrbital);
            if (p.playWhen.minVelocityOrbital != float.MinValue)
                AddMinOrbitalVelocity(p.playWhen.minVelocityOrbital);
            if (p.playWhen.maxAltitude != float.MaxValue)
                AddMaxAltitude(p.playWhen.maxAltitude);
            if (p.playWhen.minAltitude != float.MinValue)
                AddMinAltitude(p.playWhen.minAltitude);
            if (p.playWhen.maxVesselDistance != float.MaxValue)
                AddMaxVesselDistance(p.playWhen.maxVesselDistance);
            if (p.playWhen.minVesselDistance != float.MinValue)
                AddMinVesselDistance(p.playWhen.minVesselDistance);
            if (p.playWhen.vesselState != Enums.VesselState.Any && p.playWhen.vesselState != 0)
                MonitorVesselState = true;
        }

        public static EventManager Instance { get; private set; }

        public EventManager()
        {
            Instance = this;
        }

        public void Start()
        {
            // Prevent the Monitor flags from being reset on first passing from loading to the title screen.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Monobehaviour Event
        /// </summary>
        public void Update()
        {
            UpdateSituation();
        }

        private CelestialBody _homeBody;
        private double _previousSrfVel = 0;
        private double _previousObtVel = 0;
        private double _previousAlt = 0;
        private Enums.VesselState _previousVesselState = 0;
        private void UpdateSituation()
        {
            bool changed = false;
            // Throws exceptions before the initial loading screen is completed.
            if (SoundtrackEditor.CurrentSituation.scene == Enums.Scenes.Flight)
            {
                if (IsFlightSituationChanged())
                    changed = true;
            }
            else if (SoundtrackEditor.CurrentSituation.scene != Enums.Scenes.SpaceCentre)
            {
                SoundtrackEditor.CurrentSituation.paused = Enums.Selector.False;
            }

            if (SoundtrackEditor.CurrentSituation.scene == Enums.Scenes.SpaceCentre ||
                SoundtrackEditor.CurrentSituation.scene == Enums.Scenes.Flight)
            {
                if (MonitorTimeOfDay)
                {
                    if (_homeBody == null)
                        _homeBody = FlightGlobals.GetHomeBody();
                    double localTime = Sun.Instance.GetLocalTimeAtPosition(Utils.KscLatitude, Utils.KscLongitude, _homeBody);
                    Enums.TimesOfDay tod = Enums.TimeToTimeOfDay(localTime);

                    if (SoundtrackEditor.CurrentSituation.timeOfDay != tod)
                    {
                        SoundtrackEditor.CurrentSituation.timeOfDay = tod;
                        changed = true;
                    }
                }
            }

            if (changed)
                SoundtrackEditor.Instance.OnSituationChanged();
        }

        private bool IsFlightSituationChanged()
        {
            bool changed = false;
            Vessel v = SoundtrackEditor.InitialLoadingComplete ? FlightGlobals.ActiveVessel : null;
            if (v != null)
            {
                Enums.Selector inAtmosphere = v.atmDensity > 0 ? Enums.Selector.True : Enums.Selector.False;
                if (SoundtrackEditor.CurrentSituation.inAtmosphere != inAtmosphere)
                {
                    SoundtrackEditor.CurrentSituation.inAtmosphere = inAtmosphere;
                    if (MonitorInAtmosphere)
                    {
                        Utils.Log("In atmosphere changed");
                        changed = true;
                    }
                }

                // For surface velocity, orbital velocity and altitude, check if we crossed the monitored point going in either direction.
                if (MonitorSurfaceVelocity)
                {
                    if ((v.srf_velocity.magnitude > _maxSrfVel && v.srf_velocity.magnitude < _previousSrfVel) ||
                        (v.srf_velocity.magnitude < _maxSrfVel && v.srf_velocity.magnitude > _previousSrfVel))
                    {
                        changed = true;
                    }
                    if ((v.srf_velocity.magnitude > _minSrfVel && v.srf_velocity.magnitude < _previousSrfVel) ||
                        (v.srf_velocity.magnitude < _minSrfVel && v.srf_velocity.magnitude > _previousSrfVel))
                    {
                        changed = true;
                    }
                    _previousSrfVel = v.srf_velocity.magnitude;
                }
                if (MonitorOrbitalVelocity)
                {
                    if ((v.obt_velocity.magnitude > _maxObtVel && v.obt_velocity.magnitude < _previousObtVel) ||
                        (v.obt_velocity.magnitude < _maxObtVel && v.obt_velocity.magnitude > _previousObtVel))
                    {
                        changed = true;
                    }
                    if ((v.obt_velocity.magnitude > _minObtVel && v.obt_velocity.magnitude < _previousObtVel) ||
                        (v.obt_velocity.magnitude < _minObtVel && v.obt_velocity.magnitude > _previousObtVel))
                    {
                        changed = true;
                    }
                    _previousObtVel = v.obt_velocity.magnitude;
                }

                if (MonitorAltitude)
                {
                    if ((v.altitude > _maxAlt && v.altitude < _previousAlt) ||
                        (v.altitude < _maxAlt && v.altitude > _previousAlt))
                    {
                        changed = true;
                    }
                    if ((v.altitude > _minAlt && v.altitude < _previousAlt) ||
                        (v.altitude < _minAlt && v.altitude > _previousAlt))
                    {
                        changed = true;
                    }
                    _previousAlt = v.altitude;
                }

                if (MonitorNearestVessel)
                {
                    Vessel newVessel = Utils.GetNearestVessel(_minVesselDist, _maxVesselDist, v);
                    if (newVessel != null && NearestVessel != newVessel)
                    {
                        NearestVessel = newVessel;
                        changed = true;
                    }
                }

                if (MonitorVesselState)
                {
                    if (_previousVesselState != Enums.ConvertVesselState(v.state))
                    {
                        Utils.Log("Vessel state changed");
                        _previousVesselState = Enums.ConvertVesselState(v.state);
                        changed = true;
                    }
                }
            }
            return changed;
        }
        
        internal void AddEvents()
        {
            // TODO: Check playlists and only add the events required.

            /*GameEvents.onActiveJointNeedUpdate.Add(onActiveJointNeedUpdate);
            onJointBreak
            GameEvents.OnAppFocus
            GameEvents.onAsteroidSpawned*/
            GameEvents.OnCameraChange.Add(OnCameraChange);
            /*GameEvents.onCollision.Add(onCollision);
            GameEvents.OnCollisionEnhancerHit
            GameEvents.OnCollisionIgnoreUpdate
            GameEvents.onCrash.Add(onCrash);
            GameEvents.onCrashSplashdown.Add(onCrashSplashdown);
            GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
            GameEvents.onCrewKilled.Add(onCrewKilled);
            GameEvents.OnCrewmemberHired
            GameEvents.OnCrewmemberLeftForDead
            GameEvents.OnCrewmemberSacked
            GameEvents.onCrewOnEva.Add(onCrewOnEva);
            GameEvents.onCrewTransferred*/
            GameEvents.onDominantBodyChange.Add(onDominantBodyChange);
            /*GameEvents.onEditorConstructionModeChange
            onEditorConstructionModeChange
            onEditorLoad
            onEditorPartDeleted
            onEditorPartEvent
            onEditorPartPicked
            onEditorPartPlaced
            onEditorPodDeleted
            onEditorPodSelected
            onEditorRedo
            onEditorRestart
            onEditorRestoreState
            onEditorScreenChange
            GameEvents.onEditorShipModified.Add(onEditorShipModified);
            onEditorShowPartList
            onEditorSnapModeChange
            onEditorStarted
            onEditorSymmetryCoordsChange
            onEditorSymmetryMethodChange
            onEditorSymmetryModeChange
            onEditorUndo
            OnExperimentDeployed
            GameEvents.onFlagPlant.Add(onFlagPlant);
            GameEvents.onFlagSelect.Add(onFlagSelect);
            OnFlightCameraAngleChange
            OnFlightCameraModeChange
            OnFlightGlobalsReady
            OnFlightLogRecorded
            GameEvents.onFlightReady.Add(onFlightReady);
            OnFlightUIModeChanged
            GameEvents.onFloatingOriginShift.Add(onFloatingOriginShift);
            GameEvents.OnFundsChanged.Add(OnFundsChanged);
            OnGameDatabaseLoaded*/
            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);
            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
            /*GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            GameEvents.onGameStateCreated.Add(onGameStateCreated);
            onGameStatePostLoad
            GameEvents.onGameStateLoad.Add(onGameStateLoad);
            GameEvents.onGameStateSave.Add(onGameStateSave);
            GameEvents.onGameStateSaved.Add(onGameStateSaved);*/
            GameEvents.onGameUnpause.Add(onGameUnpause);
            GameEvents.onGUIAdministrationFacilityDespawn.Add(onGUIAdministrationFacilityDespawn);
            GameEvents.onGUIAdministrationFacilitySpawn.Add(onGUIAdministrationFacilitySpawn);
            /*GameEvents.onGUIApplicationLauncherDestroyed.Add(onGUIApplicationLauncherDestroyed);
            GameEvents.onGUIApplicationLauncherReady.Add(onGUIApplicationLauncherReady);*/
            GameEvents.onGUIAstronautComplexDespawn.Add(onGUIAstronautComplexDespawn);
            GameEvents.onGUIAstronautComplexSpawn.Add(onGUIAstronautComplexSpawn);
			/*onGUIEditorToolbarReady
			onGUIEngineersReportDestroy
			onGUIEngineersReportReady*/
            //GameEvents.onGUIKSPediaDespawn.Add(onGUIKSPediaDespawn);
            //GameEvents.onGUIKSPediaSpawn.Add(onGUIKSPediaSpawn);
            /*GameEvents.onGUILaunchScreenDespawn.Add(onGUILaunchScreenDespawn);
            GameEvents.onGUILaunchScreenSpawn.Add(onGUILaunchScreenSpawn);
            GameEvents.onGUILaunchScreenVesselSelected.Add(onGUILaunchScreenVesselSelected);
			onGUILock
            GameEvents.onGUIMessageSystemReady.Add(onGUIMessageSystemReady);*/
            GameEvents.onGUIMissionControlDespawn.Add(onGUIMissionControlDespawn);
            GameEvents.onGUIMissionControlSpawn.Add(onGUIMissionControlSpawn);
            /*GameEvents.onGUIPrefabLauncherReady.Add(onGUIPrefabLauncherReady);
            GameEvents.onGUIRecoveryDialogDespawn.Add(onGUIRecoveryDialogDespawn);
            GameEvents.onGUIRecoveryDialogSpawn.Add(onGUIRecoveryDialogSpawn);*/
            GameEvents.onGUIRnDComplexDespawn.Add(onGUIRnDComplexDespawn);
            GameEvents.onGUIRnDComplexSpawn.Add(onGUIRnDComplexSpawn);
			/*onGUIUnlock
            GameEvents.onHideUI.Add(onHideUI);
            GameEvents.onInputLocksModified.Add(onInputLocksModified);
			OnIVACameraKerbalChange
            GameEvents.onJointBreak.Add(onJointBreak);
            GameEvents.onKerbalAdded.Add(onKerbalAdded);
            GameEvents.onKerbalRemoved.Add(onKerbalRemoved);
            GameEvents.onKerbalStatusChange.Add(onKerbalStatusChange);
            GameEvents.onKerbalTypeChange.Add(onKerbalTypeChange);
            GameEvents.onKnowledgeChanged.Add(onKnowledgeChanged);
            GameEvents.onKrakensbaneDisengage.Add(onKrakensbaneDisengage);
            GameEvents.onKrakensbaneEngage.Add(onKrakensbaneEngage);
            GameEvents.onLaunch.Add(onLaunch);
            GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
			onLevelWasLoadedGUIReady
			OnMapEntered
			OnMapExited
			OnMapViewFiltersModified
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
			OnPartLoaderLoaded
            GameEvents.onPartPack.Add(onPartPack);
            GameEvents.OnPartPurchased.Add(OnPartPurchased);
            GameEvents.onPartRemove.Add(onPartRemove);
			onPartResourceEmptyFull
			onPartResourceEmptyNonempty
			onPartResourceFlowModeChange
			onPartResourceFlowStateChange
			onPartResourceFullEmpty
			onPartResourceFullNonempty
			onPartResourceNonemptyEmpty
			onPartResourceNonemptyFull
            GameEvents.onPartUndock.Add(onPartUndock);
            GameEvents.onPartUnpack.Add(onPartUnpack);
            GameEvents.onPlanetariumTargetChanged.Add(onPlanetariumTargetChanged);
			OnPQSCityLoaded
			OnPQSCityUnloaded
            GameEvents.OnProgressAchieved.Add(OnProgressAchieved);
            GameEvents.OnProgressComplete.Add(OnProgressComplete);
			onProgressNodeLoad
			onProgressNodeSave
            GameEvents.OnProgressReached.Add(OnProgressReached);
			onProtoCrewMemberLoad
			onProtoCrewMemberSave
			onProtoPartModuleSnapshotLoad
			onProtoPartModuleSnapshotSave
			onProtoPartSnapshotLoad
			onProtoPartSnapshotSave
			onProtoVesselLoad
			onProtoVesselSave
            GameEvents.OnReputationChanged.Add(OnReputationChanged);
            GameEvents.onRotatingFrameTransition.Add(onRotatingFrameTransition);
			OnResourceConverterOutput
            GameEvents.onSameVesselDock.Add(onSameVesselDock);
            GameEvents.onSameVesselUndock.Add(onSameVesselUndock);
            GameEvents.OnScienceChanged.Add(OnScienceChanged);
            GameEvents.OnScienceRecieved.Add(OnScienceRecieved);
			onSetSpeedMode*/
            GameEvents.onShowUI.Add(onShowUI);
            /*GameEvents.onSplashDamage.Add(onSplashDamage);
            GameEvents.onStageActivate.Add(onStageActivate);
            GameEvents.onStageSeparation.Add(onStageSeparation);
            GameEvents.OnTechnologyResearched.Add(OnTechnologyResearched);
            GameEvents.onTimeWarpRateChanged.Add(onTimeWarpRateChanged);
			onTooltipDestroyRequested
			OnTriggeredDataTransmission
            GameEvents.onUndock.Add(onUndock);
			OnUpgradeableObjLevelChange
            GameEvents.onVesselChange.Add(onVesselChange);
			onVesselClearStaging
            GameEvents.onVesselCreate.Add(onVesselCreate);
            GameEvents.onVesselDestroy.Add(onVesselDestroy);
            GameEvents.onVesselGoOffRails.Add(onVesselGoOffRails);
            GameEvents.onVesselGoOnRails.Add(onVesselGoOnRails);
            GameEvents.onVesselLoaded.Add(onVesselLoaded);
            GameEvents.onVesselOrbitClosed.Add(onVesselOrbitClosed);
            GameEvents.onVesselOrbitEscaped.Add(onVesselOrbitEscaped);
            GameEvents.onVesselRecovered.Add(onVesselRecovered);
            GameEvents.onVesselRecoveryProcessing.Add(onVesselRecoveryProcessing);
            GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);
			onVesselReferenceTransformSwitch
            GameEvents.onVesselRename.Add(onVesselRename);
			onVesselResumeStaging
            GameEvents.OnVesselRollout.Add(OnVesselRollout);*/
            GameEvents.onVesselSituationChange.Add(onVesselSituationChange);
            /*GameEvents.onVesselSOIChanged.Add(onVesselSOIChanged);
			onVesselSwitching
            GameEvents.onVesselTerminated.Add(onVesselTerminated);
            GameEvents.onVesselWasModified.Add(onVesselWasModified);
            GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);
			onWheelRepaired

            // Contract
            GameEvents.Contract.onAccepted.Add(onAccepted);
            GameEvents.Contract.onCancelled.Add(onCancelled);
            GameEvents.Contract.onCompleted.Add(onCompleted);
            GameEvents.Contract.onContractsListChanged.Add(onContractsListChanged);
            GameEvents.Contract.onContractsLoaded.Add(onContractsLoaded);
            GameEvents.Contract.onDeclined.Add(onDeclined);
            GameEvents.Contract.onFailed.Add(onFailed);
            GameEvents.Contract.onFinished.Add(onFinished);
            GameEvents.Contract.onOffered.Add(onOffered);
            GameEvents.Contract.onParameterChange.Add(onParameterChange);

            // Vessel Situation
            GameEvents.VesselSituation.onEscape.Add(onEscape);
            GameEvents.VesselSituation.onFlyBy.Add(onFlyBy);
            GameEvents.VesselSituation.onLand.Add(onLand);
            GameEvents.VesselSituation.onOrbit.Add(onOrbit);
            GameEvents.VesselSituation.onReachSpace.Add(onReachSpace);
            GameEvents.VesselSituation.onReturnFromOrbit.Add(onReturnFromOrbit);
            GameEvents.VesselSituation.onReturnFromSurface.Add(onReturnFromSurface);*/
        }

        /*private void onActiveJointNeedUpdate(Vessel v) { Utils.Log("#onActiveJointNeedUpdate"); }*/
        private void OnCameraChange(CameraManager.CameraMode cameraMode)
        {
            Enums.CameraModes currentCameraMode = Enums.ConvertCameraMode(cameraMode);
            bool cameraChanged = SoundtrackEditor.CurrentSituation.cameraMode != currentCameraMode;
            SoundtrackEditor.CurrentSituation.cameraMode = currentCameraMode;
            if (cameraChanged && MonitorCameraMode)
            {
                SoundtrackEditor.Instance.OnSituationChanged();
            }
        }
        /*private void onCollision(EventReport e) { Utils.Log("#onActiveJointNeedUpdate"); }
        private void onCrash(EventReport e) { Utils.Log("#onCrash"); }
        private void onCrashSplashdown(EventReport e) { Utils.Log("#onCrashSplashdown"); }
        private void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> a) { Utils.Log("#onCrewBoardVessel"); }
        private void onCrewKilled(EventReport e) { Utils.Log("#onCrewKilled"); }
        private void onCrewOnEva(GameEvents.FromToAction<Part, Part> a) { Utils.Log("#onCrewOnEva"); }*/
        private void onDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> bodyChange)
        {
            //Utils.Log("#Body change from " + bodyChange.from + " to " + bodyChange.to);
            SoundtrackEditor.CurrentSituation.bodyName = bodyChange.to.name;
            if (MonitorBody)
                SoundtrackEditor.Instance.OnSituationChanged();
        }
        /*private void onEditorShipModified(ShipConstruct c) { Utils.Log("#onEditorShipModified"); }
        private void onFlagPlant(Vessel v) { Utils.Log("#onFlagPlant"); }
        private void onFlagSelect(string f) { Utils.Log("#onFlagSelect"); }
        private void onFlightReady() { Utils.Log("#onFlightReady"); }
        private void onFloatingOriginShift(Vector3d s) { Utils.Log("#onFloatingOriginShift"); }
        private void OnFundsChanged(double f, TransactionReasons t) { Utils.Log("#OnFundsChanged"); }*/

        private void onGamePause()
        {
            //Utils.Log("Pausing");
            SoundtrackEditor.CurrentSituation.paused = Enums.Selector.True;
            if (MonitorPause)
            {
                SoundtrackEditor.Instance.OnSituationChanged();
                SoundtrackEditor.Instance.OnGamePause();
            }
        }
        private void onGameUnpause()
        {
            //Utils.Log("Unpausing");
            SoundtrackEditor.CurrentSituation.paused = Enums.Selector.False;
            if (MonitorPause)
            {
                SoundtrackEditor.Instance.OnSituationChanged();
                SoundtrackEditor.Instance.OnGameUnpause();
            }
        }

        private void onGameSceneLoadRequested(GameScenes scene)
        {
            //Utils.Log("Loading scene: " + scene);
            if (!SoundtrackEditor.InitialLoadingComplete && scene.Equals(GameScenes.MAINMENU))
                SoundtrackEditor.InitialLoadingComplete = true;

            SoundtrackEditor.CurrentSituation.scene = Enums.ConvertScene(scene);
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }

        private void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> scenes)
        {
            //Utils.Log("Switching scene: " + scenes.to);
            if (!SoundtrackEditor.InitialLoadingComplete && scenes.to.Equals(GameScenes.MAINMENU))
                SoundtrackEditor.InitialLoadingComplete = true;

            SoundtrackEditor.CurrentSituation.scene = Enums.ConvertScene(scenes.to);
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }

        /*private void OnGameSettingsApplied() { Utils.Log("#OnGameSettingsApplied"); }
        private void onGameStateCreated(Game g) { Utils.Log("#onGameStateCreated"); }
        private void onGameStateLoad(ConfigNode n) { Utils.Log("#onGameStateLoad"); }
        private void onGameStateSave(ConfigNode n) { Utils.Log("#onGameStateSave"); }
        private void onGameStateSaved(Game g) { Utils.Log("#onGameStateSaved"); }*/
        
        private void onGUIAdministrationFacilityDespawn()
        {
            //Utils.Log("Leaving administration facility");
            SoundtrackEditor.CurrentSituation.scene = Enums.Scenes.SpaceCentre;
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }
        private void onGUIAdministrationFacilitySpawn()
        {
            //Utils.Log("Entering administration facility");
            SoundtrackEditor.CurrentSituation.scene = Enums.Scenes.AdminFacility;
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }
        /*private void onGUIApplicationLauncherDestroyed() { Utils.Log("#onGUIApplicationLauncherDestroyed"); }
        private void onGUIApplicationLauncherReady() { Utils.Log("#onGUIApplicationLauncherReady"); }*/
        private void onGUIAstronautComplexDespawn()
        {
            //Utils.Log("Leaving astronaut complex");
            SoundtrackEditor.CurrentSituation.scene = Enums.Scenes.SpaceCentre;
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }
        private void onGUIAstronautComplexSpawn()
        {
            //Utils.Log("Entering astronaut complex");
            SoundtrackEditor.CurrentSituation.scene = Enums.Scenes.AstronautComplex;
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }

        //private void onGUIKSPediaSpawn() { }
        //private void onGUIKSPediaDespawn() {}

        /*private void onGUILaunchScreenDespawn() { Utils.Log("#onGUILaunchScreenDespawn"); }
        private void onGUILaunchScreenSpawn(GameEvents.VesselSpawnInfo i) { Utils.Log("#onGUILaunchScreenSpawn, profile name: " + i.profileName); }
        private void onGUILaunchScreenVesselSelected(ShipTemplate t) { Utils.Log("#onGUILaunchScreenVesselSelected: " + t.shipName); }
        private void onGUIMessageSystemReady() { Utils.Log("#onGUIMessageSystemReady"); }*/
        private void onGUIMissionControlDespawn()
        {
            //Utils.Log("Leaving mission control");
            SoundtrackEditor.CurrentSituation.scene = Enums.Scenes.SpaceCentre;
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }
        private void onGUIMissionControlSpawn()
        {
            //Utils.Log("Entering mission control");
            SoundtrackEditor.CurrentSituation.scene = Enums.Scenes.MissionControl;
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }
        /*private void onGUIPrefabLauncherReady() { Utils.Log("#onGUIPrefabLauncherReady"); }
        private void onGUIRecoveryDialogDespawn(MissionRecoveryDialog d) { Utils.Log("#onGUIRecoveryDialogDespawn"); }
        private void onGUIRecoveryDialogSpawn(MissionRecoveryDialog d) { Utils.Log("#onGUIRecoveryDialogSpawn"); }*/
        private void onGUIRnDComplexDespawn()
        {
            //Utils.Log("Leaving RnD");
            SoundtrackEditor.CurrentSituation.scene = Enums.Scenes.SpaceCentre;
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }
        private void onGUIRnDComplexSpawn()
        {
            //Utils.Log("Entering RnD");
            SoundtrackEditor.CurrentSituation.scene = Enums.Scenes.RnDComplex;
            if (MonitorScene)
                SoundtrackEditor.Instance.OnSituationChanged();
        }
        private void onHideUI()
        {
            GuiManager.OnHideUI();
        }
        /*private void onInputLocksModified(GameEvents.FromToAction<ControlTypes, ControlTypes> a) { Utils.Log("#onInputLocksModified"); }
        private void onJointBreak(EventReport r) { Utils.Log("#onJointBreak"); }
        private void onKerbalAdded(ProtoCrewMember c) { Utils.Log("#onKerbalAdded"); }
        private void onKerbalRemoved(ProtoCrewMember c) { Utils.Log("#onKerbalRemoved"); }
        private void onKerbalStatusChange(ProtoCrewMember c, ProtoCrewMember.RosterStatus r1, ProtoCrewMember.RosterStatus r2) { Utils.Log("#onKerbalStatusChange"); }
        private void onKerbalTypeChange(ProtoCrewMember c, ProtoCrewMember.KerbalType t1, ProtoCrewMember.KerbalType t2) { Utils.Log("#onKerbalTypeChange"); }
        private void onKnowledgeChanged(GameEvents.HostedFromToAction<IDiscoverable, DiscoveryLevels> a) { Utils.Log("#onKnowledgeChanged"); }
        private void onKrakensbaneDisengage(Vector3d v) { Utils.Log("#onKrakensbaneDisengage"); }
        private void onKrakensbaneEngage(Vector3d v) { Utils.Log("#onKrakensbaneEngage"); }
        private void onLaunch(EventReport r) { Utils.Log("#onLaunch"); }
        // GUI: (old)
        private void onLevelWasLoaded(GameScenes s) {
            Utils.Log("#onLevelWasLoaded");
            Utils.Log("Adding Draw GUI");
            RenderingManager.AddToPostDrawQueue(3, new Callback(SoundtrackEditor.Instance.drawGUI));
        }private void onMissionFlagSelect(string f) { Utils.Log("#onMissionFlagSelect"); }
        private void onNewVesselCreated(Vessel v) { Utils.Log("#onNewVesselCreated"); }
        private void onOverheat(EventReport r) { Utils.Log("#onOverheat"); }
        private void onPartActionUICreate(Part p) { Utils.Log("#onPartActionUICreate"); }
        private void onPartActionUIDismiss(Part p) { Utils.Log("#onPartActionUIDismiss"); }
        private void onPartAttach(GameEvents.HostTargetAction<Part, Part> a) { Utils.Log("#onPartAttach"); }
        private void onPartCouple(GameEvents.FromToAction<Part, Part> a) { Utils.Log("#onPartCouple"); }
        //private void onPartDestroyed(Part p) { Utils.Log("#onPartDestroyed"); }
        private void onPartDie(Part p) { Utils.Log("#onPartDie"); }
        private void onPartExplode(GameEvents.ExplosionReaction e) { Utils.Log("#onPartExplode"); }
        private void onPartJointBreak(PartJoint j) { Utils.Log("#onPartJointBreak"); }
        private void onPartPack(Part p) { Utils.Log("#onPartPack"); }
        private void OnPartPurchased(AvailablePart p) { Utils.Log("#OnPartPurchased"); }
        private void onPartRemove(GameEvents.HostTargetAction<Part, Part> a) { Utils.Log("#onPartRemove"); }
        private void onPartUndock(Part p) { Utils.Log("#onPartUndock"); }
        private void onPartUnpack(Part p) { Utils.Log("#onPartUnpack"); }
        private void onPlanetariumTargetChanged(MapObject o) { Utils.Log("#onPlanetariumTargetChanged"); }
        private void OnProgressAchieved(ProgressNode n) { Utils.Log("#OnProgressAchieved: " + n.Id); }
        private void OnProgressComplete(ProgressNode n) { Utils.Log("#OnProgressComplete: " + n.Id); }
        private void OnProgressReached(ProgressNode n) { Utils.Log("#OnProgressReached: " + n.Id); }
        private void OnReputationChanged(float r, TransactionReasons t) { Utils.Log("#OnReputationChanged"); }
        private void onRotatingFrameTransition(GameEvents.HostTargetAction<CelestialBody, bool> a) { Utils.Log("#onRotatingFrameTransition"); }
        private void onSameVesselDock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> a) { Utils.Log("#onSameVesselDock"); }
        private void onSameVesselUndock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> a) { Utils.Log("#onSameVesselUndock"); }
        private void OnScienceChanged(float s, TransactionReasons t) { Utils.Log("#OnScienceChanged"); }
        private void OnScienceRecieved(float r, ScienceSubject s) { Utils.Log("#OnScienceRecieved"); }*/
        private void onShowUI() {
            GuiManager.OnShowUI();
        }
        /*private void onSplashDamage(EventReport r) { Utils.Log("#onSplashDamage"); }
        private void onStageActivate(int s) { Utils.Log("#onStageActivate"); }
        private void onStageSeparation(EventReport r) { Utils.Log("#onStageSeparation"); }
        private void OnTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> a) { Utils.Log("#OnTechnologyResearched"); }
        private void onTimeWarpRateChanged() { Utils.Log("#onTimeWarpRateChanged"); }
        private void onUndock(EventReport r) { Utils.Log("#onUndock"); }
        private void onVesselChange(Vessel v) { Utils.Log("#onVesselChange"); }
        private void onVesselCreate(Vessel v) { Utils.Log("#onVesselCreate"); }
        private void onVesselDestroy(Vessel v) { Utils.Log("#onVesselDestroy"); }
        private void onVesselGoOffRails(Vessel v) { Utils.Log("#onVesselGoOffRails"); }
        private void onVesselGoOnRails(Vessel v) { Utils.Log("#onVesselGoOnRails"); }
        private void onVesselLoaded(Vessel v) { Utils.Log("#onVesselLoaded"); }
        private void onVesselOrbitClosed(Vessel v) { Utils.Log("#onVesselOrbitClosed"); }
        private void onVesselOrbitEscaped(Vessel v) { Utils.Log("#onVesselOrbitEscaped"); }
        private void onVesselRecovered(ProtoVessel p) { Utils.Log("#onVesselRecovered"); }
        private void onVesselRecoveryProcessing(ProtoVessel p, MissionRecoveryDialog r, float x) { Utils.Log("#onVesselRecoveryProcessing"); }
        private void OnVesselRecoveryRequested(Vessel v) { Utils.Log("#OnVesselRecoveryRequested"); }
        private void onVesselRename(GameEvents.HostedFromToAction<Vessel, string> a) { Utils.Log("#onVesselRename"); }
        private void OnVesselRollout(ShipConstruct c) { Utils.Log("#OnVesselRollout"); }*/
        private void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> a)
        {
            //Utils.Log("# Vessel situation changed");
            // TODO: Verify operation with active and non-active vessels.
            SoundtrackEditor.CurrentSituation.situation = a.to;
            if (MonitorSituation)
                SoundtrackEditor.Instance.OnSituationChanged();
        }
        /*private void onVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> a) { Utils.Log("#onVesselSOIChanged"); }
        private void onVesselTerminated(ProtoVessel p) { Utils.Log("#onVesselTerminated"); }
        private void onVesselWasModified(Vessel v) { Utils.Log("#onVesselWasModified"); }
        private void onVesselWillDestroy(Vessel v) { Utils.Log("#onVesselWillDestroy"); }

        // Contract
        private void onAccepted(Contracts.Contract c) { Utils.Log("#onAccepted"); }
        private void onCancelled(Contracts.Contract c) { Utils.Log("#onCancelled"); }
        private void onCompleted(Contracts.Contract c) { Utils.Log("#onCompleted"); }
        private void onContractsListChanged() { Utils.Log("#onContractsListChanged"); }
        private void onContractsLoaded() { Utils.Log("#onContractsLoaded"); }
        private void onDeclined(Contracts.Contract c) { Utils.Log("#onDeclined"); }
        private void onFailed(Contracts.Contract c) { Utils.Log("#onFailed"); }
        private void onFinished(Contracts.Contract c) { Utils.Log("#onFinished"); }
        private void onOffered(Contracts.Contract c) { Utils.Log("#onOffered"); }
        private void onParameterChange(Contracts.Contract c, Contracts.ContractParameter p) { Utils.Log("#onParameterChange"); }

        // Vessel Situation
        private void onEscape(Vessel v, CelestialBody b) { Utils.Log("#onEscape"); }
        private void onFlyBy(Vessel v, CelestialBody b) { Utils.Log("#onFlyBy"); }
        private void onLand(Vessel v, CelestialBody b) { Utils.Log("#onLand"); }
        private void onOrbit(Vessel v, CelestialBody b) { Utils.Log("#onOrbit"); }
        private void onReachSpace(Vessel v) { Utils.Log("#onReachSpace"); }
        private void onReturnFromOrbit(Vessel v, CelestialBody b) { Utils.Log("#onReturnFromOrbit"); }
        private void onReturnFromSurface(Vessel v, CelestialBody b) { Utils.Log("#onReturnFromSurface"); }*/
    }
}
