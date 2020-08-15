using InterVR.IF.VR.Defines;
using EcsRx.Entities;
using UnityEngine;
using System.Collections.Generic;

namespace InterVR.IF.VR.Modules
{
    public interface IF_VR_IInterVRHandInterface
    {
        void AttachToHand(IEntity vrHandEntity, IEntity attachEntity, IF_VR_GrabType grabbedWithType, IF_VR_HandAttachmentFlags flags = IF_VR_HandAttachmentFlags.Default, Transform attachmentOffset = null);
        void DetachFromHand(IEntity vrHandEntity, IEntity detachEntity, bool restoreOriginParent = true);
        IEntity GetLastAttachedInfoEntity(IEntity vrHandEntity);
        IEnumerable<IEntity> GetAttachedInfoEntities(IEntity vrHandEntity);
        IEntity GetAttachedInfoEntity(IEntity attachedEntity);
    }
}