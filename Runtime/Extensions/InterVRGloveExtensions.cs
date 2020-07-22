using Inter.VR.Components;

namespace Inter.VR.Extensions
{
    public static class InterVRGloveExtensions
    {
        public static bool IsActive(this InterVRGlove vrGlove)
        {
            return vrGlove.Active.Value;
        }

        public static void SetVisibility(this InterVRGlove vrGlove, bool state)
        {
            if (vrGlove.RenderModel)
                vrGlove.RenderModel.SetActive(state);
        }

        public static bool IsVisibility(this InterVRGlove vrGlove)
        {
            if (vrGlove.RenderModel == null)
                return false;
            return vrGlove.RenderModel.activeSelf;
        }
    }
}