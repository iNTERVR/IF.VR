using EcsRx.Components;
using Valve.VR;

namespace Inter.VR.Plugins.SteamVRInterface.Components
{
    public class SteamVRHandTracker : IComponent
    {
        public SteamVR_Behaviour_Pose TrackedObject { get; set; }
    }
}