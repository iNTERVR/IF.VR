using EcsRx.Infrastructure.Extensions;
using InterVR.IF.VR.Modules;
using UnityEngine;

namespace InterVR.IF.VR
{
    [DefaultExecutionOrder(-20000)]
    public abstract class IF_VR_ApplicationBehaviour : IF_ApplicationBehaviour
    {
        protected override void BindSystems()
        {
            base.BindSystems();

            Container.BindApplicableSystems(
                "InterVR.IF.VR.Systems",
                "InterVR.IF.VR.ViewResolvers");
        }

        protected override void LoadModules()
        {
            base.LoadModules();

            Container.LoadModule<IF_VR_ToolModules>();
        }

        protected override void LoadPlugins()
        {
            base.LoadPlugins();
        }
    }
}