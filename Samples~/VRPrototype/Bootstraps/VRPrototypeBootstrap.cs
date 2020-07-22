using Inter.VR.Modules.InterVRInterfaces;
using EcsRx.Zenject;
using EcsRx.Infrastructure.Extensions;
using UnityEngine;
using Inter.VR.VRPrototype.Installer;
using Inter.Installer;
using Inter.VR.Plugins.SteamVRInterface;
using Inter.Modules.ToolModule;

namespace Inter.VR.VRPrototype
{
    public class VRPrototypeBootstrap : EcsRxApplicationBehaviour
    {
        protected override void BindSystems()
        {
            base.BindSystems();

            // inter system setup
            Container.BindApplicableSystems(
                "Inter.Systems",
                "Inter.ViewResolvers"
                );

            // inter vr system setup
            Container.BindApplicableSystems(
                "Inter.VR.Systems",
                "Inter.VR.ViewResolvers"
                );
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
        }

        protected override void ApplicationStarted()
        {
            var settings = Container.Resolve<VRPrototypeInstaller.Settings>();
            var interSettings = Container.Resolve<InterInstaller.Settings>();
            Debug.Log($"settings.Name is {settings.Name} in {interSettings.Name}");
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
