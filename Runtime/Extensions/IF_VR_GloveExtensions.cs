using InterVR.IF.VR.Components;

namespace InterVR.IF.VR.Extensions
{
    public static class IF_VR_GloveExtensions
    {
        public static bool IsActive(this IF_VR_Glove vrGlove)
        {
            return vrGlove.Active.Value;
        }

        public static void SetVisibility(this IF_VR_Glove vrGlove, bool state)
        {
            if (vrGlove.RenderModel)
                vrGlove.RenderModel.SetActive(state);
        }

        public static bool IsVisibility(this IF_VR_Glove vrGlove)
        {
            if (vrGlove.RenderModel == null)
                return false;
            return vrGlove.RenderModel.activeSelf;
        }
    }
}