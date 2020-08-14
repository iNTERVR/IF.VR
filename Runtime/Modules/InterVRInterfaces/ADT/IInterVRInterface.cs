using InterVR.Unity.SDK.SteamVR.Components;
using InterVR.Unity.SDK.SteamVR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;

namespace InterVR.Unity.SDK.SteamVR.Modules.InterVRInterfaces
{
    public interface IInterVRInterface
    {
        BoolReactiveProperty HeadsetOnHead { get; }
        int HandCount { get; }
        Transform HMDRootTransform { get; }
        Transform HMDTransform { get; }
        float EyeHeight { get; }
        Vector3 FeetPosition { get; }
        Vector3 BodyDirection { get; }
        Collider HeadCollider { get; }
        InterVRRigType CurrentRigType { get; set; }

        IEntity GetRigEntity();
        IEntity GetHandTrackerEntity(InterVRHandType type);
        IEntity GetHandEntity(InterVRHandType type);
        IEntity GetHandRenderModelEntity(InterVRHandType type);
        IEntity GetHandControllerRenderModelEntity(InterVRHandType type);
        IEntity GetGloveEntity(InterVRHandType type);
        IEntity GetCameraEntity();

        // Hovering
        void UpdateHandHovering(IEntity handEntity);
        void HandHoverLock(IEntity handEntity, IEntity interactableEntity);
        void HandHoverUnlock(IEntity handEntity, IEntity interactableEntity);
        void HandHoverUnlockForce(IEntity handEntity);
    }
}