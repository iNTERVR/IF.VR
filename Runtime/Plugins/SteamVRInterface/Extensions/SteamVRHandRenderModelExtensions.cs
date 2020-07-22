using System;
using UnityEngine;
using Inter.VR.Plugins.SteamVRInterface.Components;
using Valve.VR;

namespace Inter.VR.Plugins.SteamVRInterface.Extensions
{
    public static class SteamVRHandRenderModelExtensions
    {
        public static void MatchHandToTransform(this SteamVRHandRenderModel vrHandRenderModel, Transform match)
        {
            if (vrHandRenderModel.Instance == null)
                return;

            vrHandRenderModel.Instance.transform.position = match.transform.position;
            vrHandRenderModel.Instance.transform.rotation = match.transform.rotation;
        }

        public static void SetHandPosition(this SteamVRHandRenderModel vrHandRenderModel, Vector3 newPosition)
        {
            if (vrHandRenderModel.Instance == null)
                return;

            vrHandRenderModel.Instance.transform.position = newPosition;
        }

        public static void SetHandRotation(this SteamVRHandRenderModel vrHandRenderModel, Quaternion newRotation)
        {
            if (vrHandRenderModel.Instance == null)
                return;

            vrHandRenderModel.Instance.transform.rotation = newRotation;
        }

        public static Vector3 GetHandPosition(this SteamVRHandRenderModel vrHandRenderModel)
        {
            if (vrHandRenderModel.Instance == null)
                return Vector3.zero;

            return vrHandRenderModel.Instance.transform.position;
        }

        public static Quaternion GetHandRotation(this SteamVRHandRenderModel vrHandRenderModel)
        {
            if (vrHandRenderModel.Instance == null)
                return Quaternion.identity;

            return vrHandRenderModel.Instance.transform.rotation;
        }

        public static Transform GetBone(this SteamVRHandRenderModel vrHandRenderModel, int boneIndex)
        {
            if (vrHandRenderModel.Skeleton == null)
                return null;

            return vrHandRenderModel.Skeleton.GetBone(boneIndex);
        }

        public static Vector3 GetBonePosition(this SteamVRHandRenderModel vrHandRenderModel, int boneIndex, bool local = false)
        {
            if (vrHandRenderModel.Skeleton == null)
                return Vector3.zero;

            return vrHandRenderModel.Skeleton.GetBonePosition(boneIndex, local);
        }

        public static Quaternion GetBoneRotation(this SteamVRHandRenderModel vrHandRenderModel, int boneIndex, bool local = false)
        {
            if (vrHandRenderModel.Skeleton == null)
                return Quaternion.identity;

            return vrHandRenderModel.Skeleton.GetBoneRotation(boneIndex, local);
        }

        public static void SetSkeletonRangeOfMotion(this SteamVRHandRenderModel vrHandRenderModel, EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
        {
            if (vrHandRenderModel.Skeleton == null)
                return;

            vrHandRenderModel.Skeleton.SetRangeOfMotion(newRangeOfMotion, blendOverSeconds);
        }

        public static EVRSkeletalMotionRange GetSkeletonRangeOfMotion(this SteamVRHandRenderModel vrHandRenderModel)
        {
            if (vrHandRenderModel.Skeleton == null)
                return EVRSkeletalMotionRange.WithController;

            return vrHandRenderModel.Skeleton.rangeOfMotion;
        }

        public static void SetTemporarySkeletonRangeOfMotion(this SteamVRHandRenderModel vrHandRenderModel, SkeletalMotionRangeChange temporaryRangeOfMotionChange, float blendOverSeconds = 0.1f)
        {
            if (vrHandRenderModel.Skeleton == null)
                return;

            vrHandRenderModel.Skeleton.SetTemporaryRangeOfMotion((EVRSkeletalMotionRange)temporaryRangeOfMotionChange, blendOverSeconds);
        }

        public static void ResetTemporarySkeletonRangeOfMotion(this SteamVRHandRenderModel vrHandRenderModel, float blendOverSeconds = 0.1f)
        {
            if (vrHandRenderModel.Skeleton == null)
                return;

            vrHandRenderModel.Skeleton.ResetTemporaryRangeOfMotion(blendOverSeconds);
        }

        public static void SetAnimationState(this SteamVRHandRenderModel vrHandRenderModel, int stateValue)
        {
            if (vrHandRenderModel.Skeleton == null)
                return;

            if (vrHandRenderModel.Skeleton.isBlending == false)
                vrHandRenderModel.Skeleton.BlendToAnimation();

            if (vrHandRenderModel.CheckAnimatorInit())
                vrHandRenderModel.Animator.SetInteger(vrHandRenderModel.AnimatorStateId, stateValue);
        }

        public static void StopAnimation(this SteamVRHandRenderModel vrHandRenderModel)
        {
            if (vrHandRenderModel.Skeleton == null)
                return;

            if (vrHandRenderModel.Skeleton.isBlending == false)
                vrHandRenderModel.Skeleton.BlendToSkeleton();

            if (vrHandRenderModel.CheckAnimatorInit())
                vrHandRenderModel.Animator.SetInteger(vrHandRenderModel.AnimatorStateId, 0);
        }

        public static bool CheckAnimatorInit(this SteamVRHandRenderModel vrHandRenderModel)
        {
            if (vrHandRenderModel.AnimatorStateId == -1 && vrHandRenderModel.Animator != null)
            {
                if (vrHandRenderModel.Animator.gameObject.activeInHierarchy && vrHandRenderModel.Animator.isInitialized)
                {
                    AnimatorControllerParameter[] parameters = vrHandRenderModel.Animator.parameters;
                    for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                    {
                        if (string.Equals(parameters[parameterIndex].name, vrHandRenderModel.AnimatorParameterStateName, StringComparison.CurrentCultureIgnoreCase))
                            vrHandRenderModel.AnimatorStateId = parameters[parameterIndex].nameHash;
                    }
                }
            }

            return vrHandRenderModel.AnimatorStateId != -1 && vrHandRenderModel.Animator != null && vrHandRenderModel.Animator.isInitialized;
        }
    }
}