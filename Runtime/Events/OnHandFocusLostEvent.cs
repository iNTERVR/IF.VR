using EcsRx.Entities;
using Inter.VR.Defines;
using Valve.VR;

namespace Inter.VR.Events
{
    public class OnHandFocusLostEvent
    {
        public IEntity HandEntity;
        public IEntity AttachedEntity;
    }
}
