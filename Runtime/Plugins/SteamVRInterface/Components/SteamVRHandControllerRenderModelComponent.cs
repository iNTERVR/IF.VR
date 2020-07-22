using EcsRx.Components;
using UnityEngine;
using Valve.VR;

namespace Inter.VR.Plugins.SteamVRInterface.Components
{
    public class SteamVRHandControllerRenderModel : IComponent
    {
        public SteamVR_RenderModel RenderModel { get; set; }
        public GameObject Instance { get; set; }
    }
}