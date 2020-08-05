using EcsRx.Groups;
using EcsRx.Systems;
using EcsRx.Groups.Observable;
using Inter.VR.Modules.InterVRInterfaces;
using UniRx;
using Inter.VR.Plugins.SteamVRInterface.Extensions;
using System.Collections.Generic;
using System;
using EcsRx.Extensions;
using Inter.VR.Plugins.SteamVRInterface.Installer;
using Valve.VR;
using UnityEngine;

namespace Inter.VR.Plugins.SteamVRInterface.Systems
{
    public class SteamVRSystem : IManualSystem
    {
        public IGroup Group => new EmptyGroup();

        private List<IDisposable> subscriptions = new List<IDisposable>();
        private readonly IInterVRInterface vrInterface;
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;

        public SteamVRSystem(IInterVRInterface vrInterface,
            SteamVRInterfaceInstaller.Settings steamVRSettings)
        {
            this.vrInterface = vrInterface;
            this.steamVRSettings = steamVRSettings;
        }

        public void StartSystem(IObservableGroup observableGroup)
        {
            Observable.EveryUpdate()
                .First()
                .Where(x => vrInterface.SteamVRInitializedAndValid())
                .Subscribe(x =>
                {
                    vrInterface.HeadsetOnHead.Value = getHeadsetOnHead();
                }).AddTo(subscriptions);
        }

        public void StopSystem(IObservableGroup observableGroup)
        {
            subscriptions.DisposeAll();
        }

        bool getHeadsetOnHead()
        {
            bool ret = false;
            var headsetOnHead = steamVRSettings.HeadsetOnHead;
            if (headsetOnHead != null)
            {
                if (headsetOnHead.GetStateUp(SteamVR_Input_Sources.Head))
                {
                    ret = false;
                    Debug.Log("<b>SteamVR Interaction System</b> Headset removed");
                }
                else if (headsetOnHead.GetStateDown(SteamVR_Input_Sources.Head))
                {
                    ret = true;
                    Debug.Log("<b>SteamVR Interaction System</b> Headset placed on head");
                }
            }
            return ret;
        }
    }
}