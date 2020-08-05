using Inter.Modules.ToolModule;
using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.Plugins.SteamVRInterface.Installer;
using Inter.VR.Installer;
using EcsRx.Collections.Database;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Plugins.ReactiveSystems.Systems;
using EcsRx.Plugins.Views.Components;
using System;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using Inter.VR.Plugins.SteamVRInterface.Extensions;

namespace Inter.VR.Plugins.SteamVRInterface.Systems
{
    public class SteamVRRigSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRRig), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly InterVRInstaller.Settings settings;
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;
        private readonly IGameObjectTool gameObjectTool;
        private readonly IInterVRInterface vrInterface;
        private readonly IEntityDatabase entityDatabase;

        public SteamVRRigSystem(InterVRInstaller.Settings settings,
            SteamVRInterfaceInstaller.Settings steamVRSettings,
            IGameObjectTool gameObjectTool,
            IInterVRInterface vrInterface,
            IEntityDatabase entityDatabase)
        {
            this.settings = settings;
            this.steamVRSettings = steamVRSettings;
            this.gameObjectTool = gameObjectTool;
            this.vrInterface = vrInterface;
            this.entityDatabase = entityDatabase;
        }

        public void Setup(IEntity entity)
        {
            var subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            var vrRig = entity.GetComponent<InterVRRig>();
            vrRig.HMDRoot.gameObject.SetActive(false);
            vrRig.HMDFallbackRoot.gameObject.SetActive(false);
            var steamVRParent = vrRig.HMD.transform;

            // create [SteamVR] (actualy init)
            var steamVRPrefab = steamVRSettings.SteamVRConfig;
            gameObjectTool.InstantiateWithInit(steamVRPrefab, steamVRParent);

            Observable.EveryUpdate()
                .Where(x => vrInterface.SteamVRInitialized())
                .First()
                .Subscribe(x =>
                {
                    if (!vrInterface.SteamVRValid())
                    {
                        vrInterface.CurrentRigType = InterVRRigType.Fallback;
                    }
                    else
                    {
                        vrInterface.CurrentRigType = vrRig.StartRigType;
                    }

                    if (vrRig.AudioListenerTransform != null)
                    {
                        gameObjectTool.SetParentWithInit(vrRig.AudioListenerTransform.gameObject, vrInterface.HMDTransform);
                    }

                    var playVolumePrefab = steamVRSettings.PlayVolume;
                    gameObjectTool.InstantiateWithInit(playVolumePrefab, steamVRParent);
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