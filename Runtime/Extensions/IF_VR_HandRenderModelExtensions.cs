using UnityEngine;
using InterVR.IF.VR.Components;

namespace InterVR.IF.VR.Extensions
{
    public static class IF_VR_HandRenderModelExtensions
    {
        public static void SetVisibility(this IF_VR_HandRenderModel vrHandRenderModel, bool state, bool permanent = false)
        {
            if (permanent)
                vrHandRenderModel.DisplayByDefault = state;

            if (vrHandRenderModel.Renderers == null)
                return;

            for (int rendererIndex = 0; rendererIndex < vrHandRenderModel.Renderers.Length; rendererIndex++)
            {
                vrHandRenderModel.Renderers[rendererIndex].enabled = state;
            }
        }

        public static void Show(this IF_VR_HandRenderModel vrHandRenderModel, bool overrideDefault = false)
        {
            vrHandRenderModel.SetVisibility(true, overrideDefault);
        }

        public static void Hide(this IF_VR_HandRenderModel vrHandRenderModel)
        {
            vrHandRenderModel.SetVisibility(false);
        }

        public static void SetMaterial(this IF_VR_HandRenderModel vrHandRenderModel, Material material)
        {
            if (vrHandRenderModel.Renderers == null)
                return;

            for (int rendererIndex = 0; rendererIndex < vrHandRenderModel.Renderers.Length; rendererIndex++)
            {
                vrHandRenderModel.Renderers[rendererIndex].material = material;
            }
        }

        public static bool IsVisibile(this IF_VR_HandRenderModel vrHandRenderModel)
        {
            if (vrHandRenderModel.Renderers == null)
                return false;

            for (int rendererIndex = 0; rendererIndex < vrHandRenderModel.Renderers.Length; rendererIndex++)
            {
                if (vrHandRenderModel.Renderers[rendererIndex] != null && vrHandRenderModel.Renderers[rendererIndex].enabled)
                    return true;
            }

            return false;
        }
    }
}