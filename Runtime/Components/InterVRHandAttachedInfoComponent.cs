using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using Inter.VR.Defines;
using UnityEngine;

namespace Inter.VR.Components
{
    public class InterVRHandAttachedInfo : IComponent
    {
        public IEntity AttachedHandEntity { get; set; }
        public IEntity AttachedEntity { get; set; }
        public Rigidbody AttachedRigidbody { get; set; }
        public CollisionDetectionMode CollisionDetectionMode { get; set; }
        public bool AttachedRigidbodyWasKinematic { get; set; }
        public bool AttachedRigidbodyUsedGravity { get; set; }
        public GameObject OriginalParent { get; set; }
        public bool IsParentedToHand { get; set; }
        public HandGrabTypes GrabbedWithType { get; set; }
        public HandAttachmentFlags AttachmentFlags { get; set; }
        public Vector3 InitialPositionalOffset { get; set; }
        public Quaternion InitialRotationalOffset { get; set; }
        public Transform AttachedOffsetTransform { get; set; }
        public Transform HandAttachmentPointTransform { get; set; }
        public Vector3 EaseSourcePosition { get; set; }
        public Quaternion EaseSourceRotation { get; set; }
        public float AttachTime { get; set; }
    }
}