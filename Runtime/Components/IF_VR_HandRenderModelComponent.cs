using InterVR.IF.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using System;
using UniRx;
using UnityEngine;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_HandRenderModel : IComponent, IDisposable
    {
        public IF_VR_HandType Type { get; set; }
        public bool DisplayByDefault { get; set; }
        public BoolReactiveProperty CastShadow { get; set; }
        public BoolReactiveProperty ReceiveShadows { get; set; }
        public Renderer[] Renderers { get; set; }
        public BoolReactiveProperty Visibility { get; set; }

        public IF_VR_HandRenderModel()
        {
            Visibility = new BoolReactiveProperty(true);
            CastShadow = new BoolReactiveProperty(false);
            ReceiveShadows = new BoolReactiveProperty(false);
        }

        public void Dispose()
        {
            Visibility.Dispose();
            CastShadow.Dispose();
            ReceiveShadows.Dispose();
        }
    }

    public class IF_VR_HandRenderModelComponent : MonoBehaviour, IConvertToEntity
    {
        public IF_VR_HandType Type;
        public bool DisplayByDefault;
        public bool CastShadow;
        public bool ReceiveShadows;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new IF_VR_HandRenderModel() : component as IF_VR_HandRenderModel;

            c.Type = Type;
            c.DisplayByDefault = DisplayByDefault;
            c.CastShadow.Value = CastShadow;
            c.ReceiveShadows.Value = ReceiveShadows;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}