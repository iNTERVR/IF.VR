using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Unity.Extensions;
using EcsRx.Zenject;
using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Events;
using System.Linq;
using UnityEngine;

namespace Inter.VR.Extensions
{
    public static class InterVRHandExtensions
    {
        public static bool IsActive(this InterVRHand vrHand)
        {
            return vrHand.Active.Value;
        }
    }
}