using Inter.Modules.ToolModule;
using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Extensions;
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
using UnityEngine;
using Valve.VR;
using System.Collections.Generic;

namespace Inter.VR.Plugins.SteamVRInterface.Systems
{
    public class SteamVRHandRenderModelSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRHandRenderModel), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly InterVRInstaller.Settings settings;
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;
        private readonly IGameObjectTool gameObjectTool;
        private readonly IInterVRInterface VRInterface;
        private readonly IEntityDatabase entityDatabase;
        private readonly IEventSystem eventSystem;

        public SteamVRHandRenderModelSystem(InterVRInstaller.Settings settings,
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
            if (!entity.HasComponent<SteamVRHandRenderModel>())
                return false;

            var vrHandRenderModel = entity.GetComponent<InterVRHandRenderModel>();
            return vrHandRenderModel.Renderers != null;
        }

        public void Setup(IEntity entity)
        {
            var subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            var vrHandRenderModel = entity.GetComponent<InterVRHandRenderModel>();
            var handType = vrHandRenderModel.Type;

            eventSystem.Receive<InterVR_SteamVR_Behaviour_Pose_DeviceIndexChangedEvent>()
                .Subscribe(evt =>
                {
                    if (evt.Type == handType)
                    {
                        destroyRenderModel(entity);
                        initializeRenderModel(entity, evt.FromAction, evt.FromSource, evt.NewDeviceIndex);
                    }
                });

            Observable.EveryUpdate()
                .Where(x => isActivated(entity))
                .Subscribe(x =>
                {
                    if (vrHandRenderModel.IsVisibile())
                    {
                        var vrHandControllerRenderModelEntity = VRInterface.GetHandControllerRenderModelEntity(handType);
                        var vrHandControllerRenderModel = vrHandControllerRenderModelEntity.GetComponent<InterVRHandControllerRenderModel>();
                        if (vrHandControllerRenderModel.IsVisibile())
                        {
                            var steamVRHandRenderModel = entity.GetComponent<SteamVRHandRenderModel>();
                            if (steamVRHandRenderModel.Skeleton.rangeOfMotion == EVRSkeletalMotionRange.WithoutController)
                            {
                                steamVRHandRenderModel.Skeleton.SetRangeOfMotion(EVRSkeletalMotionRange.WithController);
                            }
                        }
                        else
                        {
                            var steamVRHandRenderModel = entity.GetComponent<SteamVRHandRenderModel>();
                            if (steamVRHandRenderModel.Skeleton.rangeOfMotion == EVRSkeletalMotionRange.WithController)
                            {
                                steamVRHandRenderModel.Skeleton.SetRangeOfMotion(EVRSkeletalMotionRange.WithoutController);
                            }
                        }
                    }

                }).AddTo(subscriptions);
        }

        public void Teardown(IEntity entity)
        {
            destroyRenderModel(entity);
            if (subscriptionsPerEntity.TryGetValue(entity, out List<IDisposable> subscriptions))
            {
                subscriptions.DisposeAll();
                subscriptions.Clear();
                subscriptionsPerEntity.Remove(entity);
            }
        }

        IDisposable rendererShadowOptionCastShadowHandle;
        IDisposable rendererShadowOptionReceiveShadowsHandle;

        void initializeRenderModel(IEntity entity, SteamVR_Behaviour_Pose trackedObject, SteamVR_Input_Sources inputSource, int newDeviceIndex)
        {
            if (entity.HasComponent<SteamVRHandRenderModel>())
                return;

            var vrHandRenderModel = entity.GetComponent<InterVRHandRenderModel>();
            var prefab = vrHandRenderModel.Type == InterVRHandType.Left ? steamVRSettings.LeftHandRendererModelPrefab : steamVRSettings.RightHandRendererModelPrefab;
            var parent = entity.GetGameObject().transform;
            var instance = gameObjectTool.InstantiateWithInit(prefab, parent);
            var skeleton = instance.GetComponent<SteamVR_Behaviour_Skeleton>();
            if (skeleton.skeletonAction.activeBinding == false && skeleton.fallbackPoser == null)
            {
                Debug.LogWarning("Skeleton action: " + skeleton.skeletonAction.GetPath() + " is not bound. Your controller may not support SteamVR Skeleton Input. " +
                    "Please add a fallback skeleton poser to your skeleton if you want hands to be visible");
                GameObject.Destroy(instance);
                return;
            }

            var rigEntity = VRInterface.GetRigEntity();
            var rig = rigEntity.GetComponent<InterVRRig>();
            skeleton.origin = rig.TrackingOriginTransform;
            skeleton.updatePose = false;

            vrHandRenderModel.Renderers = instance.GetComponentsInChildren<Renderer>();
            if (vrHandRenderModel.Renderers != null)
            {
                updateRendererShadowOption(vrHandRenderModel.Renderers, vrHandRenderModel.CastShadow.Value, vrHandRenderModel.ReceiveShadows.Value);
                vrHandRenderModel.SetVisibility(vrHandRenderModel.DisplayByDefault);
            }

            rendererShadowOptionCastShadowHandle = vrHandRenderModel.CastShadow.DistinctUntilChanged().Subscribe(x =>
            {
                updateRendererShadowOption(vrHandRenderModel.Renderers, vrHandRenderModel.CastShadow.Value, vrHandRenderModel.ReceiveShadows.Value);
            });
            rendererShadowOptionReceiveShadowsHandle = vrHandRenderModel.ReceiveShadows.DistinctUntilChanged().Subscribe(x =>
            {
                updateRendererShadowOption(vrHandRenderModel.Renderers, vrHandRenderModel.CastShadow.Value, vrHandRenderModel.ReceiveShadows.Value);
            });

            entity.AddComponent(new SteamVRHandRenderModel()
            {
                Instance = instance,
                Skeleton = skeleton,
                Animator = instance.GetComponentInChildren<Animator>(),
            });
        }

        void destroyRenderModel(IEntity entity)
        {
            if (!entity.HasComponent<SteamVRHandRenderModel>())
                return;

            if (rendererShadowOptionCastShadowHandle != null)
            {
                rendererShadowOptionCastShadowHandle.Dispose();
                rendererShadowOptionCastShadowHandle = null;
            }
            if (rendererShadowOptionReceiveShadowsHandle != null)
            {
                rendererShadowOptionReceiveShadowsHandle.Dispose();
                rendererShadowOptionReceiveShadowsHandle = null;
            }

            var steamVRHandRenderModel = entity.GetComponent<SteamVRHandRenderModel>();
            GameObject.Destroy(steamVRHandRenderModel.Instance);
            entity.RemoveComponent<SteamVRHandRenderModel>();

            var vrHandRenderModel = entity.GetComponent<InterVRHandRenderModel>();
            vrHandRenderModel.Renderers = null;
        }

        void updateRendererShadowOption(Renderer[] renderers, bool shadowCastingMode, bool receiveShadows)
        {
            if (renderers == null)
                return;

            foreach (var renderer in renderers)
            {
                renderer.shadowCastingMode = shadowCastingMode ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = receiveShadows;
            }
        }
    }
}