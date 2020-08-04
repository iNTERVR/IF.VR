using EcsRx.Entities;
using Inter.VR.Defines;
using Valve.VR;

namespace Inter.VR.Events
{
    public class OnHandHoverBeginEvent
    {
        public IEntity HandEntity;
        public IEntity HoveringEntity;
    }
}
