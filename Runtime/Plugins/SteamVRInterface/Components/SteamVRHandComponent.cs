using EcsRx.Components;
using EcsRx.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Inter.VR.Plugins.SteamVRInterface.Components
{
    public class SteamVRHand : IComponent
    {
        private const int ColliderArraySize = 32;

        public Collider[] OverlappingColliders { get; set; }
        public GameObject ApplicationLostFocusObject { get; set; }

        public List<IEntity> AttachedInfoEntities = new List<IEntity>();

        public SteamVRHand()
        {
            // allocate array for colliders
            OverlappingColliders = new Collider[ColliderArraySize];
        }
    }
}