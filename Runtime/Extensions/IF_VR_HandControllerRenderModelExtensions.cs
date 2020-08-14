using UnityEngine;
using InterVR.IF.VR.Components;

namespace InterVR.Unity.SDK.SteamVR.Extensions
{
    public static class IF_VR_HandControllerRenderModelExtensions
    {
        public static void SetVisibility(this IF_VR_HandControllerRenderModel vrHandControllerRenderModel, bool state, bool permanent = false)
        {
            if (permanent)
                vrHandControllerRenderModel.DisplayByDefault = state;

            if (vrHandControllerRenderModel.Renderers == null)
                return;

            for (int rendererIndex = 0; rendererIndex < vrHandControllerRenderModel.Renderers.Length; rendererIndex++)
            {
                vrHandControllerRenderModel.Renderers[rendererIndex].enabled = state;
            }
        }

        public static void Show(this IF_VR_HandControllerRenderModel vrHandControllerRenderModel, bool overrideDefault = false)
        {
            vrHandControllerRenderModel.SetVisibility(true, overrideDefault);
        }

        public static void Hide(this IF_VR_HandControllerRenderModel vrHandControllerRenderModel)
        {
            vrHandControllerRenderModel.SetVisibility(false);
        }

        public static void SetMaterial(this IF_VR_HandControllerRenderModel vrHandControllerRenderModel, Material material)
        {
            if (vrHandControllerRenderModel.Renderers == null)
            {
                vrHandControllerRenderModel.DelayedSetMaterial = material;
                return;
            }

            for (int rendererIndex = 0; rendererIndex < vrHandControllerRenderModel.Renderers.Length; rendererIndex++)
            {
                vrHandControllerRenderModel.Renderers[rendererIndex].material = material;
            }
        }

        public static bool IsVisibile(this IF_VR_HandControllerRenderModel vrHandControllerRenderModel)
        {
            if (vrHandControllerRenderModel.Renderers == null)
                return false;

            for (int rendererIndex = 0; rendererIndex < vrHandControllerRenderModel.Renderers.Length; rendererIndex++)
            {
                if (vrHandControllerRenderModel.Renderers[rendererIndex].enabled)
                    return true;
            }

            return false;
        }
    }
}