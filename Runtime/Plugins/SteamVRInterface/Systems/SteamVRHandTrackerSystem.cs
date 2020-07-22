using Inter.Modules.ToolModule;
using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.Plugins.SteamVRInterface.Components;
using Inter.VR.Plugins.SteamVRInterface.Events;
using Inter.VR.Plugins.SteamVRInterface.Installer;
using Inter.VR.Installer;
using EcsRx.Collections.Database;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Plugins.ReactiveSystems.Systems;
using EcsRx.Plugins.Views.Components;
using EcsRx.Unity.Extensions;
using System;
using System.Linq;
using UniRx;
using Valve.VR;
using Inter.VR.Plugins.SteamVRInterface.Extensions;
using System.Collections.Generic;

namespace Inter.VR.Plugins.SteamVRInterface.Systems
{
    public class SteamVRHandTrackerSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRHandTracker), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly InterVRInstaller.Settings settings;
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;
        private readonly IGameObjectTool gameObjectTool;
        private readonly IInterVRInterface VRInterface;
        private readonly IEntityDatabase entityDatabase;
        private readonly IEventSystem eventSystem;

        public SteamVRHandTrackerSystem(InterVRInstaller.Settings settings,
            SteamVRInterfaceInstaller.Settings steamVRSettings,
            IGameObjectTool gameObjectTool,
            IInterVRInterface VRInterface,
            IEntityDatabase entityDatabase,
            IEventSystem eventSystem)
        {
            this.settings = settings;
            this.steamVRSettings = steamVRSettings;
            this.gameObjectTool = gameObjectTool;
            this.VRInterface = VRInterface;
            this.entityDatabase = entityDatabase;
            this.eventSystem = eventSystem;
        }

        bool isActivated(IEntity entity)
        {
            var vrHandTracker = entity.GetComponent<InterVRHandTracker>();
            return vrHandTracker.Active.Value;
        }

        public void Setup(IEntity entity)
        {
            var subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            Observable.EveryUpdate()
                .Where(x => VRInterface.SteamVRInitialized())
                .First()
                .Subscribe(x =>
                {
                    var vrHandTracker = entity.GetComponent<InterVRHandTracker>();
                    var view = entity.GetGameObject();
                    var trackedObject = view.AddComponent<SteamVR_Behaviour_Pose>();

                    trackedObject.onDeviceIndexChangedEvent = new SteamVR_Behaviour_Pose.DeviceIndexChangedHandler((fromAction, fromSource, newDeviceIndex) =>
                    {
                        if (fromSource == SteamVR_Input_Sources.LeftHand || fromSource == SteamVR_Input_Sources.RightHand)
                        {
                            // ensure activate gameObject for coroutine
                            var handEntity = VRInterface.GetHandEntity(vrHandTracker.Type);
                            var handView = handEntity.GetGameObject();
                            handView.SetActive(true);

                            //Debug.Log($"onDeviceIndexChangedEvent {fromAction} / {fromSource} / {newDeviceIndex}");

                            eventSystem.Publish(new InterVR_SteamVR_Behaviour_Pose_DeviceIndexChangedEvent()
                            {
                                Type = vrHandTracker.Type,
                                FromAction = fromAction,
                                FromSource = fromSource,
                                NewDeviceIndex = newDeviceIndex
                            });
                        }
                    });

                    trackedObject.onTransformUpdatedEvent = new SteamVR_Behaviour_Pose.UpdateHandler((fromAction, fromSource) =>
                    {
                        if (fromSource == SteamVR_Input_Sources.LeftHand || fromSource == SteamVR_Input_Sources.RightHand)
                        {
                            // ensure activate gameObject for coroutine
                            var handEntity = VRInterface.GetHandEntity(vrHandTracker.Type);
                            if (handEntity != null)
                            {
                                var handView = handEntity.GetGameObject();
                                if (handView != null)
                                    handView.SetActive(true);

                                //Debug.Log($"onTransformUpdatedEvent {fromAction} / {fromSource}");
                            }

                            eventSystem.Publish(new InterVR_SteamVR_Behaviour_Pose_TransformUpdatedEvent()
                            {
                                Type = vrHandTracker.Type,
                                FromAction = fromAction,
                                FromSource = fromSource
                            });
                        }
                    });

                    setupTrackerOrigin(trackedObject);
                    setupTrackerInputSource(entity, trackedObject);

                    entity.AddComponent(new SteamVRHandTracker()
                    {
                        TrackedObject = trackedObject
                    });

                    vrHandTracker.Active.Value = true;
                }).AddTo(subscriptions);

            Observable.EveryUpdate()
                .Where(x => isActivated(entity))
                .Subscribe(x =>
                {
                    var vrHandTracker = entity.GetComponent<InterVRHandTracker>();
                    var steamVRHandTracker = entity.GetComponent<SteamVRHandTracker>();
                    if (vrHandTracker.Registered.Value != steamVRHandTracker.TrackedObject.isActive)
                        vrHandTracker.Registered.Value = steamVRHandTracker.TrackedObject.isActive;
                    if (vrHandTracker.Valid.Value != steamVRHandTracker.TrackedObject.isValid)
                        vrHandTracker.Valid.Value = steamVRHandTracker.TrackedObject.isValid;
                }).AddTo(subscriptions);
        }

        public void Teardown(IEntity entity)
        {
            if (subscriptionsPerEntity.TryGetValue(entity, out List<IDisposable> subscriptions))
            {
                subscriptions.DisposeAll();
                subscriptions.Clear();
                subscriptionsPerEntity.Remove(entity);
            }
        }

        void setupTrackerOrigin(SteamVR_Behaviour_Pose trackedObject)
        {
            var rigEntity = VRInterface.GetRigEntity();
            if (rigEntity != null)
            {
                var rigView = rigEntity.GetGameObject();
                trackedObject.origin = rigView.transform;
            }
        }

        void setupTrackerInputSource(IEntity entity, SteamVR_Behaviour_Pose trackedObject)
        {
            var handTracker = entity.GetComponent<InterVRHandTracker>();
            if (handTracker.Type == InterVRHandType.Left)
            {
                trackedObject.inputSource = SteamVR_Input_Sources.LeftHand;
            }
            else
            {
                trackedObject.inputSource = SteamVR_Input_Sources.RightHand;
            }
        }
    }
}