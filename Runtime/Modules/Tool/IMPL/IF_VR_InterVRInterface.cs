using InterVR.IF.VR.Defines;
using EcsRx.Entities;
using System;
using UniRx;
using UnityEngine;
using EcsRx.Collections.Database;
using EcsRx.Groups;
using InterVR.IF.VR.Components;
using EcsRx.Extensions;
using EcsRx.Unity.Extensions;
using System.Linq;
using EcsRx.Events;
using InterVR.IF.VR.Events;
using EcsRx.Unity.MonoBehaviours;

namespace InterVR.IF.VR.Modules
{
    public class IF_VR_Interface : IF_VR_IInterface, IDisposable
    {
        public BoolReactiveProperty HeadsetOnHead => throw new NotImplementedException();

        public int HandCount => throw new NotImplementedException();

        public Transform HMDRootTransform => throw new NotImplementedException();

        public Transform HMDTransform => throw new NotImplementedException();

        public float EyeHeight => throw new NotImplementedException();

        public Vector3 FeetPosition => throw new NotImplementedException();

        public Vector3 BodyDirection => throw new NotImplementedException();

        public Collider HeadCollider => throw new NotImplementedException();

        public IF_VR_RigType CurrentRigType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEntity GetCameraEntity()
        {
            throw new NotImplementedException();
        }

        public IEntity GetHandControllerRenderModelEntity(IF_VR_HandType type)
        {
            throw new NotImplementedException();
        }

        public IEntity GetHandEntity(IF_VR_HandType type)
        {
            throw new NotImplementedException();
        }

        public IEntity GetHandRenderModelEntity(IF_VR_HandType type)
        {
            throw new NotImplementedException();
        }

        public IEntity GetHandTrackerEntity(IF_VR_HandType type)
        {
            throw new NotImplementedException();
        }

        public IEntity GetRigEntity()
        {
            throw new NotImplementedException();
        }
    }
}