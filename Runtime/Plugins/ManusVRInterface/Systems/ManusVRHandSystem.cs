using Inter.VR.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Plugins.ReactiveSystems.Systems;
using EcsRx.Plugins.Views.Components;
using System;
using UniRx;
using Inter.VR.Plugins.ManusVRInterface.Components;
using System.Collections.Generic;
using Inter.VR.Extensions;
using ManusVR.Hands;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.Plugins.ManusVRInterface.Extensions;
using Inter.VR.Defines;
using ManusVR.SDK.Apollo;
using ManusVR.SDK.Manus;
using UnityEngine;
using Inter.Modules.ToolModule;
using EcsRx.Collections.Database;
using EcsRx.Unity.Extensions;

namespace Inter.VR.Plugins.ManusVRInterface.Systems
{
    public class ManusVRHandSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRHand), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly IInterVRInterface vrInterface;
        private readonly IInterVRGloveInterface vrGloveInterface;
        private readonly IGameObjectTool gameObjectTool;
        private readonly IEntityDatabase entityDatabase;

        public ManusVRHandSystem(IInterVRInterface vrInterface, IInterVRGloveInterface vrGloveInterface,
            IGameObjectTool gameObjectTool,
            IEntityDatabase entityDatabase)
        {
            this.vrInterface = vrInterface;
            this.vrGloveInterface = vrGloveInterface;
            this.gameObjectTool = gameObjectTool;
            this.entityDatabase = entityDatabase;
        }

        public void Setup(IEntity entity)
        {
            var subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            var vrHand = entity.GetComponent<InterVRHand>();
            Observable.EveryUpdate()
                .Where(x => vrHand.IsActive())
                .First()
                .Subscribe(x =>
                {
                    var glovesRoot = vrGloveInterface.GetRootTransform();
                    if (glovesRoot == null)
                    {
                        var rootTransform = vrInterface.HMDRootTransform;
                        var glovesRootGo = new GameObject("InterVRGloves");
                        gameObjectTool.SetParentWithInit(glovesRootGo, rootTransform);
                        glovesRoot = glovesRootGo.transform;
                        vrGloveInterface.SetRootTransform(glovesRoot);
                    }

                    createGloveEntity(glovesRoot, vrHand.Type);
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

        void createGloveEntity(Transform parent, InterVRHandType handType)
        {
            string gloveName = "InterVRGlove" + handType.ToString();
            var gloveGo = new GameObject(gloveName);
            var gloveWristGo = new GameObject(gloveName + "Wrist");

            gameObjectTool.SetParentWithInit(gloveGo, parent);
            gameObjectTool.SetParentWithInit(gloveWristGo, gloveGo.transform);

            var pool = entityDatabase.GetCollection();
            var gloveEntity = pool.CreateEntity();
            gloveGo.LinkEntity(gloveEntity, pool);
            gloveEntity.AddComponent(new InterVRGlove()
            {
                Type = handType,
                Wrist = gloveWristGo.transform
            });
        }
    }
}