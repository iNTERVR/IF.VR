using InterVR.IF.Components;
using InterVR.IF.Modules;
using InterVR.Unity.SDK.SteamVR.Components;
using InterVR.Unity.SDK.SteamVR.Extensions;
using InterVR.Unity.SDK.SteamVR.Modules.InterVRInterfaces;
using InterVR.Unity.SDK.SteamVR.Installer;
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
using InterVR.IF.Blueprints;
using InterVR.IF.Defines;
using System.Collections.Generic;
using UniRx.Triggers;

namespace InterVR.Unity.SDK.SteamVR.Systems
{
    public class InterVRHandSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRHand), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly SteamVRInstaller.Settings settings;
        private readonly IGameObjectTool gameObjectTool;
        private readonly IInterVRInterface vrInterface;
        private readonly IEntityDatabase entityDatabase;
        private readonly IEventSystem eventSystem;

        public InterVRHandSystem(SteamVRInstaller.Settings settings,
            IGameObjectTool gameObjectTool,
            IInterVRInterface vrInterface,
            IEntityDatabase entityDatabase,
            IEventSystem eventSystem)
        {
            this.settings = settings;
            this.gameObjectTool = gameObjectTool;
            this.vrInterface = vrInterface;
            this.entityDatabase = entityDatabase;
            this.eventSystem = eventSystem;
        }

        bool trackerisActivated(IEntity entity)
        {
            var vrHand = entity.GetComponent<InterVRHand>();
            var vrHandTrackerEntity = vrInterface.GetHandTrackerEntity(vrHand.Type);
            if (vrHandTrackerEntity == null)
                return false;

            var vrHandTracker = vrHandTrackerEntity.GetComponent<InterVRHandTracker>();
            return vrHandTracker.IsActive();
        }

        public void Setup(IEntity entity)
        {
            var subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            var vrHand = entity.GetComponent<InterVRHand>();
            var vrHandView = entity.GetGameObject();
            vrHandView.OnDestroyAsObservable()
                .Subscribe(x =>
                {
                    entityDatabase.RemoveEntity(entity);
                    Debug.Log("Remove on destroy");
                }).AddTo(vrHandView);

            // create instance
            var applicationLostFocusInstance = new GameObject("_application_lost_focus");
            applicationLostFocusInstance.transform.parent = vrHandView.transform;
            applicationLostFocusInstance.SetActive(false);
            
            // convert instance to entity
            var pool = entityDatabase.GetCollection();
            vrHand.ApplicationLostFocusEntity = pool.CreateEntity();
            applicationLostFocusInstance.LinkEntity(vrHand.ApplicationLostFocusEntity, pool);

            // waiting tracker has been activated for initialize
            Observable.EveryUpdate()
                .Where(x => trackerisActivated(entity))
                .First()
                .Subscribe(x =>
                {
                    // initialize
                    var vrHandTrackerEntity = vrInterface.GetHandTrackerEntity(vrHand.Type);
                    var vrHandTracker = vrHandTrackerEntity.GetComponent<InterVRHandTracker>();
                    vrHandTracker.Valid.DistinctUntilChanged().Subscribe(value =>
                    {
                        vrHandView.SetActive(value);
                        if (value)
                        {
                            // valid
                            // gameObject set active true
                        }
                        else
                        {
                            // not valid
                            // detach object if has
                            // gameObject set active false
                        }
                    });

                    pool.CreateEntity(new IF_FollowEntityBlueprint(UpdateMomentType.Update,
                        vrHandTrackerEntity,
                        entity,
                        true,
                        true,
                        Vector3.zero,
                        Vector3.zero));

                    vrHand.Active.Value = true;
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
    }
}