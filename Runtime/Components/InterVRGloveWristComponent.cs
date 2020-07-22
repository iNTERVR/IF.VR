using Inter.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UnityEngine;

namespace Inter.VR.Components
{
    public class InterVRGloveWrist : IComponent
    {
        public InterVRHandType Type { get; set; }
    }
}