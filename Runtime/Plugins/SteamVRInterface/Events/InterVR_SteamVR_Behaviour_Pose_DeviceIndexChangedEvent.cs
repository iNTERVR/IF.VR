using Inter.VR.Defines;
using Valve.VR;

namespace Inter.VR.Plugins.SteamVRInterface.Events
{
    public class InterVR_SteamVR_Behaviour_Pose_DeviceIndexChangedEvent
    {
        public InterVRHandType Type;
        public SteamVR_Behaviour_Pose FromAction;
        public SteamVR_Input_Sources FromSource;
        public int NewDeviceIndex;
    }
}
