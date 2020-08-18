using InterVR.IF.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UniRx;
using UnityEngine;
using System;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_HandTracker : IComponent
    {
        public IF_VR_HandType Type { get; set; }
    }
}