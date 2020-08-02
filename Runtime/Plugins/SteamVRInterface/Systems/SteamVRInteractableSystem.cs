using Inter.Modules.ToolModule;
using Inter.VR.Components;
using Inter.VR.Defines;
using Inter.VR.Modules.InterVRInterfaces;
using Inter.VR.Plugins.SteamVRInterface.Installer;
using Inter.VR.Installer;
using EcsRx.Collections.Database;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Plugins.ReactiveSystems.Systems;
using EcsRx.Plugins.Views.Components;
using System;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using UnityEngine;
using EcsRx.Events;
using Inter.VR.Events;
using EcsRx.Unity.Extensions;
using Valve.VR;
using Inter.VR.Plugins.SteamVRInterface.Components;
using UniRx.Triggers;

namespace Inter.VR.Plugins.SteamVRInterface.Systems
{
    public class SteamVRInteractableSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(InterVRInteractable), typeof(ViewComponent));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly InterVRInstaller.Settings vrSettings;
        private readonly SteamVRInterfaceInstaller.Settings steamVRSettings;
        private readonly IGameObjectTool gameObjectTool;
        private readonly IInterVRInterface vrInterface;
        private readonly IInterVRHandGrabStatus vrHandGrabStatus;
        private readonly IInterVRHandInterface vrHandInterface;
        private readonly IEntityDatabase entityDatabase;
        private readonly IEventSystem eventSystem;

        public SteamVRInteractableSystem(InterVRInstaller.Settings vrSettings,
            SteamVRInterfaceInstaller.Settings steamVRSettings,
            IGameObjectTool gameObjectTool,
            IInterVRInterface vrInterface,
            IInterVRHandGrabStatus vrHandGrabStatus,
            IInterVRHandInterface vrHandInterface,
            IEntityDatabase entityDatabase,
            IEventSystem eventSystem)
        {
            this.vrSettings = vrSettings;
            this.steamVRSettings = steamVRSettings;
            this.gameObjectTool = gameObjectTool;
            this.vrInterface = vrInterface;
            this.vrHandGrabStatus = vrHandGrabStatus;
            this.vrHandInterface = vrHandInterface;
            this.entityDatabase = entityDatabase;
            this.eventSystem = eventSystem;
        }
        
        float blendToPoseTime = 0.1f;
        float releasePoseBlendTime = 0.2f;

        public void Setup(IEntity entity)
        {
            var subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            var vrInteractable = entity.GetComponent<InterVRInteractable>();
            var steamVRSkeletonPoser = entity.GetUnityComponent<SteamVR_Skeleton_Poser>();
            if (steamVRSkeletonPoser != null)
            {
                entity.AddComponent(new SteamVRInteractable()
                {
                    SkeletonPoser = steamVRSkeletonPoser
                });

                if (vrInteractable.UseHandObjectAttachmentPoint)
                {
                    //Debug.LogWarning("<b>[SteamVR Interaction]</b> SkeletonPose and useHandObjectAttachmentPoint both set at the same time. Ignoring useHandObjectAttachmentPoint.");
                    vrInteractable.UseHandObjectAttachmentPoint = false;
                }
            }

            if (steamVRSettings.HighlightMat == null)
            {
                steamVRSettings.HighlightMat = (Material)Resources.Load("SteamVR_HoverHighlight", typeof(Material));
                if (steamVRSettings.HighlightMat == null)
                    Debug.LogError("<b>[SteamVR Interaction]</b> Hover Highlight Material is missing. Please create a material named 'SteamVR_HoverHighlight' and place it in a Resources folder");
            }

            eventSystem.Receive<OnHandHoverBeginEvent>()
                .Subscribe(evt =>
                {
                    vrInteractable.WasHovering = vrInteractable.IsHovering;
                    vrInteractable.IsHovering = true;
                    vrInteractable.HoveringHandEntities.Add(evt.HandEntity);
                    if (vrInteractable.HighlightOnHover && !vrInteractable.WasHovering)
                    {
                        createHighlightRenderers(entity);
                        updateHighlightRenderers(entity);
                    }
                }).AddTo(subscriptions);

            eventSystem.Receive<OnHandHoverEndEvent>()
                .Subscribe(evt =>
                {
                    vrInteractable.WasHovering = vrInteractable.IsHovering;
                    vrInteractable.HoveringHandEntities.Remove(evt.HandEntity);
                    if (vrInteractable.HoveringHandEntities.Count == 0)
                    {
                        vrInteractable.IsHovering = false;
                        if (vrInteractable.HighlightOnHover && vrInteractable.HighlightHolder != null)
                        {
                            GameObject.Destroy(vrInteractable.HighlightHolder);
                        }
                    }
                }).AddTo(subscriptions);

            eventSystem.Receive<OnAttachedToHandEvent>()
                .Subscribe(evt =>
                {
                    var vrHandEntity = evt.HandEntity;
                    var vrHand = evt.HandEntity.GetComponent<InterVRHand>();
                    var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);
                    if (vrHandRenderModelEntity.HasComponent<SteamVRHandRenderModel>())
                    {
                        var steamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                        steamVRHandRenderModel.Skeleton.BlendToPoser(steamVRSkeletonPoser, blendToPoseTime);
                    }
                    vrInteractable.AttachedToHandEntity = vrHandEntity;
                }).AddTo(subscriptions);

            eventSystem.Receive<OnDetachedToHandEvent>()
                .Subscribe(evt =>
                {
                    var vrHandEntity = evt.HandEntity;
                    var vrHand = evt.HandEntity.GetComponent<InterVRHand>();
                    var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);
                    if (vrHandRenderModelEntity.HasComponent<SteamVRHandRenderModel>())
                    {
                        var steamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                        steamVRHandRenderModel.Skeleton.BlendToSkeleton(releasePoseBlendTime);
                    }
                    vrInteractable.AttachedToHandEntity = null;
                }).AddTo(subscriptions);

            Observable.EveryUpdate()
                .Where(x => vrInteractable.HighlightOnHover)
                .Subscribe(x =>
                {
                    updateHighlightRenderers(entity);
                    if (!vrInteractable.IsHovering && vrInteractable.HighlightHolder != null)
                    {
                        GameObject.Destroy(vrInteractable.HighlightHolder);
                    }
                }).AddTo(subscriptions);

            entity.GetGameObject().OnDisableAsObservable().Subscribe(_ =>
            {
                vrInteractable.IsDestroying = true;

                if (vrInteractable.AttachedToHandEntity != null)
                {
                    vrHandInterface.DetachFromHand(vrInteractable.AttachedToHandEntity, entity, false);
                    var vrHand = vrInteractable.AttachedToHandEntity.GetComponent<InterVRHand>();
                    vrInterface.HandHoverUnlockForce(vrInteractable.AttachedToHandEntity);
                }

                if (vrInteractable.HighlightHolder != null)
                {
                    GameObject.Destroy(vrInteractable.HighlightHolder);
                }
            }).AddTo(subscriptions);
        }

        public void Teardown(IEntity entity)
        {
            var vrInteractable = entity.GetComponent<InterVRInteractable>();
            vrInteractable.IsDestroying = true;

            if (vrInteractable.AttachedToHandEntity != null)
            {
                vrHandInterface.DetachFromHand(vrInteractable.AttachedToHandEntity, entity, false);
                var vrHand = vrInteractable.AttachedToHandEntity.GetComponent<InterVRHand>();
                var vrHandRenderModelEntity = vrInterface.GetHandRenderModelEntity(vrHand.Type);
                if (vrHandRenderModelEntity.HasComponent<SteamVRHandRenderModel>())
                {
                    var steamVRHandRenderModel = vrHandRenderModelEntity.GetComponent<SteamVRHandRenderModel>();
                    steamVRHandRenderModel.Skeleton.BlendToSkeleton(0.1f);
                }
            }

            if (vrInteractable.HighlightHolder != null)
            {
                GameObject.Destroy(vrInteractable.HighlightHolder);
            }

            if (subscriptionsPerEntity.TryGetValue(entity, out List<IDisposable> subscriptions))
            {
                subscriptions.DisposeAll();
                subscriptions.Clear();
                subscriptionsPerEntity.Remove(entity);
            }
        }

        bool shouldIgnoreHighlights(GameObject[] hideHighlights, Component component)
        {
            return shouldIgnore(hideHighlights, component.gameObject);
        }

        bool shouldIgnore(GameObject[] hideHighlights, GameObject check)
        {
            for (int ignoreIndex = 0; ignoreIndex < hideHighlights.Length; ignoreIndex++)
            {
                if (check == hideHighlights[ignoreIndex])
                    return true;
            }

            return false;
        }

        void createHighlightRenderers(IEntity entity)
        {
            var vrInteractable = entity.GetComponent<InterVRInteractable>();
            var vrInteractableView = entity.GetGameObject();
            vrInteractable.ExistingSkinnedRenderers = vrInteractableView.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            vrInteractable.HighlightHolder = new GameObject("Highlighter");
            vrInteractable.HighlightSkinnedRenderers = new SkinnedMeshRenderer[vrInteractable.ExistingSkinnedRenderers.Length];

            for (int skinnedIndex = 0; skinnedIndex < vrInteractable.ExistingSkinnedRenderers.Length; skinnedIndex++)
            {
                SkinnedMeshRenderer existingSkinned = vrInteractable.ExistingSkinnedRenderers[skinnedIndex];

                if (vrInteractable.HideHighlights != null && vrInteractable.HideHighlights.Length > 0)
                {
                    if (shouldIgnoreHighlights(vrInteractable.HideHighlights, existingSkinned))
                        continue;
                }

                GameObject newSkinnedHolder = new GameObject("SkinnedHolder");
                newSkinnedHolder.transform.parent = vrInteractable.HighlightHolder.transform;
                SkinnedMeshRenderer newSkinned = newSkinnedHolder.AddComponent<SkinnedMeshRenderer>();
                Material[] materials = new Material[existingSkinned.sharedMaterials.Length];
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = steamVRSettings.HighlightMat;
                }

                newSkinned.sharedMaterials = materials;
                newSkinned.sharedMesh = existingSkinned.sharedMesh;
                newSkinned.rootBone = existingSkinned.rootBone;
                newSkinned.updateWhenOffscreen = existingSkinned.updateWhenOffscreen;
                newSkinned.bones = existingSkinned.bones;

                vrInteractable.HighlightSkinnedRenderers[skinnedIndex] = newSkinned;
            }

            MeshFilter[] existingFilters = vrInteractableView.GetComponentsInChildren<MeshFilter>(true);
            vrInteractable.ExistingRenderers = new MeshRenderer[existingFilters.Length];
            vrInteractable.HighlightRenderers = new MeshRenderer[existingFilters.Length];

            for (int filterIndex = 0; filterIndex < existingFilters.Length; filterIndex++)
            {
                MeshFilter existingFilter = existingFilters[filterIndex];
                MeshRenderer existingRenderer = existingFilter.GetComponent<MeshRenderer>();

                if (existingFilter == null || existingRenderer == null)
                    continue;

                if (vrInteractable.HideHighlights != null && vrInteractable.HideHighlights.Length > 0)
                {
                    if (shouldIgnoreHighlights(vrInteractable.HideHighlights, existingFilter))
                        continue;
                }

                GameObject newFilterHolder = new GameObject("FilterHolder");
                newFilterHolder.transform.parent = vrInteractable.HighlightHolder.transform;
                MeshFilter newFilter = newFilterHolder.AddComponent<MeshFilter>();
                newFilter.sharedMesh = existingFilter.sharedMesh;
                MeshRenderer newRenderer = newFilterHolder.AddComponent<MeshRenderer>();

                Material[] materials = new Material[existingRenderer.sharedMaterials.Length];
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = steamVRSettings.HighlightMat;
                }
                newRenderer.sharedMaterials = materials;

                vrInteractable.HighlightRenderers[filterIndex] = newRenderer;
                vrInteractable.ExistingRenderers[filterIndex] = existingRenderer;
            }
        }

        void updateHighlightRenderers(IEntity entity)
        {
            var vrInteractable = entity.GetComponent<InterVRInteractable>();
            if (vrInteractable.HighlightHolder == null)
                return;

            for (int skinnedIndex = 0; skinnedIndex < vrInteractable.ExistingSkinnedRenderers.Length; skinnedIndex++)
            {
                SkinnedMeshRenderer existingSkinned = vrInteractable.ExistingSkinnedRenderers[skinnedIndex];
                SkinnedMeshRenderer highlightSkinned = vrInteractable.HighlightSkinnedRenderers[skinnedIndex];

                if (existingSkinned != null && highlightSkinned != null && vrInteractable.AttachedToHandEntity == null)
                {
                    highlightSkinned.transform.position = existingSkinned.transform.position;
                    highlightSkinned.transform.rotation = existingSkinned.transform.rotation;
                    highlightSkinned.transform.localScale = existingSkinned.transform.lossyScale;
                    highlightSkinned.localBounds = existingSkinned.localBounds;
                    highlightSkinned.enabled = vrInteractable.IsHovering && existingSkinned.enabled && existingSkinned.gameObject.activeInHierarchy;

                    int blendShapeCount = existingSkinned.sharedMesh.blendShapeCount;
                    for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
                    {
                        highlightSkinned.SetBlendShapeWeight(blendShapeIndex, existingSkinned.GetBlendShapeWeight(blendShapeIndex));
                    }
                }
                else if (highlightSkinned != null)
                    highlightSkinned.enabled = false;

            }

            for (int rendererIndex = 0; rendererIndex < vrInteractable.HighlightRenderers.Length; rendererIndex++)
            {
                MeshRenderer existingRenderer = vrInteractable.ExistingRenderers[rendererIndex];
                MeshRenderer highlightRenderer = vrInteractable.HighlightRenderers[rendererIndex];

                if (existingRenderer != null && highlightRenderer != null && vrInteractable.AttachedToHandEntity == null)
                {
                    highlightRenderer.transform.position = existingRenderer.transform.position;
                    highlightRenderer.transform.rotation = existingRenderer.transform.rotation;
                    highlightRenderer.transform.localScale = existingRenderer.transform.lossyScale;
                    highlightRenderer.enabled = vrInteractable.IsHovering && existingRenderer.enabled && existingRenderer.gameObject.activeInHierarchy;
                }
                else if (highlightRenderer != null)
                    highlightRenderer.enabled = false;
            }
        }
    }
}