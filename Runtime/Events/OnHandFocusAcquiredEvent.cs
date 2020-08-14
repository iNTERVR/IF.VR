using EcsRx.Entities;
using InterVR.Unity.SDK.SteamVR.Defines;
using Valve.VR;

namespace InterVR.Unity.SDK.SteamVR.Events
{
    public class OnHandFocusAcquiredEvent
    {
        public IEntity HandEntity;
        public IEntity AttachedEntity;
    }
}
