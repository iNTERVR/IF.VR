using EcsRx.Components;
using EcsRx.Entities;
using InterVR.IF.VR.Defines;
using UnityEngine;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_HandAttachedInfo : IComponent
    {
        public IEntity AttachedHandEntity { get; set; }
        public IEntity AttachedEntity { get; set; }
        public Rigidbody AttachedRigidbody { get; set; }
        public CollisionDetectionMode CollisionDetectionMode { get; set; }
        public bool AttachedRigidbodyWasKinematic { get; set; }
        public bool AttachedRigidbodyUsedGravity { get; set; }
        public GameObject OriginalParent { get; set; }
        public bool IsParentedToHand { get; set; }
        public IF_VR_GrabType GrabbedWithType { get; set; }
        public IF_VR_HandAttachmentFlags AttachmentFlags { get; set; }
        public Vector3 InitialPositionalOffset { get; set; }
        public Quaternion InitialRotationalOffset { get; set; }
        public Transform AttachedOffsetTransform { get; set; }
        public Transform HandAttachmentPointTransform { get; set; }
        public Vector3 EaseSourcePosition { get; set; }
        public Quaternion EaseSourceRotation { get; set; }
        public float AttachTime { get; set; }
    }
}