using UnityEngine;
using Inter.VR.Plugins.SteamVRInterface.Components;

namespace Inter.VR.Plugins.SteamVRInterface.Extensions
{
    public static class SteamVRHandControllerRenderModelExtensions
    {
        public static Vector3 GetControllerPosition(this SteamVRHandControllerRenderModel vrHandControllerRenderModel, string componentName = null)
        {
            if (vrHandControllerRenderModel.RenderModel == null)
                return Vector3.zero;

            return vrHandControllerRenderModel.RenderModel.GetComponentTransform(componentName).position;
        }
    }
}