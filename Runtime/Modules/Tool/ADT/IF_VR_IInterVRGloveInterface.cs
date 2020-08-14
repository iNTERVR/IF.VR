using InterVR.IF.VR.Defines;
using UniRx;
using UnityEngine;

namespace InterVR.IF.VR.Modules
{
    public interface IF_VR_IInterVRGloveInterface
    {
        FloatReactiveProperty HandYawOffsetLeft { get; }
        FloatReactiveProperty HandYawOffsetRight { get; }
        int PlayerNumber { get; }
        
        Transform GetRootTransform();
        void SetRootTransform(Transform root);
        bool GetGrabStateDown(IF_VR_HandType handType);
        bool GetGrabState(IF_VR_HandType handType);
        bool GetGrabStateUp(IF_VR_HandType handType);
    }
}