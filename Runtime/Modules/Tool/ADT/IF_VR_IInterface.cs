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
        Transform HMDRootTransform { get; }
        Transform HMDTransform { get; }
        float EyeHeight { get; }
        Vector3 FeetPosition { get; }
        Vector3 BodyDirection { get; }
        Collider HeadCollider { get; }
        IF_VR_RigType CurrentRigType { get; set; }

        IEntity GetRigEntity();
        IEntity GetHandTrackerEntity(IF_VR_HandType type);
        IEntity GetHandEntity(IF_VR_HandType type);
        IEntity GetHandRenderModelEntity(IF_VR_HandType type);
        IEntity GetHandControllerRenderModelEntity(IF_VR_HandType type);
        IEntity GetCameraEntity();

        // Hovering
        void UpdateHandHovering(IEntity handEntity);
        void HandHoverLock(IEntity handEntity, IEntity interactableEntity);
        void HandHoverUnlock(IEntity handEntity, IEntity interactableEntity);
        void HandHoverUnlockForce(IEntity handEntity);
    }
}