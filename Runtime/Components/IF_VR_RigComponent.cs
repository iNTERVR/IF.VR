using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using InterVR.IF.VR.Defines;
using UnityEngine;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_Rig : IComponent
    {
        public Transform TrackingOriginTransform { get; set; }
        public Collider HeadCollider { get; set; }
        public Transform AudioListenerTransform { get; set; }
        public Transform HMDRoot { get; set; }
        public Transform HMDFallbackRoot { get; set; }
        public GameObject HMD { get; set; }
        public GameObject HMDFallback { get; set; }
        public IF_VR_RigType StartRigType { get; set; }
    }

    public class IF_VR_RigComponent : MonoBehaviour, IConvertToEntity
    {
        public Transform TrackingOriginTransform;
        public Collider HeadCollider;
        public Transform AudioListenerTransform;
        public Transform HMDRoot;
        public Transform HMDFallbackRoot;
        public GameObject HMD;
        public GameObject HMDFallback;
        public IF_VR_RigType StartRigType = IF_VR_RigType.VR;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new IF_VR_Rig() : component as IF_VR_Rig;

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