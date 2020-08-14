using UnityEngine;
using InterVR.Unity.SDK.SteamVR.Components;

namespace InterVR.Unity.SDK.SteamVR.Extensions
{
    public static class InterVRHandRenderModelExtensions
    {
        public static void SetVisibility(this InterVRHandRenderModel vrHandRenderModel, bool state, bool permanent = false)
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

        public static void Show(this InterVRHandRenderModel vrHandRenderModel, bool overrideDefault = false)
        {
            vrHandRenderModel.SetVisibility(true, overrideDefault);
        }

        public static void Hide(this InterVRHandRenderModel vrHandRenderModel)
        {
            vrHandRenderModel.SetVisibility(false);
        }

        public static void SetMaterial(this InterVRHandRenderModel vrHandRenderModel, Material material)
        {
            if (vrHandRenderModel.Renderers == null)
                return;

            for (int rendererIndex = 0; rendererIndex < vrHandRenderModel.Renderers.Length; rendererIndex++)
            {
                vrHandRenderModel.Renderers[rendererIndex].material = material;
            }
        }

        public static bool IsVisibile(this InterVRHandRenderModel vrHandRenderModel)
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