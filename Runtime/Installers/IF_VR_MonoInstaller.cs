using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace InterVR.IF.VR.Installer
{
    public class IF_VR_MonoInstaller : MonoInstaller<IF_VR_MonoInstaller>
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