using EcsRx.Entities;
using EcsRx.Extensions;
using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.Plugins.ManusVRInterface.Installer;

namespace Inter.VR.Plugins.ManusVRInterface.Modules
{
    public class ManusVRHandGrabStatus : IInterVRHandGrabStatus
    {
        private readonly ManusVRInterfaceInstaller.Settings manusVRSettings;
        private readonly IInterVRGloveInterface vrGloveInterface;
        private readonly IInterVRHandInterface vrHandInterface;

        public ManusVRHandGrabStatus(ManusVRInterfaceInstaller.Settings manusVRSettings,
            IInterVRGloveInterface vrGloveInterface,
            IInterVRHandInterface vrHandInterface)
        {
            this.manusVRSettings = manusVRSettings;
            this.vrGloveInterface = vrGloveInterface;
            this.vrHandInterface = vrHandInterface;
        }

        public HandGrabTypes GetGrabStarting(IEntity handEntity, HandGrabTypes explicitType = HandGrabTypes.None)
        {
            var vrHand = handEntity.GetComponent<InterVRHand>();
            if (explicitType != HandGrabTypes.None)
            {
                if (explicitType == HandGrabTypes.Grip && vrGloveInterface.GetGrabStateDown(vrHand.Type))
                {
                    return HandGrabTypes.Grip;
                }
            }
            else
            {
                if (vrGloveInterface.GetGrabStateDown(vrHand.Type))
                {
                    return HandGrabTypes.Grip;
                }
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
            switch (type)
            {
                case HandGrabTypes.Pinch:
                    return false;

                case HandGrabTypes.Grip:
                    return vrGloveInterface.GetGrabState(vrHand.Type);
            }

            return false;
        }
    }
}