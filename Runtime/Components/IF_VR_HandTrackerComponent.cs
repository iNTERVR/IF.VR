using InterVR.IF.VR.Defines;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UniRx;
using UnityEngine;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_HandTracker : IComponent
    {
        public IF_VR_HandType Type { get; set; }
        public BoolReactiveProperty Registered { get; set; }    // 등록되었는가?
        public BoolReactiveProperty Valid { get; set; }         // 등록된 상태며, 유효한가?
        public BoolReactiveProperty Active { get; set; }        // 활성화 되었는가?

        public IF_VR_HandTracker()
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

    public class IF_VR_HandTrackerComponent : MonoBehaviour, IConvertToEntity
    {
        public IF_VR_HandType Type;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new IF_VR_HandTracker() : component as IF_VR_HandTracker;

            c.Type = Type;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}