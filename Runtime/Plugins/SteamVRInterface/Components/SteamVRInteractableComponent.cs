using EcsRx.Components;
using EcsRx.Entities;
using UnityEngine;
using Valve.VR;

namespace Inter.VR.Plugins.SteamVRInterface.Components
{
    public class SteamVRInteractable : IComponent
    {
        public SteamVR_Skeleton_Poser SkeletonPoser { get; set; }
    }
}