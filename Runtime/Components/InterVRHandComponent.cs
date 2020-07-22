using Inter.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using System;
using UniRx;
using UnityEngine;
using System.Collections.Generic;
using EcsRx.Unity.Extensions;

namespace Inter.VR.Components
{
    public class InterVRHand : IComponent, IDisposable
    {
        public InterVRHandType Type { get; set; }
        public LayerMask HoverLayerMask { get; set; }
        public BoolReactiveProperty Active { get; set; }
        public Transform HoverSphereTransform { get; set; }
        public float HoverSphereRadius { get; set; }
        public float HoverUpdateInterval { get; set; }
        public Transform ObjectAttachmentPoint { get; set; }

        public bool HoverLocked;
        public IEntity HoveringInteractableEntity;
        public IEntity ApplicationLostFocusEntity;

        public const int ColliderArraySize = 32;
        public Collider[] OverlappingColliders;
        public int PrevOverlappingColliders;

        public InterVRHand()
        {
            OverlappingColliders = new Collider[ColliderArraySize];
            PrevOverlappingColliders = 0;
            Active = new BoolReactiveProperty(false);
        }

        public void Dispose()
        {
            Active.Dispose();
        }
    }

    public class InterVRHandComponent : MonoBehaviour, IConvertToEntity
    {
        public InterVRHandType Type;
        public LayerMask HoverLayerMask = -1;
        public Transform HoverSphereTransform;
        public float HoverSphereRadius = 0.05f;
        public float HoverUpdateInterval = 0.1f;
        [Tooltip("A transform on the hand to center attached objects on")]
        public Transform ObjectAttachmentPoint;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new InterVRHand() : component as InterVRHand;

            if (this.gameObject.layer == 0)
                Debug.LogWarning("Hand is on default layer. This puts unnecessary strain on hover checks as it is always true for hand colliders (which are then ignored).", this);
            else
                HoverLayerMask &= ~(1 << this.gameObject.layer); //ignore self for hovering

            c.Type = Type;
            c.HoverLayerMask = HoverLayerMask;
            c.HoverSphereTransform = HoverSphereTransform == null ? this.transform : HoverSphereTransform;
            c.ObjectAttachmentPoint = ObjectAttachmentPoint == null ? this.transform : ObjectAttachmentPoint;
            c.HoverSphereRadius = HoverSphereRadius;
            c.HoverUpdateInterval = HoverUpdateInterval;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}