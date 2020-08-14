using InterVR.IF.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UnityEngine;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_Camera : IComponent
    {
    }

    public class IF_VR_CameraComponent : MonoBehaviour, IConvertToEntity
    {
        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new IF_VR_Camera() : component as IF_VR_Camera;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}