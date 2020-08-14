using InterVR.Unity.SDK.SteamVR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UnityEngine;

namespace InterVR.Unity.SDK.SteamVR.Components
{
    public class InterVRGloveWrist : IComponent
    {
        public InterVRHandType Type { get; set; }
    }
}