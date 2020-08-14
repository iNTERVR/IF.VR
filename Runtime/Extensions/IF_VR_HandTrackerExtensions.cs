using InterVR.IF.VR.Components;

namespace InterVR.IF.VR.Extensions
{
    public static class IF_VR_HandTrackerExtensions
    {
        public static bool IsRegistered(this IF_VR_HandTracker vrHandTracker)
        {
            return vrHandTracker.Registered.Value;
        }

        public static bool IsValid(this IF_VR_HandTracker vrHandTracker)
        {
            return vrHandTracker.Valid.Value;
        }

        public static bool IsActive(this IF_VR_HandTracker vrHandTracker)
        {
            return vrHandTracker.Active.Value;
        }
    }
}