using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DynamicMeshCutter
{
    public class MeshTarget : MonoBehaviour
    {
        //basic both
        [Tooltip("This represents the root of the object and the gameobject that will be destroyed if \"DestroyTargets\" is enabled in the cutter. ")]
        public GameObject GameobjectRoot;
        public Material OverrideFaceMaterial;
        public bool SeparateMeshes;
        public bool ApplyTranslation = true;
        
        //basic single
        public Behaviour[] DefaultBehaviour = new Behaviour[2] { Behaviour.Stone, Behaviour.Stone };

        public DynamicRagdoll DynamicRagdoll;
        public Animator Animator;

        public List<GroupBehaviours> GroupBehaviours = new List<GroupBehaviours>();

        //stone
        public bool[] CreateRigidbody = new bool[2] { true, true };
        public bool[] CreateMeshCollider = new bool[2] { true, true };

        //ragdoll
        public RagdollPhysics[] Physics = new RagdollPhysics[2] { RagdollPhysics.NonKinematic, RagdollPhysics.NonKinematic};
        

        //inheritance
        public bool[] Inherit = new bool[2] { false, false };

        private MeshRenderer _meshRenderer;
        private SkinnedMeshRenderer _skinnedMeshRenderer;


        private void OnEnable()
        {
            MeshTargetShephard.RegisterMeshTarget(this);
        }

        private void OnDisable()
        {
            MeshTargetShephard.UnRegisterMeshTarget(this);
        }

        public MeshRenderer MeshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                    _meshRenderer = GetComponent<MeshRenderer>();
                return _meshRenderer;
            }
        }
        public SkinnedMeshRenderer SkinnedMeshRenderer
        {
            get
            {
                if (_skinnedMeshRenderer == null)
                    _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
                return _skinnedMeshRenderer;
            }
        }
        public bool IsSkinned => SkinnedMeshRenderer != null ? true : false;
        public Renderer Renderer => IsSkinned ? (Renderer) SkinnedMeshRenderer: (Renderer)MeshRenderer;
        public Material FaceMaterial
        {
            get
            {
                if (OverrideFaceMaterial != null)
                    return OverrideFaceMaterial;
                return TryGetMaterial();

            }
        }
        public bool RequireLocal => GetComponent<SkinnedMeshRenderer>() != null;

        protected virtual void Start()
        {
            if (DynamicRagdoll == null)
                DynamicRagdoll = GetComponentInParent<DynamicRagdoll>();
        }
        public Material TryGetMaterial()
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
                return renderer.material;
            var sRenderer = GetComponent<SkinnedMeshRenderer>();
            if (sRenderer != null)
                return sRenderer.material;
            return null;
        }
    }
}