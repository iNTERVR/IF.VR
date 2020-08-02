using EcsRx.Collections.Database;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Unity.Extensions;
using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Events;
using Inter.VR.Extensions;
using Inter.VR.Installer;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.Plugins.ManusVRInterface.Installer;
using Inter.VR.Plugins.SteamVRInterface.Components;
using Inter.VR.Plugins.SteamVRInterface.Extensions;
using Inter.VR.Plugins.SteamVRInterface.Installer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace Inter.VR.Plugins.ManusVRInterface.Modules
{
    public class ManusVRHandInterface : IInterVRHandInterface
    {
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;
        private readonly InterVRInstaller.Settings vrSettings;
        private readonly IEntityDatabase entityDatabase;
        private readonly IInterVRInterface vrInterface;
        private readonly IEventSystem eventSystem;

        public ManusVRHandInterface(SteamVRInterfaceInstaller.Settings steamVRSettings,
            ManusVRInterfaceInstaller.Settings manusVRSettings,
            InterVRInstaller.Settings vrSettings,
            IEntityDatabase entityDatabase,
            IInterVRInterface vrInterface,
            IEventSystem eventSystem)
        {
            this.steamVRSettings = steamVRSettings;
            this.vrSettings = vrSettings;
            this.entityDatabase = entityDatabase;
            this.vrInterface = vrInterface;
            this.eventSystem = eventSystem;
        }

        public void AttachToHand(IEntity vrHandEntity, IEntity attachEntity, HandGrabTypes grabbedWithType, HandAttachmentFlags flags = HandAttachmentFlags.Default, Transform attachmentOffset = null)
        {
            var vrHand = vrHandEntity.GetComponent<InterVRHand>();
            var vrHandView = vrHandEntity.GetGameObject();
            var vrHandTransform = vrHandView.transform;

            var attachView = attachEntity.GetGameObject();
            var attachTransform = attachView.transform;

            var vrGloveEntity = vrInterface.GetGloveEntity(vrHand.Type);
            if (vrGloveEntity != null)
            {
                var vrGlove = vrGloveEntity.GetComponent<InterVRGlove>();
                vrGlove.SetVisibility(false);

                var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);
                var vrHandRenderModel = vrHandRenderModelEntity.GetComponent<InterVRHandRenderModel>();
                vrHandRenderModel.SetVisibility(true);
            }

            if (flags == 0)
            {
                flags = HandAttachmentFlags.Default;
            }

            var vrHandAttachedInfo = new InterVRHandAttachedInfo();
            vrHandAttachedInfo.AttachedHandEntity = vrHandEntity;
            vrHandAttachedInfo.AttachmentFlags = flags;
            vrHandAttachedInfo.AttachedOffsetTransform = attachmentOffset;
            vrHandAttachedInfo.AttachTime = Time.time;

            //Detach the object if it is already attached so that it can get re-attached at the top of the stack
            if (objectIsAttached(vrHandEntity, attachEntity))
            {
                DetachFromHand(vrHandEntity, attachEntity);
            }

            if (vrHandAttachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.DetachFromOtherHand))
            {
                var vrHandOtherEntity = vrInterface.GetHandEntity(vrHand.Type == InterVRHandType.Left ? InterVRHandType.Right : InterVRHandType.Left);
                if (vrHandOtherEntity != null)
                    DetachFromHand(vrHandOtherEntity, attachEntity);
            }

            if (vrHandAttachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.DetachOthers))
            {
                // Detach all the objects from the stack
                while (true)
                {
                    var attachedInfoEntities = GetAttachedInfoEntities(vrHandEntity);
                    if (attachedInfoEntities.Count() == 0)
                        break;

                    DetachFromHand(vrHandEntity, attachedInfoEntities.First());
                }
            }

            var currentAttachedEntity = GetLastAttachedInfoEntity(vrHandEntity);
            if (currentAttachedEntity != null)
            {
                eventSystem.Publish(new OnHandFocusLostEvent() { HandEntity = vrHandEntity });
            }

            vrHandAttachedInfo.AttachedEntity = attachEntity;
            // ckbang - teleport
            //attachedObject.allowTeleportWhileAttachedToHand = objectToAttach.GetComponent<AllowTeleportWhileAttachedToHand>();
            vrHandAttachedInfo.HandAttachmentPointTransform = vrHandTransform;

            InterVRInteractable vrInteractable = null;
            if (vrHandAttachedInfo.AttachedEntity.HasComponent<InterVRInteractable>())
            {
                vrInteractable = vrHandAttachedInfo.AttachedEntity.GetComponent<InterVRInteractable>();
                // ckbang - ease In
                //if (attachedObject.interactable.attachEaseIn)
                //{
                //    attachedObject.easeSourcePosition = attachedObject.attachedObject.transform.position;
                //    attachedObject.easeSourceRotation = attachedObject.attachedObject.transform.rotation;
                //    attachedObject.interactable.snapAttachEaseInCompleted = false;
                //}
                if (vrInteractable.UseHandObjectAttachmentPoint)
                {
                    vrHandAttachedInfo.HandAttachmentPointTransform = vrHand.ObjectAttachmentPoint;
                }

                //if (attachedObject.interactable.hideHandOnAttach)
                //    Hide();

                //if (attachedObject.interactable.hideSkeletonOnAttach && mainRenderModel != null && mainRenderModel.displayHandByDefault)
                //    HideSkeleton();

                //if (attachedObject.interactable.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
                //    HideController();

                //if (attachedObject.interactable.handAnimationOnPickup != 0)
                //    SetAnimationState(attachedObject.interactable.handAnimationOnPickup);

                //if (attachedObject.interactable.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                //    SetTemporarySkeletonRangeOfMotion(attachedObject.interactable.setRangeOfMotionOnPickup);
            }

            vrHandAttachedInfo.OriginalParent = attachTransform.parent != null ? attachTransform.parent.gameObject : null;

            vrHandAttachedInfo.AttachedRigidbody = attachView.GetComponent<Rigidbody>();
            if (vrHandAttachedInfo.AttachedRigidbody != null)
            {
                if (vrInteractable != null && vrInteractable.AttachedToHandEntity != null) //already attached to another hand
                {
                    var attachedToOtherHandEntity = vrInteractable.AttachedToHandEntity;
                    var attachedToOtherInfoEntities = GetAttachedInfoEntities(attachedToOtherHandEntity);
                    foreach (var attachedToOtherInfoEntity in attachedToOtherInfoEntities)
                    {
                        var attachedToOtherInfo = attachedToOtherInfoEntity.GetComponent<InterVRHandAttachedInfo>();
                        if (attachedToOtherInfo.AttachedEntity.Id == vrHandAttachedInfo.AttachedEntity.Id)
                        {
                            vrHandAttachedInfo.AttachedRigidbodyWasKinematic = attachedToOtherInfo.AttachedRigidbodyWasKinematic;
                            vrHandAttachedInfo.AttachedRigidbodyUsedGravity = attachedToOtherInfo.AttachedRigidbodyUsedGravity;
                            vrHandAttachedInfo.OriginalParent = attachedToOtherInfo.OriginalParent;
                        }
                    }
                }
                else
                {
                    vrHandAttachedInfo.AttachedRigidbodyWasKinematic = vrHandAttachedInfo.AttachedRigidbody.isKinematic;
                    vrHandAttachedInfo.AttachedRigidbodyUsedGravity = vrHandAttachedInfo.AttachedRigidbody.useGravity;
                }
            }

            vrHandAttachedInfo.GrabbedWithType = grabbedWithType;

            if (vrHandAttachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.ParentToHand))
            {
                attachTransform.parent = vrHandTransform;
                vrHandAttachedInfo.IsParentedToHand = true;
            }
            else
            {
                vrHandAttachedInfo.IsParentedToHand = false;
            }

            if (vrHandAttachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.SnapOnAttach))
            {
                bool hasSkeleonPoser = false;
                if (vrHandAttachedInfo.AttachedEntity.HasComponent<SteamVRInteractable>())
                {
                    var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);
                    if (vrHandRenderModelEntity.HasComponent<SteamVRHandRenderModel>())
                    {
                        var steamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                        if (steamVRHandRenderModel.Skeleton != null)
                        {
                            var steamVRInteractable = vrHandAttachedInfo.AttachedEntity.GetComponent<SteamVRInteractable>();
                            if (steamVRInteractable.SkeletonPoser != null)
                            {
                                SteamVR_Skeleton_PoseSnapshot pose = steamVRInteractable.SkeletonPoser.GetBlendedPose(steamVRHandRenderModel.Skeleton);

                                //snap the object to the center of the attach point
                                attachView.transform.position = vrHandView.transform.TransformPoint(pose.position);
                                attachView.transform.rotation = vrHandView.transform.rotation * pose.rotation;

                                vrHandAttachedInfo.InitialPositionalOffset = vrHandAttachedInfo.HandAttachmentPointTransform.InverseTransformPoint(attachView.transform.position);
                                vrHandAttachedInfo.InitialRotationalOffset = Quaternion.Inverse(vrHandAttachedInfo.HandAttachmentPointTransform.rotation) * attachView.transform.rotation;

                                hasSkeleonPoser = true;
                            }
                        }
                    }
                }

                if (hasSkeleonPoser == false)
                {
                    if (attachmentOffset != null)
                    {
                        //offset the object from the hand by the positional and rotational difference between the offset transform and the attached object
                        Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * attachView.transform.rotation;
                        attachView.transform.rotation = vrHandAttachedInfo.HandAttachmentPointTransform.rotation * rotDiff;

                        Vector3 posDiff = attachView.transform.position - attachmentOffset.transform.position;
                        attachView.transform.position = vrHandAttachedInfo.HandAttachmentPointTransform.position + posDiff;
                    }
                    else
                    {
                        //snap the object to the center of the attach point
                        attachView.transform.rotation = vrHandAttachedInfo.HandAttachmentPointTransform.rotation;
                        attachView.transform.position = vrHandAttachedInfo.HandAttachmentPointTransform.position;
                    }

                    Transform followPoint = attachView.transform;

                    vrHandAttachedInfo.InitialPositionalOffset = vrHandAttachedInfo.HandAttachmentPointTransform.InverseTransformPoint(followPoint.position);
                    vrHandAttachedInfo.InitialRotationalOffset = Quaternion.Inverse(vrHandAttachedInfo.HandAttachmentPointTransform.rotation) * followPoint.rotation;
                }
            }
            else
            {
                bool hasSkeleonPoser = false;
                if (vrHandAttachedInfo.AttachedEntity.HasComponent<SteamVRInteractable>())
                {
                    var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);
                    if (vrHandRenderModelEntity.HasComponent<SteamVRHandRenderModel>())
                    {
                        var steamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                        if (steamVRHandRenderModel.Skeleton != null)
                        {
                            var steamVRInteractable = vrHandAttachedInfo.AttachedEntity.GetComponent<SteamVRInteractable>();
                            if (steamVRInteractable.SkeletonPoser != null)
                            {
                                vrHandAttachedInfo.InitialPositionalOffset = vrHandAttachedInfo.HandAttachmentPointTransform.InverseTransformPoint(attachView.transform.position);
                                vrHandAttachedInfo.InitialRotationalOffset = Quaternion.Inverse(vrHandAttachedInfo.HandAttachmentPointTransform.rotation) * attachView.transform.rotation;

                                hasSkeleonPoser = true;
                            }
                        }
                    }
                }

                if (hasSkeleonPoser == false)
                {
                    if (attachmentOffset != null)
                    {
                        //get the initial positional and rotational offsets between the hand and the offset transform
                        Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * attachView.transform.rotation;
                        Quaternion targetRotation = vrHandAttachedInfo.HandAttachmentPointTransform.rotation * rotDiff;
                        Quaternion rotationPositionBy = targetRotation * Quaternion.Inverse(attachView.transform.rotation);

                        Vector3 posDiff = (rotationPositionBy * attachView.transform.position) - (rotationPositionBy * attachmentOffset.transform.position);

                        vrHandAttachedInfo.InitialPositionalOffset = vrHandAttachedInfo.HandAttachmentPointTransform.InverseTransformPoint(vrHandAttachedInfo.HandAttachmentPointTransform.position + posDiff);
                        vrHandAttachedInfo.InitialRotationalOffset = Quaternion.Inverse(vrHandAttachedInfo.HandAttachmentPointTransform.rotation) * (vrHandAttachedInfo.HandAttachmentPointTransform.rotation * rotDiff);
                    }
                    else
                    {
                        vrHandAttachedInfo.InitialPositionalOffset = vrHandAttachedInfo.HandAttachmentPointTransform.InverseTransformPoint(attachView.transform.position);
                        vrHandAttachedInfo.InitialRotationalOffset = Quaternion.Inverse(vrHandAttachedInfo.HandAttachmentPointTransform.rotation) * attachView.transform.rotation;
                    }
                }
            }

            if (vrHandAttachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.TurnOnKinematic))
            {
                if (vrHandAttachedInfo.AttachedRigidbody != null)
                {
                    vrHandAttachedInfo.CollisionDetectionMode = vrHandAttachedInfo.AttachedRigidbody.collisionDetectionMode;
                    if (vrHandAttachedInfo.CollisionDetectionMode == CollisionDetectionMode.Continuous)
                        vrHandAttachedInfo.AttachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

                    vrHandAttachedInfo.AttachedRigidbody.isKinematic = true;
                }
            }

            if (vrHandAttachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.TurnOffGravity))
            {
                if (vrHandAttachedInfo.AttachedRigidbody != null)
                {
                    vrHandAttachedInfo.AttachedRigidbody.useGravity = false;
                }
            }

            // ckbang - ease in
            //if (attachedObject.interactable != null && attachedObject.interactable.attachEaseIn)
            //{
            //    attachedObject.attachedObject.transform.position = attachedObject.easeSourcePosition;
            //    attachedObject.attachedObject.transform.rotation = attachedObject.easeSourceRotation;
            //}

            var pool = entityDatabase.GetCollection();
            var vrHandAttachedInfoEntity = pool.CreateEntity();
            vrHandAttachedInfoEntity.AddComponent(vrHandAttachedInfo);

            vrInterface.UpdateHandHovering(vrHandEntity);

            //if (spewDebugText)
            //    HandDebugLog("AttachObject " + objectToAttach);
            //objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);

            eventSystem.Publish(new OnAttachedToHandEvent() { HandEntity = vrHandEntity });
        }

        public IEntity GetLastAttachedInfoEntity(IEntity vrHandEntity)
        {
            var entities = entityDatabase.GetEntitiesFor(new Group(typeof(InterVRHandAttachedInfo)), 0).Where(x => x.GetComponent<InterVRHandAttachedInfo>().AttachedHandEntity.Id == vrHandEntity.Id);
            if (entities.Count() > 0)
            {
                return entities.Last();
            }

            return null;
        }

        public IEnumerable<IEntity> GetAttachedInfoEntities(IEntity vrHandEntity)
        {
            return entityDatabase.GetEntitiesFor(new Group(typeof(InterVRHandAttachedInfo)), 0).Where(x => x.GetComponent<InterVRHandAttachedInfo>().AttachedHandEntity.Id == vrHandEntity.Id);
        }

        bool objectIsAttached(IEntity vrHandEntity, IEntity attachEntity)
        {
            var attachedInfoEntities = GetAttachedInfoEntities(vrHandEntity);
            foreach (var attachedInfoEntity in attachedInfoEntities)
            {
                var attachedInfo = attachedInfoEntity.GetComponent<InterVRHandAttachedInfo>();
                if (attachedInfo.AttachedEntity != null && attachedInfo.AttachedEntity.Id == attachEntity.Id)
                    return true;
            }
            return false;
        }

        public IEntity GetAttachedInfoEntity(IEntity attachedEntity)
        {
            var attachedInfoEntities = entityDatabase.GetEntitiesFor(new Group(typeof(InterVRHandAttachedInfo)), 0);
            foreach (var attachedInfoEntity in attachedInfoEntities)
            {
                var attachedInfo = attachedInfoEntity.GetComponent<InterVRHandAttachedInfo>();
                if (attachedInfo.AttachedEntity.Id == attachedEntity.Id)
                    return attachedInfoEntity;
            }

            return null;
        }

        public void DetachFromHand(IEntity handEntity, IEntity detachEntity, bool restoreOriginParent = true)
        {
            var vrHand = handEntity.GetComponent<InterVRHand>();
            var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);

            var vrGloveEntity = vrInterface.GetGloveEntity(vrHand.Type);
            if (vrGloveEntity != null)
            {
                var vrGlove = vrGloveEntity.GetComponent<InterVRGlove>();
                vrGlove.SetVisibility(true);

                var vrHandRenderModel = vrHandRenderModelEntity.GetComponent<InterVRHandRenderModel>();
                vrHandRenderModel.SetVisibility(false);
            }

            var attachedInfoEntity = GetAttachedInfoEntity(detachEntity);
            if (attachedInfoEntity != null)
            {
                var attachedInfo = attachedInfoEntity.GetComponent<InterVRHandAttachedInfo>();
                var attachEntity = attachedInfo.AttachedEntity;
                var attachView = attachEntity.GetGameObject();
                //Debug.Log("DetachObject " + attachView);

                //GameObject prevTopObject = currentAttachedObject;

                //if (attachedObjects[index].interactable != null)
                //{
                //    if (attachedObjects[index].interactable.hideHandOnAttach)
                //        Show();

                //    if (attachedObjects[index].interactable.hideSkeletonOnAttach && mainRenderModel != null && mainRenderModel.displayHandByDefault)
                //        ShowSkeleton();

                //    if (attachedObjects[index].interactable.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
                //        ShowController();

                //    if (attachedObjects[index].interactable.handAnimationOnPickup != 0)
                //        StopAnimation();

                //    if (attachedObjects[index].interactable.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                //        ResetTemporarySkeletonRangeOfMotion();
                //}

                if (attachedInfo.IsParentedToHand)
                {
                    if (restoreOriginParent && (attachedInfo.OriginalParent != null))
                    {
                        attachView.transform.parent = attachedInfo.OriginalParent.transform;
                    }
                    else
                    {
                        attachView.transform.parent = null;
                    }
                }

                if (attachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.TurnOnKinematic))
                {
                    if (attachedInfo.AttachedRigidbody != null)
                    {
                        attachedInfo.AttachedRigidbody.isKinematic = attachedInfo.AttachedRigidbodyWasKinematic;
                        attachedInfo.AttachedRigidbody.collisionDetectionMode = attachedInfo.CollisionDetectionMode;
                    }
                }

                if (attachedInfo.AttachmentFlags.HasFlag(HandAttachmentFlags.TurnOffGravity))
                {
                    if (attachedInfo.AttachedRigidbody != null)
                    {
                        attachedInfo.AttachedRigidbody.useGravity = attachedInfo.AttachedRigidbodyUsedGravity;
                    }
                }

                if (attachEntity.HasComponent<InterVRInteractable>())
                {
                    var vrInteractable = attachEntity.GetComponent<InterVRInteractable>();
                    if (vrInteractable.HandFollowTransform)
                    {
                        if (vrHandRenderModelEntity.HasComponent<SteamVRHandRenderModel>())
                        {
                            var steamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                            if (steamVRHandRenderModel.Skeleton != null)
                            {
                                steamVRHandRenderModel.Skeleton.transform.localPosition = Vector3.zero;
                                steamVRHandRenderModel.Skeleton.transform.localRotation = Quaternion.identity;
                            }
                        }
                    }
                }

                if (attachEntity.HasComponent<InterVRInteractable>() == false ||
                    (attachEntity.HasComponent<InterVRInteractable>() && attachEntity.GetComponent<InterVRInteractable>().IsDestroying == false))
                {
                    attachView.SetActive(true);
                }

                //attachedObjects[index].attachedGameObject.SendMessage("OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver);
                eventSystem.Publish(new OnDetachedToHandEvent() { HandEntity = handEntity });

                //CleanUpAttachedObjectStack();

                entityDatabase.RemoveEntity(attachedInfoEntity);

                vrHand.HoverLocked = false;

                //if (newTopObject != null && newTopObject != prevTopObject)
                //{
                //    newTopObject.SetActive(true);
                //    newTopObject.SendMessage("OnHandFocusAcquired", this, SendMessageOptions.DontRequireReceiver);
                //}
            }

            //CleanUpAttachedObjectStack();

            if (vrHandRenderModelEntity.HasComponent<SteamVRHandRenderModel>())
            {
                var vrSteamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                var vrHandRenderModelView = vrHandRenderModelEntity.GetGameObject();
                vrSteamVRHandRenderModel.MatchHandToTransform(vrHandRenderModelView.transform);
            }
        }
    }
}