using InterVR.IF.VR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;
using InterVR.IF.VR.Components;

namespace InterVR.IF.VR.Modules
{
    public interface IF_VR_IGrabStatus
    {
        IF_VR_GrabType GetGrabStarting(IEntity handEntity, IF_VR_GrabType explicitType = IF_VR_GrabType.None);
        IF_VR_GrabType GetGrabEnding(IEntity handEntity, IF_VR_GrabType explicitType = IF_VR_GrabType.None);
        bool IsGrabbingWithType(IEntity handEntity, IF_VR_GrabType type);
        bool IsGrabbingWithOppositeType(IEntity handEntity, IF_VR_GrabType type);
        IF_VR_GrabType GetBestGrabbingType(IEntity handEntity, IF_VR_GrabType preferred, bool forcePreference = false);
    }
}