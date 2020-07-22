using Inter.Blueprints;
using Inter.Components;
using Inter.Modules.ToolModule;
using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.Installer;
using EcsRx.Collections.Database;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Plugins.ReactiveSystems.Systems;
using EcsRx.Plugins.Views.Components;
using EcsRx.Unity.Extensions;
using ManusVR.SDK.Apollo;
using ManusVR.Hands;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using Valve.VR;
using System.Collections.Generic;
using ManusVR.SDK.Manus;
using Inter.VR.Extensions;
using Inter.Defines;
using Inter.VR.Plugins.SteamVRInterface.Installer;
using Inter.VR.Plugins.ManusVRInterface.Extensions;

namespace Inter.VR.Plugins.ManusVRInterface.Systems
{
    public class ManusVRGloveSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRGlove), typeof(ViewComponent));

        private List<IDisposable> subscriptions = new List<IDisposable>();
        private readonly InterVRInstaller.Settings settings;
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;
        private readonly IGameObjectTool gameObjectTool;
        private readonly IInterVRInterface vrInterface;
        private readonly IInterVRGloveInterface vrGloveInterface;
        private readonly IEntityDatabase entityDatabase;
        private readonly IEventSystem eventSystem;

        public ManusVRGloveSystem(InterVRInstaller.Settings settings,
            SteamVRInterfaceInstaller.Settings steamVRSettings,
            IGameObjectTool gameObjectTool,
            IInterVRInterface vrInterface,
            IInterVRGloveInterface vrGloveInterface,
            IEntityDatabase entityDatabase,
            IEventSystem eventSystem)
        {
            this.settings = settings;
            this.steamVRSettings = steamVRSettings;
            this.gameObjectTool = gameObjectTool;
            this.vrInterface = vrInterface;
            this.vrGloveInterface = vrGloveInterface;
            this.entityDatabase = entityDatabase;
            this.eventSystem = eventSystem;
        }

        // check validate
        bool vrHandTrackerIsValid(IEntity entity)
        {
            var vrGlove = entity.GetComponent<InterVRGlove>();
            var vrHandTrackerEntity = vrInterface.GetHandTrackerEntity(vrGlove.Type);
            if (vrHandTrackerEntity == null)
                return false;
            var vrHandTracker = vrHandTrackerEntity.GetComponent<InterVRHandTracker>();
            return vrHandTracker.IsValid();
        }

        // to change follow target
        bool vrHandIsActivated(IEntity entity)
        {
            var vrGlove = entity.GetComponent<InterVRGlove>();
            var vrHandEntity = vrInterface.GetHandEntity(vrGlove.Type);
            var vrHand = vrHandEntity.GetComponent<InterVRHand>();
            return vrHand.IsActive();
        }

        bool vrHandRenderModelsAreInitialized(IEntity entity)
        {
            var vrGlove = entity.GetComponent<InterVRGlove>();
            var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrGlove.Type);
            if (vrHandRenderModelEntity == null)
                return false;

            var vrHandRenderModel = vrHandRenderModelEntity.GetComponent<InterVRHandRenderModel>();
            if (vrHandRenderModel.Renderers == null)
                return false;

            var vrHandControllerRenderModelEntity = vrInterface.GetHandControllerRenderModelEntity(vrGlove.Type);
            if (vrHandControllerRenderModelEntity == null)
                return false;

            var vrHandControllerRenderModel = vrHandControllerRenderModelEntity.GetComponent<InterVRHandControllerRenderModel>();
            if (vrHandControllerRenderModel.Renderers == null)
                return false;

            return true;
        }

        bool manusIsConnected(IEntity entity)
        {
            var vrGlove = entity.GetComponent<InterVRGlove>();
            HandDataManager.EnsureLoaded();
            var deviceType = vrGlove.Type == InterVRHandType.Left ? device_type_t.GLOVE_LEFT : device_type_t.GLOVE_RIGHT;
            vrGlove.Connected = Manus.ManusIsConnected(HandDataManager.ManusSession, deviceType);
            return vrGlove.Connected;
        }

        bool vrGloveIsActivated(IEntity entity)
        {
            var vrGlove = entity.GetComponent<InterVRGlove>();
            return vrGlove.IsActive();
        }

        public void Setup(IEntity entity)
        {
            Observable.EveryUpdate()
                .Where(x => vrHandTrackerIsValid(entity) &&
                    vrHandIsActivated(entity) &&
                    vrHandRenderModelsAreInitialized(entity) &&
                    manusIsConnected(entity) &&
                    vrInterface.ManusVRInitialized())
                .First()
                .Subscribe(x =>
                {
                    var vrGlove = entity.GetComponent<InterVRGlove>();
                    var gloveType = vrGlove.Type;
                    var vrGloveView = entity.GetGameObject();

                    // create wrist and setup
                    var pool = entityDatabase.GetCollection();
                    var wristEntity = pool.CreateEntity();
                    vrGlove.Wrist.gameObject.LinkEntity(wristEntity, pool);
                    var wristView = wristEntity.GetGameObject();
                    var wrist = wristEntity.AddComponent<InterVRGloveWrist>();
                    wrist.Type = gloveType;
                    vrGlove.WristEntity = wristEntity;

                    // instaniate render model prefab
                    var prefab = gloveType == InterVRHandType.Left ? steamVRSettings.LeftHandRendererModelPrefab : steamVRSettings.RightHandRendererModelPrefab;
                    var parent = wristView.transform;
                    var instance = gameObjectTool.InstantiateWithInit(prefab, parent);
                    var skeleton = instance.GetComponentInChildren<SteamVR_Behaviour_Skeleton>();
                    var manusRigger = vrGloveView.AddComponent<ManusRigger>();
                    var handRig = gloveType == InterVRHandType.Left ? manusRigger.LeftHand = new HandRig() : manusRigger.RightHand = new HandRig();

                    handRig.WristTransform = vrGlove.Wrist;
                    handRig.Thumb.Proximal = skeleton.thumbProximal;
                    handRig.Thumb.Intermedial = skeleton.thumbMiddle;
                    handRig.Thumb.Distal = skeleton.thumbDistal;
                    handRig.Index.Proximal = skeleton.indexProximal;
                    handRig.Index.Intermedial = skeleton.indexMiddle;
                    handRig.Index.Distal = skeleton.indexDistal;
                    handRig.Middle.Proximal = skeleton.middleProximal;
                    handRig.Middle.Intermedial = skeleton.middleMiddle;
                    handRig.Middle.Distal = skeleton.middleDistal;
                    handRig.Ring.Proximal = skeleton.ringProximal;
                    handRig.Ring.Intermedial = skeleton.ringMiddle;
                    handRig.Ring.Distal = skeleton.ringDistal;
                    handRig.Pinky.Proximal = skeleton.pinkyProximal;
                    handRig.Pinky.Intermedial = skeleton.pinkyMiddle;
                    handRig.Pinky.Distal = skeleton.pinkyDistal;

                    var hand = wristView.AddComponent<ManusVR.Hands.Hand>();
                    hand.DeviceType = gloveType == InterVRHandType.Left ? device_type_t.GLOVE_LEFT : device_type_t.GLOVE_RIGHT;
                    hand.Initialize(manusRigger);
                    if (gloveType == InterVRHandType.Left)
                    {
                        hand.HandYawOffset = vrGloveInterface.HandYawOffsetLeft.Value;
                        vrGloveInterface.HandYawOffsetLeft.DistinctUntilChanged().Subscribe(f =>
                        {
                            hand.HandYawOffset = f;
                        }).AddTo(subscriptions);
                    }
                    else
                    {
                        hand.HandYawOffset = vrGloveInterface.HandYawOffsetRight.Value;
                        vrGloveInterface.HandYawOffsetRight.DistinctUntilChanged().Subscribe(f =>
                        {
                            hand.HandYawOffset = f;
                        }).AddTo(subscriptions);
                    }

                    GameObject.Destroy(skeleton);
                    GameObject.Destroy(instance.GetComponentInChildren<Animator>());

                    // follow tracker
                    var vrHandTrackerEntity = vrInterface.GetHandTrackerEntity(gloveType);
                    pool.CreateEntity(new InterFollowEntityBlueprint(UpdateMomentType.Update,
                        vrHandTrackerEntity,
                        entity,
                        true,
                        true,
                        new Vector3(0.0f, 0.07f, -0.04f),
                        Vector3.zero));

                    var vrHandEntity = vrInterface.GetHandEntity(gloveType);
                    var vrHand = vrHandEntity.GetComponent<InterVRHand>();
                    var followEntityEntities = entityDatabase.GetEntitiesFor(new Group(typeof(InterFollowEntity)), 0);
                    foreach (var followEntityEntity in followEntityEntities)
                    {
                        var followEntity = followEntityEntity.GetComponent<InterFollowEntity>();
                        if (followEntity.FollowSourceEntity.HasComponent<InterVRHand>())
                        {
                            var vrHandSource = followEntity.FollowSourceEntity.GetComponent<InterVRHand>();
                            if (vrHandSource.Type == vrGlove.Type)
                            {
                                followEntity.FollowTargetEntity = wristEntity;
                                if (vrHand.Type == InterVRHandType.Left)
                                {
                                    followEntity.OffsetPosition = new Vector3(-0.14f, 0.03f, -0.08f);
                                    followEntity.OffsetRotation = new Vector3(0.0f, -135.0f, 90.0f);
                                }
                                else
                                {
                                    followEntity.OffsetPosition = new Vector3(0.14f, -0.03f, 0.08f);
                                    followEntity.OffsetRotation = new Vector3(0.0f, 45.0f, 90.0f);
                                }
                                break;
                            }
                        }
                    }

                    var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrGlove.Type);
                    if (vrHandRenderModelEntity != null)
                    {
                        var vrHandRenderModel = vrHandRenderModelEntity.GetComponent<InterVRHandRenderModel>();
                        vrHandRenderModel.SetVisibility(false, true);
                    }

                    var vrHandControllerRenderModelEntity = vrInterface.GetHandControllerRenderModelEntity(vrGlove.Type);
                    if (vrHandControllerRenderModelEntity != null)
                    {
                        var vrHandControllerRenderModel = vrHandControllerRenderModelEntity.GetComponent<InterVRHandControllerRenderModel>();
                        vrHandControllerRenderModel.SetVisibility(false, true);
                    }

                    vrGlove.RenderModel = instance;
                    vrGlove.Active.Value = true;

                }).AddTo(subscriptions);

            Observable.EveryUpdate()
                .Where(x => vrGloveIsActivated(entity) && HandDataManager.IsPlayerNumberValid(vrGloveInterface.PlayerNumber))
                .Subscribe(x =>
                {
                    var vrGlove = entity.GetComponent<InterVRGlove>();
                    var vrGloveView = entity.GetGameObject();
                    var wristEntity = vrGlove.WristEntity;
                    var wristView = wristEntity.GetGameObject();
                    var hand = wristView.GetComponent<Hand>();
                    if (HandDataManager.CanGetHandData(vrGloveInterface.PlayerNumber, hand.DeviceType))
                    {
                        ApolloHandData handData = HandDataManager.GetHandData(vrGloveInterface.PlayerNumber, hand.DeviceType);
                        hand.AnimateHand(handData);
                        hand.UpdateHand(handData);
                    }

                    if (vrGloveView.activeSelf)
                    {
                        bool state = vrHandTrackerIsValid(entity) && manusIsConnected(entity);
                        if (!state && vrGlove.GoingToDisconnect == false)
                        {
                            vrGlove.GoingToDisconnect = true;
                            vrGlove.DisconnectStateTimer = 0.0f;
                        }
                    }
                    else
                    {
                        bool state = vrHandTrackerIsValid(entity) && manusIsConnected(entity);
                        if (state)
                        {
                            vrGloveView.SetActive(true);
                        }
                    }

                    if (vrGlove.GoingToDisconnect)
                    {
                        vrGlove.DisconnectStateTimer += Time.deltaTime;
                       
                        bool state = vrHandTrackerIsValid(entity) && manusIsConnected(entity);
                        if (state)
                        {
                            vrGlove.GoingToDisconnect = false;
                        }
                        else if (vrGlove.DisconnectStateTimer >= InterVRGlove.MaxDisconnectStateTimeSeconds)
                        {
                            vrGloveView.SetActive(false);
                            vrGlove.GoingToDisconnect = false;
                        }
                    }
                });
        }

        public void Teardown(IEntity entity)
        {
            subscriptions.DisposeAll();
        }
    }
}