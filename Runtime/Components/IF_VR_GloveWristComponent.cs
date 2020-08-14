using InterVR.IF.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UnityEngine;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_GloveWrist : IComponent
    {
        public IF_VR_HandType Type { get; set; }
    }
}