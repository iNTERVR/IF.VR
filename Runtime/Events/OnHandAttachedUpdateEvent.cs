using EcsRx.Entities;
using InterVR.Unity.SDK.SteamVR.Defines;
using Valve.VR;

namespace InterVR.Unity.SDK.SteamVR.Events
{
    public class OnHandAttachedUpdateEvent
    {
        public IEntity HandEntity;
        public IEntity AttachedEntity;
    }
}
