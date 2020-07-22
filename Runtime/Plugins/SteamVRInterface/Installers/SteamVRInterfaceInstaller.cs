using Inter.VR.Defines;
using System;
using UnityEngine;
using Valve.VR;
using Zenject;

namespace Inter.VR.Plugins.SteamVRInterface.Installer
{
    [CreateAssetMenu(fileName = "SteamVRInterfaceSettings", menuName = "Inter/VR/Plugin/SteamVRInterface/Settings")]
    public class SteamVRInterfaceInstaller : ScriptableObjectInstaller<SteamVRInterfaceInstaller>
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
            [Header("Prefabs")]
            public GameObject SteamVRConfig;
            public GameObject PlayVolume;
            public Material HighlightMat;

            [Header("Hands")]
            public GameObject LeftHandRendererModelPrefab;
            public GameObject RightHandRendererModelPrefab;

            [Header("Controller")]
            public GameObject ControllerRendererModelPrefab;

            [Header("Actions")]
            public SteamVR_Action_Boolean HeadsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
            public SteamVR_Action_Boolean GrabPinchAction = SteamVR_Input.GetBooleanAction("GrabPinch");
            public SteamVR_Action_Boolean GrabGripAction = SteamVR_Input.GetBooleanAction("GrabGrip");
        }
    }
}