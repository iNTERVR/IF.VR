using InterVR.IF.VR.Defines;
using EcsRx.Entities;
using System;
using UniRx;
using UnityEngine;
using EcsRx.Collections.Database;
using EcsRx.Groups;
using InterVR.IF.VR.Components;
using EcsRx.Extensions;
using EcsRx.Unity.Extensions;
using System.Linq;
using EcsRx.Events;
using InterVR.IF.VR.Events;
using EcsRx.Unity.MonoBehaviours;

namespace InterVR.IF.VR.Modules
{
    public class IF_VR_Interface : IF_VR_IInterface, IDisposable
    {
        private readonly IEntityDatabase entityDatabase;
        private readonly IEventSystem eventSystem;
        IF_VR_RigType currentRigType;

        public IF_VR_Interface(IEntityDatabase entityDatabase,
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
                return entityDatabase.GetEntitiesFor(new Group(typeof(IF_VR_Hand)), 0).Count();
            }
        }

        public Transform HMDRootTransform
        {
            get
            {
                var rigEntity = GetRigEntity();
                if (rigEntity == null)
                    return null;

                var rig = rigEntity.GetComponent<IF_VR_Rig>();
                if (rig == null)
                    return null;

                if (CurrentRigType == IF_VR_RigType.VR)
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

                var rig = rigEntity.GetComponent<IF_VR_Rig>();
                if (rig == null)
                    return null;

                if (CurrentRigType == IF_VR_RigType.VR)
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

                var rig = rigEntity.GetComponent<IF_VR_Rig>();
                if (rig == null)
                    return null;

                return rig.HeadCollider;
            }
        }

        public IF_VR_RigType CurrentRigType
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
                    var rig = rigEntity.GetComponent<IF_VR_Rig>();
                    if (rig != null)
                    {
                        if (currentRigType == IF_VR_RigType.Fallback)
                        {
                            rig.HMDRoot.gameObject.SetActive(false);
                            rig.HMDFallbackRoot.gameObject.SetActive(true);
                        }
                        else
                        {
                            rig.HMDRoot.gameObject.SetActive(true);
                            rig.HMDFallbackRoot.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        public IEntity GetRigEntity()
        {
            return entityDatabase.GetEntitiesFor(new Group(typeof(IF_VR_Rig)), 0).FirstOrDefault();
        }

        public IEntity GetHandTrackerEntity(IF_VR_HandType type)
        {
            var handTrackerEntities = entityDatabase.GetEntitiesFor(new Group(typeof(IF_VR_HandTracker)), 0);
            foreach (var handTrackerEntity in handTrackerEntities)
            {
                var handTracker = handTrackerEntity.GetComponent<IF_VR_HandTracker>();
                if (handTracker.Type == type)
                    return handTrackerEntity;
            }
            return null;
        }

        public IEntity GetHandEntity(IF_VR_HandType type)
        {
            var handEntities = entityDatabase.GetEntitiesFor(new Group(typeof(IF_VR_Hand)), 0);
            foreach (var handEntity in handEntities)
            {
                var hand = handEntity.GetComponent<IF_VR_Hand>();
                if (hand.Type == type)
                    return handEntity;
            }
            return null;
        }

        public IEntity GetHandRenderModelEntity(IF_VR_HandType type)
        {
            var handRenderModelEntities = entityDatabase.GetEntitiesFor(new Group(typeof(IF_VR_HandRenderModel)), 0);
            foreach (var handRenderModelEntity in handRenderModelEntities)
            {
                var handRenderModel = handRenderModelEntity.GetComponent<IF_VR_HandRenderModel>();
                if (handRenderModel.Type == type)
                    return handRenderModelEntity;
            }
            return null;
        }

        public IEntity GetHandControllerRenderModelEntity(IF_VR_HandType type)
        {
            var handControllerRenderModelEntities = entityDatabase.GetEntitiesFor(new Group(typeof(IF_VR_HandControllerRenderModel)), 0);
            foreach (var handControllerRenderModelEntity in handControllerRenderModelEntities)
            {
                var handControllerRenderModel = handControllerRenderModelEntity.GetComponent<IF_VR_HandControllerRenderModel>();
                if (handControllerRenderModel.Type == type)
                    return handControllerRenderModelEntity;
            }
            return null;
        }

        public IEntity GetCameraEntity()
        {
            return entityDatabase.GetEntitiesFor(new Group(typeof(IF_VR_Camera)), 0).FirstOrDefault();
        }

        bool checkHoveringForTransform(IEntity entity, Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref IEntity closestInteractableEntity, Color debugColor)
        {
            var vrHand = entity.GetComponent<IF_VR_Hand>();

            bool foundCloser = false;

            // null out old vals
            for (int i = 0; i < vrHand.OverlappingColliders.Length; ++i)
            {
                vrHand.OverlappingColliders[i] = null;
            }

            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, vrHand.OverlappingColliders, vrHand.HoverLayerMask.value);

            if (numColliding >= IF_VR_Hand.ColliderArraySize)
                Debug.LogWarning("<b>[SteamVR Interaction]</b> This hand is overlapping the max number of colliders: " + IF_VR_Hand.ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on Hand.cs");

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
                if (!entityView.Entity.HasComponent<IF_VR_Interactable>())
                    continue;

                var contactingEntity = entityView.Entity;
                var contactingView = contactingEntity.GetGameObject();
                var contacting = contactingEntity.GetComponent<IF_VR_Interactable>();

                // Ignore this collider for hovering
                if (contacting.IgnoreHandHovering != IF_VR_IgnoreHandHoveringType.None)
                {
                    if (contacting.IgnoreHandHovering == IF_VR_IgnoreHandHoveringType.Both ||
                        contacting.IgnoreHandHovering == (vrHand.Type == IF_VR_HandType.Left ? IF_VR_IgnoreHandHoveringType.Left : IF_VR_IgnoreHandHoveringType.Right))
                        continue;
                }

                // Can't hover over the object if it's attached
                bool hoveringOverAttached = false;

                var attachedInfoEntities = entityDatabase.GetEntitiesFor(new Group(typeof(IF_VR_HandAttachedInfo)), 0);
                foreach (var attachedInfoEntity in attachedInfoEntities)
                {
                    var attachedInfo = attachedInfoEntity.GetComponent<IF_VR_HandAttachedInfo>();
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
                    var closestInteractable = closestInteractableEntity.GetComponent<IF_VR_Interactable>();
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
            var vrHand = entity.GetComponent<IF_VR_Hand>();
            if (vrHand.HoveringInteractableEntity == null && hoveringEntity == null)
                return;

            if (vrHand.HoveringInteractableEntity != null && hoveringEntity != null)
            {
                if (vrHand.HoveringInteractableEntity.Id == hoveringEntity.Id)
                    return;
            }

            if (vrHand.HoveringInteractableEntity != null)
            {
                eventSystem.Publish(new IF_VR_OnHandHoverEndEvent() { HandEntity = entity, HoveringEntity = vrHand.HoveringInteractableEntity });
            }

            vrHand.HoveringInteractableEntity = hoveringEntity;

            if (vrHand.HoveringInteractableEntity != null)
            {
                eventSystem.Publish(new IF_VR_OnHandHoverBeginEvent() { HandEntity = entity, HoveringEntity = vrHand.HoveringInteractableEntity });
            }
        }

        public void UpdateHandHovering(IEntity handEntity)
        {
            var vrHandView = handEntity.GetGameObject();
            if (vrHandView == null)
                return;

            var vrHand = handEntity.GetComponent<IF_VR_Hand>();
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

            float scaledHoverRadius = vrHand.HoverSphereRadius * Mathf.Abs(vrHand.HoverSphereTransform.localScale.x);
            checkHoveringForTransform(handEntity, vrHand.HoverSphereTransform.position, scaledHoverRadius, ref closestDistance, ref closestInteractableEntity, Color.green);

            setHoveringInteractable(handEntity, closestInteractableEntity);
        }

        public void HandHoverLock(IEntity handEntity, IEntity interactableEntity)
        {
            var vrHand = handEntity.GetComponent<IF_VR_Hand>();
            vrHand.HoverLocked = true;
            vrHand.HoveringInteractableEntity = interactableEntity;
        }

        public void HandHoverUnlock(IEntity handEntity, IEntity interactableEntity)
        {
            var vrHand = handEntity.GetComponent<IF_VR_Hand>();
            if (vrHand.HoveringInteractableEntity.Id == interactableEntity.Id)
            {
                vrHand.HoverLocked = false;
            }
        }

        public void HandHoverUnlockForce(IEntity handEntity)
        {
            var vrHand = handEntity.GetComponent<IF_VR_Hand>();
            vrHand.HoverLocked = false;
        }
    }
}