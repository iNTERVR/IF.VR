using EcsRx.Entities;
using InterVR.IF.VR.Defines;

namespace InterVR.IF.VR.Events
{
    public class IF_VR_Event_SetInputSource
    {
        public IEntity SourceEntity;
        public IF_VR_HandType HandType;
    }
}
