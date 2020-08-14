using InterVR.Unity.SDK.SteamVR.Components;
using InterVR.Unity.SDK.SteamVR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;

namespace InterVR.Unity.SDK.SteamVR.Modules.InterVRInterfaces
{
    public interface IInterVRHandGrabStatus
    {
        HandGrabTypes GetGrabStarting(IEntity handEntity, HandGrabTypes explicitType = HandGrabTypes.None);
        bool IsGrabEnding(IEntity handEntity, IEntity interactableEntity);
        bool IsGrabbingWithType(IEntity handEntity, HandGrabTypes type);
    }
}