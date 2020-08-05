using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Plugins.ReactiveSystems.Systems;
using EcsRx.Plugins.Views.Components;
using System;
using UniRx;
using System.Collections.Generic;
using UnityEngine;
using EcsRx.Events;
using Inter.VR.Events;
using EcsRx.Unity.Extensions;
using Inter.VR.VRPrototype.Components;

namespace Inter.VR.VRPrototype.Systems
{
    public class VRPrototypeInteractableSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRInteractable), typeof(VRPrototypeInteractable), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly IInterVRInterface vrInterface;
        private readonly IInterVRHandGrabStatus vrHandGrabStatus;
        private readonly IInterVRHandInterface vrHandInterface;
        private readonly IEventSystem eventSystem;

        public VRPrototypeInteractableSystem(
            IInterVRInterface vrInterface,
            IInterVRHandGrabStatus vrHandGrabStatus,
            IInterVRHandInterface vrHandInterface,
            IEventSystem eventSystem)
        {
            this.vrInterface = vrInterface;
            this.vrHandGrabStatus = vrHandGrabStatus;
            this.vrHandInterface = vrHandInterface;
            this.eventSystem = eventSystem;
        }

        public void Setup(IEntity entity)
        {
            List<IDisposable> subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            var vrInteractable = entity.GetComponent<InterVRInteractable>();
            var vrPrototypeInteractable = entity.GetComponent<VRPrototypeInteractable>();
            var vrPrototypeInteractableView = entity.GetGameObject();
            vrPrototypeInteractable.GeneralText.text = "No Hand Hovering";
            vrPrototypeInteractable.HoveringText.text = "Hovering: False";

            eventSystem.Receive<OnHandHoverBeginEvent>()
                .Subscribe(evt =>
                {
                    if (evt.HoveringEntity.Id == entity.Id)
                    {
                        var vrHandView = evt.HandEntity.GetGameObject();
                        vrPrototypeInteractable.GeneralText.text = "Hovering hand: " + vrHandView.name;
                    }
                }).AddTo(subscriptions);

            eventSystem.Receive<OnHandHoverEndEvent>()
                .Subscribe(evt =>
                {
                    if (evt.HoveringEntity.Id == entity.Id)
                    {
                        vrPrototypeInteractable.GeneralText.text = "No Hand Hovering";
                    }
                }).AddTo(subscriptions);

            eventSystem.Receive<OnHandHoverUpdateEvent>()
                .Subscribe(evt =>
                {
                    var vrHandEntity = evt.HandEntity;
                    var vrHand = evt.HandEntity.GetComponent<InterVRHand>();
                    if (vrHand.HoveringInteractableEntity.Id == entity.Id)
                    {
                        var vrInteractableEntity = entity;
                        var startingHandGrabType = vrHandGrabStatus.GetGrabStarting(vrHandEntity);
                        bool isGrabEnding = vrHandGrabStatus.IsGrabEnding(vrHandEntity, vrInteractableEntity);

                        if (vrInteractable.AttachedToHandEntity == null && startingHandGrabType != HandGrabTypes.None)
                        {
                            // Save our position/rotation so that we can restore it when we detach
                            vrPrototypeInteractable.OldPosition = vrPrototypeInteractableView.transform.position;
                            vrPrototypeInteractable.OldRotation = vrPrototypeInteractableView.transform.rotation;

                            // Call this to continue receiving HandHoverUpdate messages,
                            // and prevent the hand from hovering over anything else
                            vrInterface.HandHoverLock(vrHandEntity, vrInteractableEntity);

                            // Attach this object to the hand
                            vrHandInterface.AttachToHand(vrHandEntity, vrInteractableEntity, startingHandGrabType, vrPrototypeInteractable.AttachmentFlags);
                        }
                        else if (isGrabEnding)
                        {
                            // Detach this object from the hand
                            vrHandInterface.DetachFromHand(vrHandEntity, vrInteractableEntity);

                            // Call this to undo HoverLock
                            vrInterface.HandHoverUnlock(vrHandEntity, vrInteractableEntity);

                            // Restore position/rotation
                            vrPrototypeInteractableView.transform.position = vrPrototypeInteractable.OldPosition;
                            vrPrototypeInteractableView.transform.rotation = vrPrototypeInteractable.OldRotation;
                        }
                    }
                }).AddTo(subscriptions);

            eventSystem.Receive<OnAttachedToHandEvent>()
                .Subscribe(evt =>
                {
                    var attachedInfoEntity = vrHandInterface.GetLastAttachedInfoEntity(evt.HandEntity);
                    var attachedInfo = attachedInfoEntity.GetComponent<InterVRHandAttachedInfo>();
                    if (attachedInfo.AttachedEntity.Id == entity.Id)
                    {
                        var vrHandView = evt.HandEntity.GetGameObject();
                        vrPrototypeInteractable.GeneralText.text = string.Format("Attached: {0}", vrHandView.name);
                        vrPrototypeInteractable.AttachTime = Time.time;
                    }
                }).AddTo(subscriptions);

            eventSystem.Receive<OnDetachedToHandEvent>()
                .Subscribe(evt =>
                {
                    if (evt.AttachedEntity != null && evt.AttachedEntity.Id == entity.Id)
                    {
                        var vrHandView = evt.HandEntity.GetGameObject();
                        vrPrototypeInteractable.GeneralText.text = string.Format("Detached: {0}", vrHandView.name);
                    }
                }).AddTo(subscriptions);

            eventSystem.Receive<OnHandAttachedUpdateEvent>()
                .Subscribe(evt =>
                {
                    if (evt.AttachedEntity != null && evt.AttachedEntity.Id == entity.Id)
                    {
                        var vrHandView = evt.HandEntity.GetGameObject();
                        vrPrototypeInteractable.GeneralText.text = string.Format("Attached: {0} :: Time: {1:F2}", vrHandView.name, (Time.time - vrPrototypeInteractable.AttachTime));
                    }
                }).AddTo(subscriptions);

            eventSystem.Receive<OnHandFocusLostEvent>()
                .Subscribe(evt =>
                {
                    if (evt.AttachedEntity != null && evt.AttachedEntity.Id == entity.Id)
                    {
                    }
                }).AddTo(subscriptions);

            eventSystem.Receive<OnHandFocusAcquiredEvent>()
                .Subscribe(evt =>
                {
                    if (evt.AttachedEntity != null && evt.AttachedEntity.Id == entity.Id)
                    {
                    }
                }).AddTo(subscriptions);

            Observable.EveryUpdate()
                .Subscribe(x =>
                {
                    if (vrInteractable.IsHovering != vrPrototypeInteractable.LastHovering)
                    {
                        vrPrototypeInteractable.HoveringText.text = string.Format("Hovering: {0}", vrInteractable.IsHovering);
                        vrPrototypeInteractable.LastHovering = vrInteractable.IsHovering;
                    }
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