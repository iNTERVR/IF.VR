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
}