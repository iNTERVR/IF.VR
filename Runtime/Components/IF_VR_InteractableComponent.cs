using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Extensions;
using InterVR.IF.VR.Defines;
using System.Collections.Generic;
using UnityEngine;

namespace InterVR.IF.VR.Components
{
    public class IF_VR_Interactable : IComponent
    {
        public IF_VR_IgnoreHandHoveringType IgnoreHandHovering { get; set; }
        public bool HideControllerOnAttach { get; set; }
        public int HoverPriority { get; set; }
        public IEntity AttachedToHandEntity { get; set; }
        public bool HighlightOnHover { get; set; }
        public bool UseHandObjectAttachmentPoint { get; set; }
        public bool HandFollowTransform { get; set; }

        public GameObject[] HideHighlights;
        public MeshRenderer[] HighlightRenderers;
        public MeshRenderer[] ExistingRenderers;
        public GameObject HighlightHolder;
        public SkinnedMeshRenderer[] HighlightSkinnedRenderers;
        public SkinnedMeshRenderer[] ExistingSkinnedRenderers;
        public List<IEntity> HoveringHandEntities = new List<IEntity>();
        public IEntity HoveringHandEntity
        {
            get
            {
                if (HoveringHandEntities.Count > 0)
                    return HoveringHandEntities[0];
                return null;
            }
        }

        public bool IsDestroying { get; set; }
        public bool IsHovering { get; set; }
        public bool WasHovering { get; set; }
    }

    public class IF_VR_InteractableComponent : MonoBehaviour, IConvertToEntity
    {
        [Tooltip("Hide the controller part of the hand on attachment and show on detach")]
        public bool HideControllerOnAttach;
        [Tooltip("Higher is better")]
        public int HoverPriority;
        public IF_VR_IgnoreHandHoveringType IgnoreHandHovering;
        public bool HighlightOnHover = true;
        [Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
        public GameObject[] HideHighlights;
        [Tooltip("Specify whether you want to snap to the hand's object attachment point, or just the raw hand")]
        public bool UseHandObjectAttachmentPoint = true;
        [Tooltip("Should the rendered hand lock on to and follow the object")]
        public bool HandFollowTransform = true;

        public void Convert(IEntity entity, IComponent component = null)
        {
            var c = component == null ? new IF_VR_Interactable() : component as IF_VR_Interactable;

            c.HideControllerOnAttach = HideControllerOnAttach;
            c.HoverPriority = HoverPriority;
            c.IgnoreHandHovering = IgnoreHandHovering;
            c.HighlightOnHover = HighlightOnHover;
            c.HideHighlights = HideHighlights;
            c.UseHandObjectAttachmentPoint = UseHandObjectAttachmentPoint;
            c.HandFollowTransform = HandFollowTransform;
            
            entity.AddComponentSafe(c);

            Destroy(this);
        }
    }
}