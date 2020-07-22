using Inter.VR.Components;
using Inter.VR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;

namespace Inter.VR.Modules.InterVRInterfaces
{
    public interface IInterVRHandGrabStatus
    {
        HandGrabTypes GetGrabStarting(IEntity handEntity, HandGrabTypes explicitType = HandGrabTypes.None);
        bool IsGrabEnding(IEntity handEntity, IEntity interactableEntity);
        bool IsGrabbingWithType(IEntity handEntity, HandGrabTypes type);
    }
}