using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Unity.Extensions;
using EcsRx.Zenject;
using InterVR.Unity.SDK.SteamVR.Components;
using InterVR.Unity.SDK.SteamVR.Defines;
using InterVR.Unity.SDK.SteamVR.Events;
using System.Linq;
using UnityEngine;

namespace InterVR.Unity.SDK.SteamVR.Extensions
{
    public static class InterVRHandExtensions
    {
        public static bool IsActive(this InterVRHand vrHand)
        {
            return vrHand.Active.Value;
        }
    }
}