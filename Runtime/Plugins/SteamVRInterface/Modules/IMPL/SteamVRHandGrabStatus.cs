using EcsRx.Entities;
using EcsRx.Extensions;
using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.Plugins.SteamVRInterface.Installer;
using Valve.VR;

namespace Inter.VR.Plugins.SteamVRInterface.Modules
{
    public class SteamVRHandGrabStatus : IInterVRHandGrabStatus
    {
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;
        private readonly IInterVRHandInterface vrHandInterface;

        public SteamVRHandGrabStatus(SteamVRInterfaceInstaller.Settings steamVRSettings,
            IInterVRHandInterface vrHandInterface)
        {
            this.steamVRSettings = steamVRSettings;
            this.vrHandInterface = vrHandInterface;
        }

        public HandGrabTypes GetGrabStarting(IEntity handEntity, HandGrabTypes explicitType = HandGrabTypes.None)
        {
            var vrHand = handEntity.GetComponent<InterVRHand>();
            var handType = vrHand.Type == InterVRHandType.Left ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;

            if (explicitType != HandGrabTypes.None)
            {
                if (explicitType == HandGrabTypes.Pinch && steamVRSettings.GrabPinchAction.GetStateDown(handType))
                    return HandGrabTypes.Pinch;
                if (explicitType == HandGrabTypes.Grip && steamVRSettings.GrabGripAction.GetStateDown(handType))
                    return HandGrabTypes.Grip;
            }
            else
            {
                if (steamVRSettings.GrabPinchAction.GetStateDown(handType))
                    return HandGrabTypes.Pinch;
                if (steamVRSettings.GrabGripAction.GetStateDown(handType))
                    return HandGrabTypes.Grip;
            }

            return HandGrabTypes.None;
        }

        public bool IsGrabEnding(IEntity handEntity, IEntity interactableEntity)
        {
            var attachedInfoEntity = vrHandInterface.GetAttachedInfoEntity(interactableEntity);
            if (attachedInfoEntity == null)
                return false;

            var attachedInfo = attachedInfoEntity.GetComponent<InterVRHandAttachedInfo>();
            return IsGrabbingWithType(handEntity, attachedInfo.GrabbedWithType) == false;
        }

        public bool IsGrabbingWithType(IEntity handEntity, HandGrabTypes type)
        {
            var vrHand = handEntity.GetComponent<InterVRHand>();
            var steamVRHandType = vrHand.Type == InterVRHandType.Left ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            switch (type)
            {
                case HandGrabTypes.Pinch:
                    return steamVRSettings.GrabPinchAction.GetState(steamVRHandType);

                case HandGrabTypes.Grip:
                    return steamVRSettings.GrabGripAction.GetState(steamVRHandType);
            }

            return false;
        }
    }
}