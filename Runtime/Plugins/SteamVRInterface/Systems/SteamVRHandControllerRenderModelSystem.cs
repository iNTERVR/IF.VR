using Inter.Modules.ToolModule;
using Inter.VR.Components;
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
using Inter.VR.Defines;
using System.Collections.Generic;

namespace Inter.VR.Plugins.SteamVRInterface.Systems
{
    public class SteamVRHandControllerRenderModelSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRHandControllerRenderModel), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly InterVRInstaller.Settings settings;
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;
        private readonly IGameObjectTool gameObjectTool;
        private readonly IInterVRInterface VRInterface;
        private readonly IEntityDatabase entityDatabase;
        private readonly IEventSystem eventSystem;

        public SteamVRHandControllerRenderModelSystem(InterVRInstaller.Settings settings,
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
            if (!entity.HasComponent<SteamVRHandControllerRenderModel>())
                return false;

            var vrHandControllerRenderModel = entity.GetComponent<InterVRHandControllerRenderModel>();
            return vrHandControllerRenderModel.Renderers != null;
        }

        public void Setup(IEntity entity)
        {
            var subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            var vrHandControllerRenderModel = entity.GetComponent<InterVRHandControllerRenderModel>();
            var handType = vrHandControllerRenderModel.Type;

            eventSystem.Receive<InterVR_SteamVR_Behaviour_Pose_DeviceIndexChangedEvent>()
                .Subscribe(evt =>
                {
                    if (evt.Type == handType)
                    {
                        destroyRenderModel(entity);
                        initializeRenderModel(entity, evt.NewDeviceIndex);
                    }
                });

            Observable.EveryUpdate()
                .Where(x => isActivated(entity))
                .Subscribe(x =>
                {
                }).AddTo(subscriptions);

            SteamVR_Events.RenderModelLoaded.AsObservable().Subscribe(evt =>
            {
                initializeRenderModel(entity);

                var steamVRControllerRenderModel = entity.GetComponent<SteamVRHandControllerRenderModel>();
                if (steamVRControllerRenderModel.RenderModel == evt.Item1)
                {
                    vrHandControllerRenderModel.Renderers = steamVRControllerRenderModel.Instance.GetComponentsInChildren<Renderer>();
                    if (vrHandControllerRenderModel.Renderers != null)
                    {
                        updateRendererShadowOption(vrHandControllerRenderModel.Renderers, vrHandControllerRenderModel.CastShadow.Value, vrHandControllerRenderModel.ReceiveShadows.Value);

                        vrHandControllerRenderModel.SetVisibility(vrHandControllerRenderModel.DisplayByDefault);
                        if (vrHandControllerRenderModel.DelayedSetMaterial != null)
                        {
                            vrHandControllerRenderModel.SetMaterial(vrHandControllerRenderModel.DelayedSetMaterial);
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

        void initializeRenderModel(IEntity entity, int newDeviceIndex = -1)
        {
            if (entity.HasComponent<SteamVRHandControllerRenderModel>())
            {
                if (newDeviceIndex != -1)
                {
                    var steamVRHandControllerRenderModel = entity.GetComponent<SteamVRHandControllerRenderModel>();
                    steamVRHandControllerRenderModel.RenderModel.SetDeviceIndex(newDeviceIndex);
                }
                return;
            }

            var vrHandControllerRenderModel = entity.GetComponent<InterVRHandControllerRenderModel>();

            var prefab = steamVRSettings.ControllerRendererModelPrefab;
            var parent = entity.GetGameObject().transform;
            var instance = gameObjectTool.InstantiateWithInit(prefab, parent);
            var renderModel = instance.GetComponent<SteamVR_RenderModel>();

            renderModel.SetInputSource(vrHandControllerRenderModel.Type == InterVRHandType.Left ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
            if (newDeviceIndex != -1)
                renderModel.SetDeviceIndex(newDeviceIndex);

            rendererShadowOptionCastShadowHandle = vrHandControllerRenderModel.CastShadow.DistinctUntilChanged().Subscribe(x =>
            {
                updateRendererShadowOption(vrHandControllerRenderModel.Renderers, vrHandControllerRenderModel.CastShadow.Value, vrHandControllerRenderModel.ReceiveShadows.Value);
            });
            rendererShadowOptionReceiveShadowsHandle = vrHandControllerRenderModel.ReceiveShadows.DistinctUntilChanged().Subscribe(x =>
            {
                updateRendererShadowOption(vrHandControllerRenderModel.Renderers, vrHandControllerRenderModel.CastShadow.Value, vrHandControllerRenderModel.ReceiveShadows.Value);
            });

            entity.AddComponent(new SteamVRHandControllerRenderModel()
            {
                Instance = instance,
                RenderModel = renderModel
            });
        }

        void destroyRenderModel(IEntity entity)
        {
            if (!entity.HasComponent<SteamVRHandControllerRenderModel>())
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

            var steamVRControllerRenderModel = entity.GetComponent<SteamVRHandControllerRenderModel>();
            GameObject.Destroy(steamVRControllerRenderModel.Instance);
            entity.RemoveComponent<SteamVRHandControllerRenderModel>();

            var vrHandControllerRenderModel = entity.GetComponent<InterVRHandControllerRenderModel>();
            vrHandControllerRenderModel.Renderers = null;
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