using InterVR.IF.VR.Defines;
using System;
using UnityEngine;
using Zenject;

namespace InterVR.IF.VR.Installer
{
    [CreateAssetMenu(fileName = "IF_VR_Settings", menuName = "InterVR/IF/VR/Settings")]
    public class IF_VR_Installer : ScriptableObjectInstaller<IF_VR_Installer>
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
            public string Name = "IF VR Installer";
        }
    }
}