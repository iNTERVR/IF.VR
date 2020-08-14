using InterVR.IF.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using System;
using UniRx;
using UnityEngine;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_Glove : IComponent, IDisposable
    {
        public IF_VR_HandType Type { get; set; }
        public Transform Wrist { get; set; }
        public IEntity WristEntity { get; set; }
        public bool Connected { get; set; }
        public BoolReactiveProperty Active { get; set; }

        public const float MaxDisconnectStateTimeSeconds = 1.0f;
        public bool GoingToDisconnect { get; set; }
        public float DisconnectStateTimer { get; set; }
        public GameObject RenderModel { get; set; }

        public IF_VR_Glove()
        {
            Active = new BoolReactiveProperty(false);
        }

        public void Dispose()
        {
            Active.Dispose();
        }
    }

    public class IF_VR_GloveComponent : MonoBehaviour, IConvertToEntity
    {
        public IF_VR_HandType Type;
        public Transform Wrist;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new IF_VR_Glove() : component as IF_VR_Glove;

            c.Type = Type;
            c.Wrist = Wrist;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}