using Inter.VR.Components;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Plugins.ReactiveSystems.Systems;
using EcsRx.Plugins.Views.Components;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.VRGlovePrototype.Installer;

namespace Inter.VR.VRGlovePrototype.Systems
{
    public class VRGlovePrototypeSystem: ISetupSystem
    {
        public IGroup Group => new Group(typeof(InterVRGlove), typeof(ViewComponent));

        private readonly IInterVRGloveInterface vrGloveInterface;
        private readonly VRGlovePrototypeInstaller.Settings settings;

        public VRGlovePrototypeSystem(IInterVRGloveInterface vrGloveInterface,
            VRGlovePrototypeInstaller.Settings settings)
        {
            this.vrGloveInterface = vrGloveInterface;
            this.settings = settings;
        }

        public void Setup(IEntity entity)
        {
            vrGloveInterface.HandYawOffsetLeft.Value = settings.HandYawOffsetLeft;
            vrGloveInterface.HandYawOffsetRight.Value = settings.HandYawOffsetRight;
        }
    }
}