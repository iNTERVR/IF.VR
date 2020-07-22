using Inter.VR.Defines;
using EcsRx.Entities;
using System;
using UniRx;
using UnityEngine;
using EcsRx.Collections.Database;
using EcsRx.Groups;
using Inter.VR.Components;
using EcsRx.Extensions;
using EcsRx.Unity.Extensions;
using System.Linq;
using EcsRx.Events;
using Inter.VR.Events;
using EcsRx.Unity.MonoBehaviours;

namespace Inter.VR.Modules.InterVRInterfaces
{
    public class InterVRInterface : IInterVRInterface, IDisposable
    {
        private readonly IEntityDatabase entityDatabase;
        private readonly IEventSystem eventSystem;
        InterVRRigType currentRigType;

        public InterVRInterface(IEntityDatabase entityDatabase,
            IEventSystem eventSystem)
        {
            this.entityDatabase = entityDatabase;
            this.eventSystem = eventSystem;
            HeadsetOnHead = new BoolReactiveProperty(false);
        }

        public void Dispose()
        {
            HeadsetOnHead.Dispose();
        }

        public BoolReactiveProperty HeadsetOnHead { get; private set; }

        public int HandCount
        {
            get
            {
                return entityDatabase.GetEntitiesFor(new Group(typeof(InterVRHand)), 0).Count();
            }
        }

        public Transform HMDRootTransform
        {
            get
            {
                var rigEntity = GetRigEntity();
                if (rigEntity == null)
                    return null;

                var rig = rigEntity.GetComponent<InterVRRig>();
                if (rig == null)
                    return null;

                if (CurrentRigType == InterVRRigType.VR)
                    return rig.HMDRoot;

                return null;
            }
        }

        public Transform HMDTransform
        {
            get
            {
                var rigEntity = GetRigEntity();
                if (rigEntity == null)
                    return null;

                var rig = rigEntity.GetComponent<InterVRRig>();
                if (rig == null)
                    return null;

                if (CurrentRigType == InterVRRigType.VR)
                    return rig.HMD.transform;

                return rig.HMDFallback.transform;
            }
        }

        public float EyeHeight
        {
            get
            {
                var rigEntity = GetRigEntity();
                if (rigEntity == null)
                    return 0.0f;

                var rigView = rigEntity.GetGameObject();
                Transform trackingOriginTransform = rigView.transform;
                Transform hmd = HMDTransform;
                if (hmd && trackingOriginTransform)
                {
                    Vector3 eyeOffset = Vector3.Project(hmd.position - trackingOriginTransform.position, trackingOriginTransform.up);
                    return eyeOffset.magnitude / trackingOriginTransform.lossyScale.x;
                }
                return 0.0f;
            }
        }

        public Vector3 FeetPosition
        {
            get
            {
                var rigEntity = GetRigEntity();
                if (rigEntity == null)
                    return Vector3.zero;

                var rigView = rigEntity.GetGameObject();
                Transform trackingOriginTransform = rigView.transform;
                Transform hmd = HMDTransform;
                if (hmd && trackingOriginTransform)
                {
                    return trackingOriginTransform.position + Vector3.ProjectOnPlane(hmd.position - trackingOriginTransform.position, trackingOriginTransform.up);
                }
                return trackingOriginTransform.position;
            }
        }

        public Vector3 BodyDirection
        {
            get
            {
                var rigEntity = GetRigEntity();
                if (rigEntity == null)
                    return Vector3.zero;

                var rigView = rigEntity.GetGameObject();
                Transform trackingOriginTransform = rigView.transform;
                Transform hmd = HMDTransform;
                if (hmd && trackingOriginTransform)
                {
                    Vector3 direction = Vector3.ProjectOnPlane(hmd.forward, trackingOriginTransform.up);
                    if (Vector3.Dot(hmd.up, trackingOriginTransform.up) < 0.0f)
                    {
                        // The HMD is upside-down. Either
                        // -The player is bending over backwards
                        // -The player is bent over looking through their legs
                        direction = -direction;
                    }
                    return direction;
                }
                return trackingOriginTransform.forward;
            }
        }

        public Collider HeadCollider
        {
            get
            {
                var rigEntity = GetRigEntity();
                if (rigEntity == null)
                    return null;

                var rig = rigEntity.GetComponent<InterVRRig>();
                if (rig == null)
                    return null;

                return rig.HeadCollider;
            }
        }

        public InterVRRigType CurrentRigType
        {
            get
            {
                return currentRigType;
            }
            set
            {
                currentRigType = value;

                var rigEntity = GetRigEntity();
                if (rigEntity != null)
                {
                    var rig = rigEntity.GetComponent<InterVRRig>();
                    if (rig != null)
                    {
                        if (currentRigType == InterVRRigType.Fallback)
                        {
                            rig.HMD.SetActive(false);
                            rig.HMDFallback.SetActive(true);
                        }
                        else
                        {
                            rig.HMD.SetActive(true);
                            rig.HMDFallback.SetActive(false);
                        }
                    }
                }
            }
        }

        public IEntity GetRigEntity()
        {
            return entityDatabase.GetEntitiesFor(new Group(typeof(InterVRRig)), 0).FirstOrDefault();
        }

        public IEntity GetHandTrackerEntity(InterVRHandType type)
        {
            var handTrackerEntities = entityDatabase.GetEntitiesFor(new Group(typeof(InterVRHandTracker)), 0);
            foreach (var handTrackerEntity in handTrackerEntities)
            {
                var handTracker = handTrackerEntity.GetComponent<InterVRHandTracker>();
                if (handTracker.Type == type)
                    return handTrackerEntity;
            }
            return null;
        }

        public IEntity GetHandEntity(InterVRHandType type)
        {
            var handEntities = entityDatabase.GetEntitiesFor(new Group(typeof(InterVRHand)), 0);
            foreach (var handEntity in handEntities)
            {
                var hand = handEntity.GetComponent<InterVRHand>();
                if (hand.Type == type)
                    return handEntity;
            }
            return null;
        }

        public IEntity GetHandRenderModelEntity(InterVRHandType type)
        {
            var handRenderModelEntities = entityDatabase.GetEntitiesFor(new Group(typeof(InterVRHandRenderModel)), 0);
            foreach (var handRenderModelEntity in handRenderModelEntities)
            {
                var handRenderModel = handRenderModelEntity.GetComponent<InterVRHandRenderModel>();
                if (handRenderModel.Type == type)
                    return handRenderModelEntity;
            }
            return null;
        }

        public IEntity GetHandControllerRenderModelEntity(InterVRHandType type)
        {
            var handControllerRenderModelEntities = entityDatabase.GetEntitiesFor(new Group(typeof(InterVRHandControllerRenderModel)), 0);
            foreach (var handControllerRenderModelEntity in handControllerRenderModelEntities)
            {
                var handControllerRenderModel = handControllerRenderModelEntity.GetComponent<InterVRHandControllerRenderModel>();
                if (handControllerRenderModel.Type == type)
                    return handControllerRenderModelEntity;
            }
            return null;
        }

        public IEntity GetGloveEntity(InterVRHandType type)
        {
            var gloveEntities = entityDatabase.GetEntitiesFor(new Group(typeof(InterVRGlove)), 0);
            foreach (var gloveEntity in gloveEntities)
            {
                var glove = gloveEntity.GetComponent<InterVRGlove>();
                if (glove.Type == type)
                    return gloveEntity;
            }
            return null;
        }

        public IEntity GetCameraEntity()
        {
            return entityDatabase.GetEntitiesFor(new Group(typeof(InterVRCamera)), 0).FirstOrDefault();
        }

        bool checkHoveringForTransform(IEntity entity, Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref IEntity closestInteractableEntity, Color debugColor)
        {
            var vrHand = entity.GetComponent<InterVRHand>();

            bool foundCloser = false;

            // null out old vals
            for (int i = 0; i < vrHand.OverlappingColliders.Length; ++i)
            {
                vrHand.OverlappingColliders[i] = null;
            }

            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, vrHand.OverlappingColliders, vrHand.HoverLayerMask.value);

            if (numColliding >= InterVRHand.ColliderArraySize)
                Debug.LogWarning("<b>[SteamVR Interaction]</b> This hand is overlapping the max number of colliders: " + InterVRHand.ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on Hand.cs");

            // DebugVar
            int iActualColliderCount = 0;

            // Pick the closest hovering
            for (int colliderIndex = 0; colliderIndex < vrHand.OverlappingColliders.Length; colliderIndex++)
            {
                Collider collider = vrHand.OverlappingColliders[colliderIndex];

                if (collider == null)
                    continue;

                var entityView = collider.GetComponentInParent<EntityView>();
                if (entityView == null)
                    continue;
                if (!entityView.Entity.HasComponent<InterVRInteractable>())
                    continue;

                var contactingEntity = entityView.Entity;
                var contactingView = contactingEntity.GetGameObject();
                var contacting = contactingEntity.GetComponent<InterVRInteractable>();

                // Ignore this collider for hovering
                if (contacting.IgnoreHandHovering != IgnoreHandHoveringType.None)
                {
                    if (contacting.IgnoreHandHovering == IgnoreHandHoveringType.Both ||
                        contacting.IgnoreHandHovering == (vrHand.Type == InterVRHandType.Left ? IgnoreHandHoveringType.Left : IgnoreHandHoveringType.Right))
                        continue;
                }

                // Can't hover over the object if it's attached
                bool hoveringOverAttached = false;

                var attachedInfoEntities = entityDatabase.GetEntitiesFor(new Group(typeof(InterVRHandAttachedInfo)), 0);
                foreach (var attachedInfoEntity in attachedInfoEntities)
                {
                    var attachedInfo = attachedInfoEntity.GetComponent<InterVRHandAttachedInfo>();
                    if (attachedInfo.AttachedEntity.Id == contactingEntity.Id)
                    {
                        hoveringOverAttached = true;
                        break;
                    }
                }

                if (hoveringOverAttached)
                    continue;

                // Best candidate so far...
                var contactingPosition = contactingView.transform.position;
                float distance = Vector3.Distance(contactingPosition, hoverPosition);
                bool lowerPriority = false;

                if (closestInteractableEntity != null)
                {
                    var closestInteractable = closestInteractableEntity.GetComponent<InterVRInteractable>();
                    // compare to closest interactable to check priority
                    lowerPriority = contacting.HoverPriority < closestInteractable.HoverPriority;
                }

                bool isCloser = (distance < closestDistance);
                if (isCloser && !lowerPriority)
                {
                    closestDistance = distance;
                    closestInteractableEntity = contactingEntity;
                    foundCloser = true;
                }
                iActualColliderCount++;
            }

            if (/*showDebugInteractables &&*/ foundCloser)
            {
                var closestInteractableView = closestInteractableEntity.GetGameObject();
                Debug.DrawLine(hoverPosition, closestInteractableView.transform.position, debugColor, .05f, false);
            }

            if (iActualColliderCount > 0 && iActualColliderCount != vrHand.PrevOverlappingColliders)
            {
                vrHand.PrevOverlappingColliders = iActualColliderCount;

                //if (spewDebugText)
                //    HandDebugLog("Found " + iActualColliderCount + " overlapping colliders.");
            }

            return foundCloser;
        }

        void setHoveringInteractable(IEntity entity, IEntity hoveringEntity)
        {
            var vrHand = entity.GetComponent<InterVRHand>();
            if (vrHand.HoveringInteractableEntity == null && hoveringEntity == null)
                return;

            if (vrHand.HoveringInteractableEntity != null && hoveringEntity != null)
            {
                if (vrHand.HoveringInteractableEntity.Id == hoveringEntity.Id)
                    return;
            }

            if (vrHand.HoveringInteractableEntity != null)
            {
                eventSystem.Publish(new OnHandHoverEndEvent() { HandEntity = entity });
            }

            vrHand.HoveringInteractableEntity = hoveringEntity;

            if (vrHand.HoveringInteractableEntity != null)
            {
                eventSystem.Publish(new OnHandHoverBeginEvent() { HandEntity = entity });
            }
        }

        public void UpdateHandHovering(IEntity handEntity)
        {
            var vrHandView = handEntity.GetGameObject();
            if (vrHandView == null)
                return;

            var vrHand = handEntity.GetComponent<InterVRHand>();
            if (vrHand.HoverLocked)
                return;

            if (vrHand.ApplicationLostFocusEntity != null)
            {
                var lostView = vrHand.ApplicationLostFocusEntity.GetGameObject();
                if (lostView != null && lostView.activeSelf)
                    return;
            }

            float closestDistance = float.MaxValue;
            IEntity closestInteractableEntity = null;

            float scaledHoverRadius = vrHand.HoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(vrHand.HoverSphereTransform));
            checkHoveringForTransform(handEntity, vrHand.HoverSphereTransform.position, scaledHoverRadius, ref closestDistance, ref closestInteractableEntity, Color.green);

            setHoveringInteractable(handEntity, closestInteractableEntity);
        }

        public void HandHoverLock(IEntity handEntity, IEntity interactableEntity)
        {
            var vrHand = handEntity.GetComponent<InterVRHand>();
            vrHand.HoverLocked = true;
            vrHand.HoveringInteractableEntity = interactableEntity;
        }

        public void HandHoverUnlock(IEntity handEntity, IEntity interactableEntity)
        {
            var vrHand = handEntity.GetComponent<InterVRHand>();
            if (vrHand.HoveringInteractableEntity.Id == interactableEntity.Id)
            {
                vrHand.HoverLocked = false;
            }
        }

        public void HandHoverUnlockForce(IEntity handEntity)
        {
            var vrHand = handEntity.GetComponent<InterVRHand>();
            vrHand.HoverLocked = false;
        }
    }
}