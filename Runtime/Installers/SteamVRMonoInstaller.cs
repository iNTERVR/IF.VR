using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace InterVR.Unity.SDK.SteamVR.Installer
{
    public class SteamVRMonoInstaller : MonoInstaller<SteamVRMonoInstaller>
    {
        public List<ScriptableObjectInstaller> settings;

        public override void InstallBindings()
        {
            var settingsInstaller = settings.Cast<IInstaller>();
            foreach (var installer in settingsInstaller)
            {
                Container.Inject(installer);
                installer.InstallBindings();
            }
        }
    }
}