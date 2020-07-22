using Inter.VR.Components;

namespace Inter.VR.Extensions
{
    public static class InterVRHandTrackerExtensions
    {
        public static bool IsRegistered(this InterVRHandTracker vrHandTracker)
        {
            return vrHandTracker.Registered.Value;
        }

        public static bool IsValid(this InterVRHandTracker vrHandTracker)
        {
            return vrHandTracker.Valid.Value;
        }

        public static bool IsActive(this InterVRHandTracker vrHandTracker)
        {
            return vrHandTracker.Active.Value;
        }
    }
}