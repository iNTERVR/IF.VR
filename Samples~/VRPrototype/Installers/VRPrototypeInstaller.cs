using System;
using UnityEngine;
using Zenject;

namespace Inter.VR.VRPrototype.Installer
{
    [CreateAssetMenu(fileName = "VRPrototypeSettings", menuName = "Inter/Mods/VRPrototype/Settings")]
    public class VRPrototypeInstaller : ScriptableObjectInstaller<VRPrototypeInstaller>
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
            public string Name = "VRPrototype Installer";
        }
    }
}