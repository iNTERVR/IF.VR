using Inter.VR.Components;
using Inter.VR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;

namespace Inter.VR.Modules.InterVRInterfaces
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