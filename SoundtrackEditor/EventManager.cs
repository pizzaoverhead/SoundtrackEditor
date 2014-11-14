using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundtrackEditor
{
    class EventManager
    {
        internal void AddEvents()
        {
            // TODO: Check playlists and only add the events required.

            GameEvents.onActiveJointNeedUpdate.Add(onActiveJointNeedUpdate);
            GameEvents.onCollision.Add(onCollision);
            GameEvents.onCrash.Add(onCrash);
            GameEvents.onCrashSplashdown.Add(onCrashSplashdown);
            GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
            GameEvents.onCrewKilled.Add(onCrewKilled);
            GameEvents.onCrewOnEva.Add(onCrewOnEva);
            GameEvents.onDominantBodyChange.Add(onDominantBodyChange);
            GameEvents.onEditorShipModified.Add(onEditorShipModified);
            GameEvents.onFlagPlant.Add(onFlagPlant);
            GameEvents.onFlagSelect.Add(onFlagSelect);
            GameEvents.onFlightReady.Add(onFlightReady);
            GameEvents.onFloatingOriginShift.Add(onFloatingOriginShift);
            GameEvents.OnFundsChanged.Add(OnFundsChanged);
            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            GameEvents.onGameStateCreated.Add(onGameStateCreated);
            GameEvents.onGameStateLoad.Add(onGameStateLoad);
            GameEvents.onGameStateSave.Add(onGameStateSave);
            GameEvents.onGameStateSaved.Add(onGameStateSaved);
            GameEvents.onGameUnpause.Add(onGameUnpause);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(onGUIApplicationLauncherDestroyed);
            GameEvents.onGUIApplicationLauncherReady.Add(onGUIApplicationLauncherReady);
            GameEvents.onGUIAstronautComplexDespawn.Add(onGUIAstronautComplexDespawn);
            GameEvents.onGUIAstronautComplexSpawn.Add(onGUIAstronautComplexSpawn);
            GameEvents.onGUILaunchScreenDespawn.Add(onGUILaunchScreenDespawn);
            GameEvents.onGUILaunchScreenSpawn.Add(onGUILaunchScreenSpawn);
            GameEvents.onGUILaunchScreenVesselSelected.Add(onGUILaunchScreenVesselSelected);
            GameEvents.onGUIMessageSystemReady.Add(onGUIMessageSystemReady);
            GameEvents.onGUIMissionControlDespawn.Add(onGUIMissionControlDespawn);
            GameEvents.onGUIMissionControlSpawn.Add(onGUIMissionControlSpawn);
            GameEvents.onGUIPrefabLauncherReady.Add(onGUIPrefabLauncherReady);
            GameEvents.onGUIRecoveryDialogDespawn.Add(onGUIRecoveryDialogDespawn);
            GameEvents.onGUIRecoveryDialogSpawn.Add(onGUIRecoveryDialogSpawn);
            GameEvents.onGUIRnDComplexDespawn.Add(onGUIRnDComplexDespawn);
            GameEvents.onGUIRnDComplexSpawn.Add(onGUIRnDComplexSpawn);
            GameEvents.onHideUI.Add(onHideUI);
            GameEvents.onInputLocksModified.Add(onInputLocksModified);
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
            GameEvents.OnProgressAchieved.Add(OnProgressAchieved);
            GameEvents.OnProgressComplete.Add(OnProgressComplete);
            GameEvents.OnProgressReached.Add(OnProgressReached);
            GameEvents.OnReputationChanged.Add(OnReputationChanged);
            GameEvents.onRotatingFrameTransition.Add(onRotatingFrameTransition);
            GameEvents.onSameVesselDock.Add(onSameVesselDock);
            GameEvents.onSameVesselUndock.Add(onSameVesselUndock);
            GameEvents.OnScienceChanged.Add(OnScienceChanged);
            GameEvents.OnScienceRecieved.Add(OnScienceRecieved);
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
            GameEvents.onVesselRecoveryProcessing.Add(onVesselRecoveryProcessing);
            GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);
            GameEvents.onVesselRename.Add(onVesselRename);
            GameEvents.OnVesselRollout.Add(OnVesselRollout);
            GameEvents.onVesselSituationChange.Add(onVesselSituationChange);
            GameEvents.onVesselSOIChanged.Add(onVesselSOIChanged);
            GameEvents.onVesselTerminated.Add(onVesselTerminated);
            GameEvents.onVesselWasModified.Add(onVesselWasModified);
            GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);

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
            GameEvents.VesselSituation.onReturnFromSurface.Add(onReturnFromSurface);
        }

        private void onActiveJointNeedUpdate(Vessel v) { Utils.Log("#onActiveJointNeedUpdate"); }
        private void onCollision(EventReport e) { Utils.Log("#onActiveJointNeedUpdate"); }
        private void onCrash(EventReport e) { Utils.Log("#onCrash"); }
        private void onCrashSplashdown(EventReport e) { Utils.Log("#onCrashSplashdown"); }
        private void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> a) { Utils.Log("#onCrewBoardVessel"); }
        private void onCrewKilled(EventReport e) { Utils.Log("#onCrewKilled"); }
        private void onCrewOnEva(GameEvents.FromToAction<Part, Part> a) { Utils.Log("#onCrewOnEva"); }
        private void onDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> bodyChange)
        {
            Utils.Log("#Body change from " + bodyChange.from + " to " + bodyChange.to);
            SoundtrackEditor.CurrentSituation.bodyName = bodyChange.to.name;
            SoundtrackEditor.Instance.OnSituationChanged();
        }
        private void onEditorShipModified(ShipConstruct c) { Utils.Log("#onEditorShipModified"); }
        private void onFlagPlant(Vessel v) { Utils.Log("#onFlagPlant"); }
        private void onFlagSelect(string f) { Utils.Log("#onFlagSelect"); }
        private void onFlightReady() { Utils.Log("#onFlightReady"); }
        private void onFloatingOriginShift(Vector3d s) { Utils.Log("#onFloatingOriginShift"); }
        private void OnFundsChanged(double f, TransactionReasons t) { Utils.Log("#OnFundsChanged"); }
        private void onGamePause() { Utils.Log("#onGamePause"); }

        private void onGameSceneLoadRequested(GameScenes scene)
        {
            Utils.Log("Changing scene: " + scene);
            if (!SoundtrackEditor.InitialLoadingComplete && scene.Equals(GameScenes.MAINMENU))
                SoundtrackEditor.InitialLoadingComplete = true;

            SoundtrackEditor.CurrentSituation.scene = Enums.ConvertScene(scene);
            SoundtrackEditor.Instance.OnSituationChanged();
        }

        private void OnGameSettingsApplied() { Utils.Log("#OnGameSettingsApplied"); }
        private void onGameStateCreated(Game g) { Utils.Log("#onGameStateCreated"); }
        private void onGameStateLoad(ConfigNode n) { Utils.Log("#onGameStateLoad"); }
        private void onGameStateSave(ConfigNode n) { Utils.Log("#onGameStateSave"); }
        private void onGameStateSaved(Game g) { Utils.Log("#onGameStateSaved"); }
        private void onGameUnpause() { Utils.Log("#onGameUnpause"); }
        private void onGUIApplicationLauncherDestroyed() { Utils.Log("#onGUIApplicationLauncherDestroyed"); }
        private void onGUIApplicationLauncherReady() { Utils.Log("#onGUIApplicationLauncherReady"); }
        private void onGUIAstronautComplexDespawn() { Utils.Log("#onGUIAstronautComplexDespawn"); }
        private void onGUIAstronautComplexSpawn() { Utils.Log("#onGUIAstronautComplexSpawn"); }
        private void onGUILaunchScreenDespawn() { Utils.Log("#onGUILaunchScreenDespawn"); }
        private void onGUILaunchScreenSpawn(GameEvents.VesselSpawnInfo i) { Utils.Log("#onGUILaunchScreenSpawn, profile name: " + i.profileName); }
        private void onGUILaunchScreenVesselSelected(ShipTemplate t) { Utils.Log("#onGUILaunchScreenVesselSelected: " + t.shipName); }
        private void onGUIMessageSystemReady() { Utils.Log("#onGUIMessageSystemReady"); }
        private void onGUIMissionControlDespawn() { Utils.Log("#onGUIMissionControlDespawn"); }
        private void onGUIMissionControlSpawn() { Utils.Log("#onGUIMissionControlSpawn"); }
        private void onGUIPrefabLauncherReady() { Utils.Log("#onGUIPrefabLauncherReady"); }
        private void onGUIRecoveryDialogDespawn(MissionRecoveryDialog d) { Utils.Log("#onGUIRecoveryDialogDespawn"); }
        private void onGUIRecoveryDialogSpawn(MissionRecoveryDialog d) { Utils.Log("#onGUIRecoveryDialogSpawn"); }
        private void onGUIRnDComplexDespawn() { Utils.Log("#onGUIRnDComplexDespawn"); }
        private void onGUIRnDComplexSpawn() { Utils.Log("#onGUIRnDComplexSpawn"); }
        private void onHideUI() { Utils.Log("#onHideUI"); }
        private void onInputLocksModified(GameEvents.FromToAction<ControlTypes, ControlTypes> a) { Utils.Log("#onInputLocksModified"); }
        private void onJointBreak(EventReport r) { Utils.Log("#onJointBreak"); }
        private void onKerbalAdded(ProtoCrewMember c) { Utils.Log("#onKerbalAdded"); }
        private void onKerbalRemoved(ProtoCrewMember c) { Utils.Log("#onKerbalRemoved"); }
        private void onKerbalStatusChange(ProtoCrewMember c, ProtoCrewMember.RosterStatus r1, ProtoCrewMember.RosterStatus r2) { Utils.Log("#onKerbalStatusChange"); }
        private void onKerbalTypeChange(ProtoCrewMember c, ProtoCrewMember.KerbalType t1, ProtoCrewMember.KerbalType t2) { Utils.Log("#onKerbalTypeChange"); }
        private void onKnowledgeChanged(GameEvents.HostedFromToAction<IDiscoverable, DiscoveryLevels> a) { Utils.Log("#onKnowledgeChanged"); }
        private void onKrakensbaneDisengage(Vector3d v) { Utils.Log("#onKrakensbaneDisengage"); }
        private void onKrakensbaneEngage(Vector3d v) { Utils.Log("#onKrakensbaneEngage"); }
        private void onLaunch(EventReport r) { Utils.Log("#onLaunch"); }
        private void onLevelWasLoaded(GameScenes s) {
            Utils.Log("#onLevelWasLoaded");

            Utils.Log("Adding Draw GUI");
            RenderingManager.AddToPostDrawQueue(3, new Callback(SoundtrackEditor.Instance.drawGUI));
        }
        private void onMissionFlagSelect(string f) { Utils.Log("#onMissionFlagSelect"); }
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
        private void OnScienceRecieved(float r, ScienceSubject s) { Utils.Log("#OnScienceRecieved"); }
        private void onShowUI() { Utils.Log("#onShowUI"); }
        private void onSplashDamage(EventReport r) { Utils.Log("#onSplashDamage"); }
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
        private void OnVesselRollout(ShipConstruct c) { Utils.Log("#OnVesselRollout"); }
        private void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> a)
        {
            Utils.Log("# Vessel situation changed");
            // TODO: Verify operation with active and non-active vessels.
            SoundtrackEditor.CurrentSituation.situation = a.to;
            SoundtrackEditor.Instance.OnSituationChanged();
        }
        private void onVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> a) { Utils.Log("#onVesselSOIChanged"); }
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
        private void onReturnFromSurface(Vessel v, CelestialBody b) { Utils.Log("#onReturnFromSurface"); }
    }
}
