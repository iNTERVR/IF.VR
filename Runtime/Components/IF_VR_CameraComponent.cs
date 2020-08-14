using InterVR.Unity.SDK.SteamVR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UnityEngine;

namespace InterVR.Unity.SDK.SteamVR.Components
{
    public class InterVRCamera : IComponent
    {
    }

    public class IF_VR_CameraComponent : MonoBehaviour, IConvertToEntity
    {
        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new InterVRCamera() : component as InterVRCamera;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}