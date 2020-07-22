using Inter.Components;
using Inter.Modules.ToolModule;
using Inter.VR.Components;
using Inter.VR.Extensions;
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
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using Inter.Blueprints;
using Inter.Defines;
using System.Collections.Generic;
using EcsRx.Systems;
using EcsRx.Groups.Observable;

namespace Inter.VR.Systems
{
    public class TestBedSystem : IManualSystem
    {
        public IGroup Group => new EmptyGroup();
        
        private readonly IInterVRInterface vrInterface;
        private readonly IEntityDatabase entityDatabase;

        public TestBedSystem(IInterVRInterface vrInterface, IEntityDatabase entityDatabase)
        {
            this.vrInterface = vrInterface;
            this.entityDatabase = entityDatabase;
        }

        public void StartSystem(IObservableGroup observableGroup)
        {
            Observable.EveryUpdate().Subscribe(x =>
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    var vrHandEntity = vrInterface.GetHandEntity(Defines.InterVRHandType.Right);
                    if (vrHandEntity != null)
                    {
                        entityDatabase.RemoveEntity(vrHandEntity);
                    }
                }
            });
        }

        public void StopSystem(IObservableGroup observableGroup)
        {
        }
    }
}