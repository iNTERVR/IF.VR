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
using Inter.VR.Plugins.SteamVRInterface.Events;
using Valve.VR;
using Inter.VR.Defines;
using System.Collections.Generic;
using Inter.VR.Events;

namespace Inter.VR.Plugins.SteamVRInterface.Systems
{
    public class SteamVRHandSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRHand), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly IEventSystem eventSystem;
        private readonly IInterVRInterface vrInterface;
        private readonly IInterVRHandInterface vrHandInterface;

        public SteamVRHandSystem(IEventSystem eventSystem,
            IInterVRInterface vrInterface,
            IInterVRHandInterface vrHandInterface)
        {
            this.eventSystem = eventSystem;
            this.vrInterface = vrInterface;
            this.vrHandInterface = vrHandInterface;
        }

        public void Setup(IEntity entity)
        {
            var subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            var vrHand = entity.GetComponent<InterVRHand>();
            vrHand.Active.DistinctUntilChanged().Subscribe(active =>
            {
                if (active)
                {
                    if (!entity.HasComponent<SteamVRHand>())
                        entity.AddComponent<SteamVRHand>();
                }
            }).AddTo(subscriptions);

            eventSystem.Receive<InterVR_SteamVR_Behaviour_Pose_TransformUpdatedEvent>()
                .Subscribe(evt =>
                {
                    var currentAttachedInfoEntity = vrHandInterface.GetLastAttachedInfoEntity(entity);
                    if (currentAttachedInfoEntity != null && currentAttachedInfoEntity.HasComponent<InterVRHandAttachedInfo>())
                    {
                        var attachedInfo = currentAttachedInfoEntity.GetComponent<InterVRHandAttachedInfo>();
                        if (attachedInfo.AttachedEntity.HasComponent<InterVRInteractable>() && attachedInfo.AttachedEntity.HasComponent<SteamVRInteractable>())
                        {
                            SteamVR_Skeleton_PoseSnapshot pose = null;

                            var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);
                            var steamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                            if (steamVRHandRenderModel.Skeleton != null)
                            {
                                var steamVRInteractable = attachedInfo.AttachedEntity.GetComponent<SteamVRInteractable>();
                                if (steamVRInteractable.SkeletonPoser != null)
                                {
                                    pose = steamVRInteractable.SkeletonPoser.GetBlendedPose(steamVRHandRenderModel.Skeleton);
                                }
                            }

                            var vrInteractable = attachedInfo.AttachedEntity.GetComponent<InterVRInteractable>();
                            if (vrInteractable.HandFollowTransform)
                            {
                                var vrHandView = entity.GetGameObject();
                                var vrHandTransform = vrHandView.transform;
                                var attachView = attachedInfo.AttachedEntity.GetGameObject();
                                var attachTransform = attachView.transform;

                                Quaternion targetHandRotation = Quaternion.identity;
                                Vector3 targetHandPosition = Vector3.zero;

                                if (pose == null)
                                {
                                    Quaternion offset = Quaternion.Inverse(vrHandTransform.rotation) * attachedInfo.HandAttachmentPointTransform.rotation;
                                    targetHandRotation = attachTransform.rotation * Quaternion.Inverse(offset);

                                    Vector3 worldOffset = (vrHandTransform.position - attachedInfo.HandAttachmentPointTransform.position);
                                    Quaternion rotationDiff = steamVRHandRenderModel.Instance.transform.rotation * Quaternion.Inverse(vrHandTransform.rotation);
                                    Vector3 localOffset = rotationDiff * worldOffset;
                                    targetHandPosition = attachTransform.position + localOffset;
                                }
                                else
                                {
                                    Transform objectT = attachView.transform;
                                    Vector3 oldItemPos = objectT.position;
                                    Quaternion oldItemRot = objectT.transform.rotation;
                                    objectT.position = targetItemPosition(entity, attachedInfo);
                                    objectT.rotation = targetItemRotation(entity, attachedInfo);
                                    Vector3 localSkelePos = objectT.InverseTransformPoint(vrHandTransform.position);
                                    Quaternion localSkeleRot = Quaternion.Inverse(objectT.rotation) * vrHandTransform.rotation;
                                    objectT.position = oldItemPos;
                                    objectT.rotation = oldItemRot;

                                    targetHandPosition = objectT.TransformPoint(localSkelePos);
                                    targetHandRotation = objectT.rotation * localSkeleRot;
                                }

                                steamVRHandRenderModel.Instance.transform.rotation = targetHandRotation;
                                steamVRHandRenderModel.Instance.transform.position = targetHandPosition;
                            }
                        }
                    }

                }).AddTo(subscriptions);

            Observable.EveryFixedUpdate()
                .Where(x => vrHandInterface.GetLastAttachedInfoEntity(entity) != null)
                .Subscribe(x =>
                {
                    var currentAttachedInfoEntity = vrHandInterface.GetLastAttachedInfoEntity(entity);
                    var attachedInfo = currentAttachedInfoEntity.GetComponent<InterVRHandAttachedInfo>();
                    if (attachedInfo.AttachedEntity != null)
                    {
                        var attachView = attachedInfo.AttachedEntity.GetGameObject();
                        var attachTransform = attachView.transform;

                        if (attachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.VelocityMovement))
                        {
                            // ckbang - ease in
                            //if (attachedInfo.interactable.attachEaseIn == false || attachedInfo.interactable.snapAttachEaseInCompleted)
                            //    UpdateAttachedVelocity(attachedInfo);
                        }
                        else
                        {
                            if (attachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.ParentToHand))
                            {
                                attachTransform.position = targetItemPosition(entity, attachedInfo);
                                attachTransform.rotation = targetItemRotation(entity, attachedInfo);
                            }
                        }

                        // ckbang - ease in
                        //if (attachedInfo.interactable.attachEaseIn)
                        //{
                        //    float t = Util.RemapNumberClamped(Time.time, attachedInfo.attachTime, attachedInfo.attachTime + attachedInfo.interactable.snapAttachEaseInTime, 0.0f, 1.0f);
                        //    if (t < 1.0f)
                        //    {
                        //        if (attachedInfo.HasAttachFlag(AttachmentFlags.VelocityMovement))
                        //        {
                        //            attachedInfo.attachedRigidbody.velocity = Vector3.zero;
                        //            attachedInfo.attachedRigidbody.angularVelocity = Vector3.zero;
                        //        }
                        //        t = attachedInfo.interactable.snapAttachEaseInCurve.Evaluate(t);
                        //        attachedInfo.attachedGameObject.transform.position = Vector3.Lerp(attachedInfo.easeSourcePosition, TargetItemPosition(attachedInfo), t);
                        //        attachedInfo.attachedGameObject.transform.rotation = Quaternion.Lerp(attachedInfo.easeSourceRotation, TargetItemRotation(attachedInfo), t);
                        //    }
                        //    else if (!attachedInfo.interactable.snapAttachEaseInCompleted)
                        //    {
                        //        attachedInfo.interactable.gameObject.SendMessage("OnThrowableAttachEaseInCompleted", this, SendMessageOptions.DontRequireReceiver);
                        //        attachedInfo.interactable.snapAttachEaseInCompleted = true;
                        //    }
                        //}
                    }
                }).AddTo(subscriptions);

            Observable.EveryUpdate()
                .Where(x => vrHandInterface.GetLastAttachedInfoEntity(entity) != null)
                .Subscribe(x =>
                {
                    var currentAttachedInfoEntity = vrHandInterface.GetLastAttachedInfoEntity(entity);
                    var attachedInfo = currentAttachedInfoEntity.GetComponent<InterVRHandAttachedInfo>();
                    if (attachedInfo.AttachedEntity != null)
                    {
                        eventSystem.Publish(new OnHandAttachedUpdate() { HandEntity = entity });
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

        Vector3 targetItemPosition(IEntity handEntity, InterVRHandAttachedInfo attachedInfo)
        {
            var vrHand = handEntity.GetComponent<InterVRHand>();
            var vrHandView = handEntity.GetGameObject();
            var vrHandTransform = vrHandView.transform;
            if (attachedInfo.AttachedEntity.HasComponent<SteamVRInteractable>())
            {
                var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);
                if (vrHandRenderModelEntity.HasComponent<SteamVRHandRenderModel>())
                {
                    var steamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                    if (steamVRHandRenderModel.Skeleton != null)
                    {
                        var steamVRInteractable = attachedInfo.AttachedEntity.GetComponent<SteamVRInteractable>();
                        if (steamVRInteractable.SkeletonPoser != null)
                        {
                            Vector3 tp = attachedInfo.HandAttachmentPointTransform.InverseTransformPoint(vrHandTransform.TransformPoint(steamVRInteractable.SkeletonPoser.GetBlendedPose(steamVRHandRenderModel.Skeleton).position));
                            //tp.x *= -1;
                            return attachedInfo.HandAttachmentPointTransform.TransformPoint(tp);
                        }
                    }
                }
            }

            return attachedInfo.HandAttachmentPointTransform.TransformPoint(attachedInfo.InitialPositionalOffset);
        }

        Quaternion targetItemRotation(IEntity handEntity, InterVRHandAttachedInfo attachedInfo)
        {
            var vrHand = handEntity.GetComponent<InterVRHand>();
            var vrHandView = handEntity.GetGameObject();
            var vrHandTransform = vrHandView.transform;
            if (attachedInfo.AttachedEntity.HasComponent<SteamVRInteractable>())
            {
                var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);
                if (vrHandRenderModelEntity.HasComponent<SteamVRHandRenderModel>())
                {
                    var steamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                    if (steamVRHandRenderModel.Skeleton != null)
                    {
                        var steamVRInteractable = attachedInfo.AttachedEntity.GetComponent<SteamVRInteractable>();
                        if (steamVRInteractable.SkeletonPoser != null)
                        {
                            Quaternion tr = Quaternion.Inverse(attachedInfo.HandAttachmentPointTransform.rotation) * (vrHandTransform.rotation * steamVRInteractable.SkeletonPoser.GetBlendedPose(steamVRHandRenderModel.Skeleton).rotation);
                            return attachedInfo.HandAttachmentPointTransform.rotation * tr;
                        }
                    }
                }
            }

            return attachedInfo.HandAttachmentPointTransform.rotation * attachedInfo.InitialRotationalOffset;
        }
    }
}