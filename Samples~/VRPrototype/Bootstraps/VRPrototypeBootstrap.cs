using InterVR.Unity.SDK.SteamVR.Modules.InterVRInterfaces;
using EcsRx.Zenject;
using EcsRx.Infrastructure.Extensions;
using UnityEngine;
using InterVR.Extern.SteamVR.VRPrototype.Installer;
using InterVR.IF.Installer;

namespace InterVR.Extern.SteamVR.VRPrototype
{
    public class VRPrototypeBootstrap : EcsRxApplicationBehaviour
    {
        protected override void LoadModules()
        {
            base.LoadModules();

            Container.LoadModule<InterVRInterfaceModuleSetupDependency>();
        }

        protected override void LoadPlugins()
        {
            base.LoadPlugins();

            //RegisterPlugin(new SteamVRInterfacePlugin());
        }

        protected override void ApplicationStarted()
        {
            var settings = Container.Resolve<VRPrototypeInstaller.Settings>();
            var interSettings = Container.Resolve<IF_Installer.Settings>();
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
