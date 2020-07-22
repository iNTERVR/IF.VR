using System;
using UnityEngine;
using Zenject;

namespace Inter.VR.Plugins.ManusVRInterface.Installer
{
    [CreateAssetMenu(fileName = "ManusVRInterfaceSettings", menuName = "Inter/VR/Plugin/ManusVRInterface/Settings")]
    public class ManusVRInterfaceInstaller : ScriptableObjectInstaller<ManusVRInterfaceInstaller>
    {
#pragma warning disable 0649
        [SerializeField]
        Settings settings;
#pragma warning restore 0649

        public override void InstallBindings()
        {
            Container.BindInstance(settings).IfNotBound();
        }

        [Serializable]
        public class Settings
        {
        }
    }
}