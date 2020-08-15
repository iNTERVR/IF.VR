using InterVR.IF.VR.Defines;
using EcsRx.Entities;

namespace InterVR.IF.VR.Modules
{
    public interface IF_VR_IInterVRHandGrabStatus
    {
        IF_VR_GrabType GetGrabStarting(IEntity handEntity, IF_VR_GrabType explicitType = IF_VR_GrabType.None);
        bool IsGrabEnding(IEntity handEntity, IEntity interactableEntity);
        bool IsGrabbingWithType(IEntity handEntity, IF_VR_GrabType type);
    }
}