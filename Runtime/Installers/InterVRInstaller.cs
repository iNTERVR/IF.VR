using Inter.VR.Defines;
using System;
using UnityEngine;
using Zenject;

namespace Inter.VR.Installer
{
    [CreateAssetMenu(fileName = "InterVRSettings", menuName = "Inter/VR/Settings")]
    public class InterVRInstaller : ScriptableObjectInstaller<InterVRInstaller>
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
            public string Name = "Inter VR Installer";
        }
    }
}