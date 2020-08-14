namespace InterVR.IF.VR.Defines
{
    public enum IF_VR_HandType
    {
        Left,
        Right
    }

    public enum IF_VR_RigType
    {
        Fallback,
        VR
    }

    public enum IF_VR_IgnoreHandHoveringType
    {
        None,       // nothing
        Left,       // left hand
        Right,      // right hand
        Both        // both hands
    }

    public enum IF_VR_HandGrabTypes
    {
        None,
        Grip,
        Pinch,
        Scripted,
    }
}
