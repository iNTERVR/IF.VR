using InterVR.IF.VR.Components;

namespace InterVR.IF.VR.Extensions
{
    public static class IF_VR_HandExtensions
    {
        public static bool IsActive(this IF_VR_Hand vrHand)
        {
            return vrHand.Active.Value;
        }
    }
}