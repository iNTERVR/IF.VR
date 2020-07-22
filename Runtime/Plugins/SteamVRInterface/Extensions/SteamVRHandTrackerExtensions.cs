using Inter.VR.Plugins.SteamVRInterface.Components;

namespace Inter.VR.Plugins.SteamVRInterface.Extensions
{
    public static class SteamVRHandTrackerExtensions
    {
        public static bool IsPoseValid(this SteamVRHandTracker steamVRHandTracker)
        {
            if (steamVRHandTracker.TrackedObject == null)
                return false;

            return steamVRHandTracker.TrackedObject.isValid;
        }

        public static bool IsActive(this SteamVRHandTracker steamVRHandTracker)
        {
            if (steamVRHandTracker.TrackedObject == null)
                return false;

            return steamVRHandTracker.TrackedObject.isActive;
        }
    }
}