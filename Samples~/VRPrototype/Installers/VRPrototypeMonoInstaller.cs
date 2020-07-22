using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Inter.VR.VRPrototype.Installer
{
    public class VRPrototypeMonoInstaller : MonoInstaller<VRPrototypeMonoInstaller>
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