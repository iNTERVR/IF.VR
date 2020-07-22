using Inter.VR.Components;
using Inter.VR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;
using System.Collections.Generic;

namespace Inter.VR.Modules.InterVRInterfaces
{
    public interface IInterVRHandInterface
    {
        void AttachToHand(IEntity vrHandEntity, IEntity attachEntity, HandGrabTypes grabbedWithType, HandAttachmentFlags flags = HandAttachmentFlags.Default, Transform attachmentOffset = null);
        void DetachFromHand(IEntity vrHandEntity, IEntity detachEntity, bool restoreOriginParent = true);
        IEntity GetLastAttachedInfoEntity(IEntity vrHandEntity);
        IEnumerable<IEntity> GetAttachedInfoEntities(IEntity vrHandEntity);
        IEntity GetAttachedInfoEntity(IEntity attachedEntity);
    }
}