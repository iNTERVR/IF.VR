using UnityEngine;
using InterVR.Unity.SDK.SteamVR.Components;

namespace InterVR.Unity.SDK.SteamVR.Extensions
{
    public static class InterVRHandControllerRenderModelExtensions
    {
        public static void SetVisibility(this InterVRHandControllerRenderModel vrHandControllerRenderModel, bool state, bool permanent = false)
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

        public static void Show(this InterVRHandControllerRenderModel vrHandControllerRenderModel, bool overrideDefault = false)
        {
            vrHandControllerRenderModel.SetVisibility(true, overrideDefault);
        }

        public static void Hide(this InterVRHandControllerRenderModel vrHandControllerRenderModel)
        {
            vrHandControllerRenderModel.SetVisibility(false);
        }

        public static void SetMaterial(this InterVRHandControllerRenderModel vrHandControllerRenderModel, Material material)
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

        public static bool IsVisibile(this InterVRHandControllerRenderModel vrHandControllerRenderModel)
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