using InterVR.Unity.SDK.SteamVR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UniRx;
using UnityEngine;

namespace InterVR.Unity.SDK.SteamVR.Components
{
    public class InterVRHandControllerRenderModel : IComponent
    {
        public InterVRHandType Type { get; set; }
        public int DeviceIndex { get; set; }
        public bool DisplayByDefault { get; set; }
        public BoolReactiveProperty CastShadow { get; set; }
        public BoolReactiveProperty ReceiveShadows { get; set; }
        public Renderer[] Renderers { get; set; }
        public BoolReactiveProperty Visibility { get; set; }
        public Material DelayedSetMaterial { get; set; }

        public InterVRHandControllerRenderModel()
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

    public class InterVRHandControllerRenderModelComponent : MonoBehaviour, IConvertToEntity
    {
        public InterVRHandType Type;
        public bool DisplayByDefault;
        public bool CastShadow;
        public bool ReceiveShadows;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new InterVRHandControllerRenderModel() : component as InterVRHandControllerRenderModel;

            c.Type = Type;
            c.CastShadow.Value = CastShadow;
            c.ReceiveShadows.Value = ReceiveShadows;
            c.DisplayByDefault = DisplayByDefault;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}