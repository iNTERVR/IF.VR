using Inter.Components;
using Inter.Modules.ToolModule;
using Inter.VR.Components;
using Inter.VR.Extensions;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.Plugins.SteamVRInterface.Components;
using Inter.VR.Plugins.SteamVRInterface.Extensions;
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
using Inter.Blueprints;
using Inter.Defines;
using Inter.VR.Defines;
using Valve.VR;
using EcsRx.UnityEditor.MonoBehaviours;
using Inter.VR.Plugins.SteamVRInterface.Events;
using EcsRx.Unity.MonoBehaviours;
using Inter.VR.Events;
using System.Collections.Generic;
using ManusVR.Hands;

namespace Inter.VR.Plugins.SteamVRInterface.Systems
{
    public class SteamVRHandHoverSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRHand), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly InterVRInstaller.Settings vrSettings;
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;
        private readonly IGameObjectTool gameObjectTool;
        private readonly IInterVRInterface vrInterface;
        private readonly IInterVRHandInterface vrHandInterface;
        private readonly IEntityDatabase entityDatabase;
        private readonly IEventSystem eventSystem;

        public SteamVRHandHoverSystem(InterVRInstaller.Settings vrSettings,
            SteamVRInterfaceInstaller.Settings steamVRSettings,
            IGameObjectTool gameObjectTool,
            IInterVRInterface vrInterface,
            IInterVRHandInterface vrHandInterface,
            IEntityDatabase entityDatabase,
            IEventSystem eventSystem)
        {
            this.vrSettings = vrSettings;
            this.steamVRSettings = steamVRSettings;
            this.gameObjectTool = gameObjectTool;
            this.vrInterface = vrInterface;
            this.vrHandInterface = vrHandInterface;
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
            var handType = vrHand.Type;
            var vrHandView = entity.GetGameObject();

            Observable.EveryUpdate()
                .Where(x => trackerisActivated(entity))
                .First()
                .Subscribe(x =>
                {
                    float hoverUpdateBegin = vrHand.Type == InterVRHandType.Left ? (0.5f * vrHand.HoverUpdateInterval) : 0.0f;
                    Observable.Interval(TimeSpan.FromSeconds(hoverUpdateBegin))
                        .First()
                        .Subscribe(y =>
                        {
                            // Debug.Log($"hover update launched {vrHand.Type} in after {hoverUpdateBegin} seconds");
                            Observable.Interval(TimeSpan.FromSeconds(vrHand.HoverUpdateInterval))
                                .Subscribe(z =>
                                {
                                    vrInterface.UpdateHandHovering(entity);
                                }).AddTo(subscriptions);
                        }).AddTo(subscriptions);

                    SteamVR_Events.InputFocus.AsObservable().Subscribe(hasFocus =>
                    {
                        if (hasFocus)
                        {
                            vrHandInterface.DetachFromHand(entity, vrHand.ApplicationLostFocusEntity);
                            vrHand.ApplicationLostFocusEntity.GetGameObject().SetActive(false);
                            vrInterface.UpdateHandHovering(entity);
                        }
                        else
                        {
                            vrHand.ApplicationLostFocusEntity.GetGameObject().SetActive(true);
                            vrHandInterface.AttachToHand(entity, vrHand.ApplicationLostFocusEntity, HandGrabTypes.Scripted, HandAttachmentFlags.ParentToHand);
                        }

                        eventSystem.Publish(new InterVR_SteamVR_Event_InputFocus() { HasFocus = hasFocus });

                    }).AddTo(subscriptions);

                    Observable.FromEvent<EditorGUICallbacks.DrawGizmos>(
                        h => () => h(),
                        h => EditorGUICallbacks.OnDrawGizmosEventCallback += h,
                        h => EditorGUICallbacks.OnDrawGizmosEventCallback -= h)
                        .Subscribe(_ =>
                        {
                            onDrawGizmos(entity);
                        });
                }).AddTo(subscriptions);

            Observable.EveryUpdate()
                .Where(x => vrHand.HoveringInteractableEntity != null)
                .Subscribe(x =>
                {
                    eventSystem.Publish(new OnHandHoverUpdateEvent() { HandEntity = entity });
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

        void onDrawGizmos(IEntity entity)
        {
            if (!entity.HasComponent<InterVRHand>())
                return;

            var vrHand = entity.GetComponent<InterVRHand>();
            if (vrHand.HoverSphereTransform != null)
            {
                Gizmos.color = Color.green;
                float scaledHoverRadius = vrHand.HoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(vrHand.HoverSphereTransform));
                Gizmos.DrawWireSphere(vrHand.HoverSphereTransform.position, scaledHoverRadius / 2);
            }
        }
    }
}