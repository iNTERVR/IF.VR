using InterVR.Unity.SDK.SteamVR.Components;
using InterVR.Unity.SDK.SteamVR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;

namespace InterVR.Unity.SDK.SteamVR.Modules.InterVRInterfaces
{
    public interface IInterVRGloveInterface
    {
        FloatReactiveProperty HandYawOffsetLeft { get; }
        FloatReactiveProperty HandYawOffsetRight { get; }
        int PlayerNumber { get; }
        
        Transform GetRootTransform();
        void SetRootTransform(Transform root);
        bool GetGrabStateDown(InterVRHandType handType);
        bool GetGrabState(InterVRHandType handType);
        bool GetGrabStateUp(InterVRHandType handType);
    }
}