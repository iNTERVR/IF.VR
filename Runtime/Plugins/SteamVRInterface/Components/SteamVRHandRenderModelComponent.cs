using EcsRx.Components;
using UnityEngine;
using Valve.VR;

namespace Inter.VR.Plugins.SteamVRInterface.Components
{
    public class SteamVRHandRenderModel : IComponent
    {
        public SteamVR_Behaviour_Skeleton Skeleton { get; set; }
        public GameObject Instance { get; set; }
        public Animator Animator { get; set; }
        public string AnimatorParameterStateName { get; set; }
        public int AnimatorStateId { get; set; }

        public SteamVRHandRenderModel()
        {
            AnimatorParameterStateName = "AnimationState";
            AnimatorStateId = -1;
        }
    }
}