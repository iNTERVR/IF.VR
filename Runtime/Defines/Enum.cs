namespace InterVR.Unity.SDK.SteamVR.Defines
{
    public enum InterVRHandType
    {
        Left,
        Right
    }

    public enum InterVRRigType
    {
        Fallback,
        VR
    }

    public enum IgnoreHandHoveringType
    {
        None,       // nothing
        Left,       // left hand
        Right,      // right hand
        Both        // both hands
    }

    public enum HandGrabTypes
    {
        None,
        Grip,
        Pinch,
        Scripted,
    }
}
