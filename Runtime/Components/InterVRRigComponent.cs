using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using UnityEngine;

namespace Inter.VR.Components
{
    public class InterVRRig : IComponent
    {
        public Transform TrackingOriginTransform { get; set; }
        public Collider HeadCollider { get; set; }
        public Transform AudioListenerTransform { get; set; }
        public Transform HMDRoot { get; set; }
        public GameObject HMD { get; set; }
        public GameObject HMDFallback { get; set; }
    }

    public class InterVRRigComponent : MonoBehaviour, IConvertToEntity
    {
        public Transform TrackingOriginTransform;
        public Collider HeadCollider;
        public Transform AudioListenerTransform;
        public Transform HMDRoot;
        public GameObject HMD;
        public GameObject HMDFallback;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new InterVRRig() : component as InterVRRig;

            if (TrackingOriginTransform == null)
                TrackingOriginTransform = this.transform;

            c.TrackingOriginTransform = TrackingOriginTransform;
            c.HeadCollider = HeadCollider;
            c.AudioListenerTransform = AudioListenerTransform;
            c.HMDRoot = HMDRoot;
            c.HMD = HMD;
            c.HMDFallback = HMDFallback;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}