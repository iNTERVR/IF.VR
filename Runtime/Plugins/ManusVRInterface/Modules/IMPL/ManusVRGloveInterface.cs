using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using ManusVR.Hands;
using System;
using UniRx;
using UnityEngine;

namespace Inter.VR.Plugins.ManusVRInterface.Modules
{
    public class ManusVRGloveInterface : IInterVRGloveInterface, IDisposable
    {
        int playerNumber = HandDataManager.invalidPlayerNumber;
        public int PlayerNumber
        {
            get
            {
                if (playerNumber == HandDataManager.invalidPlayerNumber)
                {
                    HandDataManager.GetPlayerNumber(out playerNumber);
                }

                return playerNumber;
            }
        }

        public FloatReactiveProperty HandYawOffsetLeft { get; private set; }
        public FloatReactiveProperty HandYawOffsetRight { get; private set; }

        public ManusVRGloveInterface()
        {
            HandYawOffsetLeft = new FloatReactiveProperty();
            HandYawOffsetRight = new FloatReactiveProperty();
        }

        Transform rootTransform;

        public Transform GetRootTransform()
        {
            return rootTransform;
        }

        public void SetRootTransform(Transform root)
        {
            rootTransform = root;
        }

        public bool GetGrabState(InterVRHandType handType)
        {
            var handData = ManusVR.Hands.HandDataManager.GetHandData(handType == InterVRHandType.Left ? ManusVR.SDK.Apollo.device_type_t.GLOVE_LEFT : ManusVR.SDK.Apollo.device_type_t.GLOVE_RIGHT);
            if (handData == null)
                return false;

            return handData.GetGrabState();
        }

        public bool GetGrabStateDown(InterVRHandType handType)
        {
            var handData = ManusVR.Hands.HandDataManager.GetHandData(handType == InterVRHandType.Left ? ManusVR.SDK.Apollo.device_type_t.GLOVE_LEFT : ManusVR.SDK.Apollo.device_type_t.GLOVE_RIGHT);
            if (handData == null)
                return false;

            return handData.GetGrabStateDown();
        }

        public bool GetGrabStateUp(InterVRHandType handType)
        {
            var handData = ManusVR.Hands.HandDataManager.GetHandData(handType == InterVRHandType.Left ? ManusVR.SDK.Apollo.device_type_t.GLOVE_LEFT : ManusVR.SDK.Apollo.device_type_t.GLOVE_RIGHT);
            if (handData == null)
                return false;

            return handData.GetGrabStateUp();
        }

        public void Dispose()
        {
            HandYawOffsetLeft.Dispose();
            HandYawOffsetRight.Dispose();
        }
    }
}