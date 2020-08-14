using InterVR.Unity.SDK.SteamVR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using System;
using UniRx;
using UnityEngine;

namespace InterVR.Unity.SDK.SteamVR.Components
{
    public class InterVRGlove : IComponent, IDisposable
    {
        public InterVRHandType Type { get; set; }
        public Transform Wrist { get; set; }
        public IEntity WristEntity { get; set; }
        public bool Connected { get; set; }
        public BoolReactiveProperty Active { get; set; }

        public const float MaxDisconnectStateTimeSeconds = 1.0f;
        public bool GoingToDisconnect { get; set; }
        public float DisconnectStateTimer { get; set; }
        public GameObject RenderModel { get; set; }

        public InterVRGlove()
        {
            Active = new BoolReactiveProperty(false);
        }

        public void Dispose()
        {
            Active.Dispose();
        }
    }

    public class InterVRGloveComponent : MonoBehaviour, IConvertToEntity
    {
        public InterVRHandType Type;
        public Transform Wrist;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new InterVRGlove() : component as InterVRGlove;

            c.Type = Type;
            c.Wrist = Wrist;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}