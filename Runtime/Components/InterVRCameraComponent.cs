using Inter.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UnityEngine;

namespace Inter.VR.Components
{
    public class InterVRCamera : IComponent
    {
    }

    public class InterVRCameraComponent : MonoBehaviour, IConvertToEntity
    {
        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new InterVRCamera() : component as InterVRCamera;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}