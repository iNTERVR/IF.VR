using System;
using UnityEngine;
using Zenject;

namespace Inter.VR.VRGlovePrototype.Installer
{
    [CreateAssetMenu(fileName = "VRGlovePrototypeSettings", menuName = "Inter/Mods/VRGlovePrototype/Settings")]
    public class VRGlovePrototypeInstaller : ScriptableObjectInstaller<VRGlovePrototypeInstaller>
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
            public string Name = "VGloveRPrototype Installer";
            public float HandYawOffsetLeft;
            public float HandYawOffsetRight;
        }
    }
}