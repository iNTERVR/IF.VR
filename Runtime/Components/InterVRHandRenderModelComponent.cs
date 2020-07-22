using Inter.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using System;
using UniRx;
using UnityEngine;

namespace Inter.VR.Components
{
    public class InterVRHandRenderModel : IComponent, IDisposable
    {
        public InterVRHandType Type { get; set; }
        public bool DisplayByDefault { get; set; }
        public BoolReactiveProperty CastShadow { get; set; }
        public BoolReactiveProperty ReceiveShadows { get; set; }
        public Renderer[] Renderers { get; set; }
        public BoolReactiveProperty Visibility { get; set; }

        public InterVRHandRenderModel()
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

    public class InterVRHandRenderModelComponent : MonoBehaviour, IConvertToEntity
    {
        public InterVRHandType Type;
        public bool DisplayByDefault;
        public bool CastShadow;
        public bool ReceiveShadows;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new InterVRHandRenderModel() : component as InterVRHandRenderModel;

            c.Type = Type;
            c.DisplayByDefault = DisplayByDefault;
            c.CastShadow.Value = CastShadow;
            c.ReceiveShadows.Value = ReceiveShadows;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}