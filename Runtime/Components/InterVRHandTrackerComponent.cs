using InterVR.Unity.SDK.SteamVR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UniRx;
using UnityEngine;

namespace InterVR.Unity.SDK.SteamVR.Components
{
    public class InterVRHandTracker : IComponent
    {
        public InterVRHandType Type { get; set; }
        public BoolReactiveProperty Registered { get; set; }    // 등록되었는가?
        public BoolReactiveProperty Valid { get; set; }         // 등록된 상태며, 유효한가?
        public BoolReactiveProperty Active { get; set; }        // 활성화 되었는가?

        public InterVRHandTracker()
        {
            Registered = new BoolReactiveProperty(false);
            Valid = new BoolReactiveProperty(false);
            Active = new BoolReactiveProperty(false);
        }

        public void Dispose()
        {
            Registered.Dispose();
            Valid.Dispose();
            Active.Dispose();
        }
    }

    public class InterVRHandTrackerComponent : MonoBehaviour, IConvertToEntity
    {
        public InterVRHandType Type;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new InterVRHandTracker() : component as InterVRHandTracker;

            c.Type = Type;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}