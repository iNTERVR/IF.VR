using InterVR.IF.VR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;

namespace InterVR.IF.VR.Modules
{
    public interface IF_VR_IInterface
    {
        BoolReactiveProperty HeadsetOnHead { get; }
        int HandCount { get; }
        GameObject Rig { get; }
        Transform HMDTransform { get; }
        float EyeHeight { get; }
        Vector3 FeetPosition { get; }
        Vector3 BodyDirection { get; }
        Collider HeadCollider { get; }
        IF_VR_RigType CurrentRigType { get; }

        IEntity GetRigEntity();
        IEntity GetHandTrackerEntity(IF_VR_HandType type);
        IEntity GetHandEntity(IF_VR_HandType type);
        IEntity GetCameraEntity();
    }
}