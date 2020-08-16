using InterVR.IF.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using System;
using UniRx;
using UnityEngine;
using System.Collections.Generic;
using EcsRx.Unity.Extensions;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_Hand : IComponent
    {
        public IF_VR_HandType Type { get; set; }
    }
}