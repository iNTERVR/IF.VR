using Inter.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using System;
using UniRx;
using UnityEngine;
using System.Collections.Generic;
using EcsRx.Unity.Extensions;

namespace Inter.VR.VRGlovePrototype.Components
{
    public class VRGlovePrototypeInteractable : IComponent
    {
        public TextMesh GeneralText;
        public TextMesh HoveringText;
        public Vector3 OldPosition;
        public Quaternion OldRotation;
        public float AttachTime;
        public HandAttachmentFlags AttachmentFlags;
        public bool LastHovering;
    }

    public class VRGlovePrototypeInteractableComponent : MonoBehaviour, IConvertToEntity
    {
        public TextMesh GeneralText;
        public TextMesh HoveringText;
        public HandAttachmentFlags AttachmentFlags = HandAttachmentFlags.Default & (~HandAttachmentFlags.SnapOnAttach) & (~HandAttachmentFlags.DetachOthers) & (~HandAttachmentFlags.VelocityMovement);

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new VRGlovePrototypeInteractable() : component as VRGlovePrototypeInteractable;

            c.GeneralText = GeneralText;
            c.HoveringText = HoveringText;
            c.AttachmentFlags = AttachmentFlags;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}