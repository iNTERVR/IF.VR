using EcsRx.Entities;
using InterVR.Unity.SDK.SteamVR.Defines;
using Valve.VR;

namespace InterVR.Unity.SDK.SteamVR.Events
{
    public class OnHandHoverBeginEvent
    {
        public IEntity HandEntity;
        public IEntity HoveringEntity;
    }
}
