using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Plugins;
using EcsRx.Infrastructure.Extensions;
using EcsRx.Systems;
using System;
using System.Collections.Generic;
using Inter.VR.Plugins.ManusVRInterface.Modules;

namespace Inter.VR.Plugins.ManusVRInterface
{
    public class ManusVRGloveInterfacePlugin : IEcsRxPlugin
    {
        public string Name => "ManusVR Glove Plugin";
        public Version Version => new Version(0, 0, 1);

        public void SetupDependencies(IDependencyContainer container)
        {
            container.LoadModule<ManusVRGloveInterfaceModuleSetupDependency>();

            var applicationNamespace = GetType().Namespace;
            var namespaces = new[]
            {
                $"{applicationNamespace}.Systems",
                $"{applicationNamespace}.ViewResolvers"
            };

            container.BindApplicableSystems(namespaces);
        }

        public IEnumerable<ISystem> GetSystemsForRegistration(IDependencyContainer container)
        {
            return new ISystem[0];
        }
    }
}