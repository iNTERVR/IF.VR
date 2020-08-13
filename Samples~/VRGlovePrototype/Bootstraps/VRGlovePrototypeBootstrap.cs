using Inter.VR.Modules.InterVRInterfaces;
using EcsRx.Zenject;
using EcsRx.Infrastructure.Extensions;
using UnityEngine;
using Inter.VR.VRGlovePrototype.Installer;
using Inter.Installer;
using Inter.VR.Plugins.SteamVRInterface;
using Inter.Modules.ToolModule;
using Inter.VR.Plugins.ManusVRInterface;

namespace Inter.VR.VRGlovePrototype
{
    public class VRGlovePrototypeBootstrap : EcsRxApplicationBehaviour
    {
        public bool UseGlove;

        protected override void BindSystems()
        {
            base.BindSystems();

            Container.BindApplicableSystems(
                "Inter.Systems",
                "Inter.ViewResolvers");
            Container.BindApplicableSystems(
                "Inter.VR.Systems",
                "Inter.VR.ViewResolvers");
        }

        protected override void LoadModules()
        {
            base.LoadModules();

            Container.LoadModule<ToolModuleSetupDependency>();
            Container.LoadModule<InterVRInterfaceModuleSetupDependency>();
        }

        protected override void LoadPlugins()
        {
            base.LoadPlugins();

            RegisterPlugin(new SteamVRInterfacePlugin());
            if (UseGlove)
                RegisterPlugin(new ManusVRGloveInterfacePlugin());
        }

        protected override void ApplicationStarted()
        {
            var settings = Container.Resolve<VRGlovePrototypeInstaller.Settings>();
            var interSettings = Container.Resolve<InterInstaller.Settings>();
            Debug.Log($"settings.Name is {settings.Name} in {interSettings.Name}");

            if (UseGlove)
            {
                var vrGloveInterface = Container.Resolve<IInterVRGloveInterface>();
                vrGloveInterface.HandYawOffsetLeft.Value = settings.HandYawOffsetLeft;
                vrGloveInterface.HandYawOffsetRight.Value = settings.HandYawOffsetRight;
            }
        }

        private void OnDestroy()
        {
            StopAndUnbindAllSystems();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == false)
            {
            }
        }

        private void OnApplicationFocus(bool focus)
        {

        }
    }
}
