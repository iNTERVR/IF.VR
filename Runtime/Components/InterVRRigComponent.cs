using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using Inter.VR.Defines;
using UnityEngine;

namespace Inter.VR.Components
{
    public class InterVRRig : IComponent
    {
        public Transform TrackingOriginTransform { get; set; }
        public Collider HeadCollider { get; set; }
        public Transform AudioListenerTransform { get; set; }
        public Transform HMDRoot { get; set; }
        public Transform HMDFallbackRoot { get; set; }
        public GameObject HMD { get; set; }
        public GameObject HMDFallback { get; set; }
        public InterVRRigType StartRigType { get; set; }
    }

    public class InterVRRigComponent : MonoBehaviour, IConvertToEntity
    {
        public Transform TrackingOriginTransform;
        public Collider HeadCollider;
        public Transform AudioListenerTransform;
        public Transform HMDRoot;
        public Transform HMDFallbackRoot;
        public GameObject HMD;
        public GameObject HMDFallback;
        public InterVRRigType StartRigType = InterVRRigType.VR;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new InterVRRig() : component as InterVRRig;

            if (TrackingOriginTransform == null)
                TrackingOriginTransform = this.transform;

            c.TrackingOriginTransform = TrackingOriginTransform;
            c.HeadCollider = HeadCollider;
            c.AudioListenerTransform = AudioListenerTransform;
            c.HMDRoot = HMDRoot;
            c.HMDFallbackRoot = HMDFallbackRoot;
            c.HMD = HMD;
            c.HMDFallback = HMDFallback;
            c.StartRigType = StartRigType;

            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}