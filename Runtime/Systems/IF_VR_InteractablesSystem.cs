using EcsRx.Groups;
using EcsRx.Events;
using UniRx;
using System.Collections.Generic;
using System;
using EcsRx.Extensions;
using EcsRx.Collections.Database;
using EcsRx.Unity.Extensions;
using InterVR.IF.VR.Components;
using EcsRx.Plugins.ReactiveSystems.Systems;
using EcsRx.Entities;
using UniRx.Triggers;
using EcsRx.Plugins.Views.Components;

namespace InterVR.IF.VR.Systems
{
    public class IF_VR_InteractableSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(IF_VR_Interactable), typeof(ViewComponent));

        private List<IDisposable> subscriptions = new List<IDisposable>();
        private readonly IEntityDatabase entityDatabase;

        public IF_VR_InteractableSystem(IEntityDatabase entityDatabase)
        {
            this.entityDatabase = entityDatabase;
        }

        public void Setup(IEntity entity)
        {
            var view = entity.GetGameObject();
            view.OnDestroyAsObservable().Subscribe(x =>
            {
                entityDatabase.RemoveEntity(entity);
            }).AddTo(subscriptions);
        }

        public void Teardown(IEntity entity)
        {
            subscriptions.DisposeAll();
        }
    }
}