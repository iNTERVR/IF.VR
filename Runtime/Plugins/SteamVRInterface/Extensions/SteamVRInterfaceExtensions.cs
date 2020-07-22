using EcsRx.Entities;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using Valve.VR;

namespace Inter.VR.Plugins.SteamVRInterface.Extensions
{
    public static class IInterVRInterfaceExtensions
    {
        public static bool SteamVRInitialized(this IInterVRInterface vrInterface)
        {
            bool init = !(SteamVR.initializedState == SteamVR.InitializedStates.None ||
                SteamVR.initializedState == SteamVR.InitializedStates.Initializing ||
                SteamVR.initializedState == SteamVR.InitializedStates.InitializeFailure);

            if (init == false)
                return false;

            if (SteamVR.instance == null)
                return false;

            return true;
        }
    }
}