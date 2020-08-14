using InterVR.Unity.SDK.SteamVR.Defines;
using System;
using UnityEngine;
using Zenject;

namespace InterVR.Unity.SDK.SteamVR.Installer
{
    [CreateAssetMenu(fileName = "SteamVRSettings", menuName = "InterVR/SteamVR/Settings")]
    public class SteamVRInstaller : ScriptableObjectInstaller<SteamVRInstaller>
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
            public string Name = "SteamVR Installer";
        }
    }
}