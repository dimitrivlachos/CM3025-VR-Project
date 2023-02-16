using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DynamicMeshCutter
{
    //}
    public class DynamicRagdoll : MonoBehaviour , ISerializationCallbackReceiver
    {

        public int[] Assignments; //the array thats actually being read by dynamic mesh cutter. what part does each vertex belong to?
        public Dictionary<int, DynamicRagdollPart> Parts = new Dictionary<int, DynamicRagdollPart>();

        [SerializeField] List<DynamicRagdollPart> _values = new List<DynamicRagdollPart>();
        [SerializeField] List<int> _keys = new List<int>();

        public Renderer Renderer;
        public bool IsRagdollKinematic;

        public void SetRagdollKinematic(bool value)
        {   
            foreach(var part in Parts)
            {
                if (part.Value.Rigidbody != null)
                {
                    if (value)
                    {
                        part.Value.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    }
                    part.Value.Rigidbody.isKinematic = value;
                    if(!value)
                        part.Value.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
            }
        }


#if UNITY_EDITOR
        public bool Debug;
        [Range(0.5f,2f)]
        public float Scale = 1;
        void OnDrawGizmosSelected()
        {
            if (Debug)
            {
                DebugVertices();
                DebugNames();
            }
        }

        void DebugNames()
        {
            foreach(var entry in Parts)
            {
                DynamicRagdollPart part = entry.Value;
                if (!string.IsNullOrEmpty(part.Name) && part.Colliders != null)
                {
                    if(part.Colliders != null && part.Colliders.Length > 0 && part.Colliders[0] != null)
                    {
                        Transform t = part.Colliders[0].transform;
                        Vector3 pos = t.position;

                        CapsuleCollider capsule = part.Colliders[0] as CapsuleCollider;
                        BoxCollider box = part.Colliders[0] as BoxCollider;
                        SphereCollider sphere = part.Colliders[0] as SphereCollider;

                        if(capsule != null)
                        {
                            pos = t.TransformPoint(capsule.center);
                        }
                        else if( box != null)
                        {
                            pos = t.TransformPoint(box.center);
                        }
                        else if( sphere != null)
                        {
                            pos = t.TransformPoint(sphere.center);
                        }
                        GUIStyle style = new GUIStyle(EditorStyles.label);
                        style.normal.textColor = part.Color;
                        Handles.Label(pos, $"{entry.Key}: {part.Name}",style);
                    }
                    
                }
            }
        }
        void DebugVertices()
        {
            if (Assignments == null)
                return;

            if (Renderer != null)
            {
                Mesh mesh = TryGetMesh(Renderer);
                Matrix4x4 localToWorld = Renderer.transform.localToWorldMatrix;
                Vector3[] vertices = mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Color color = Color.white;
                    color.a = 0.1f;
                    Vector3 worldPoint = localToWorld.MultiplyPoint3x4(vertices[i]);
                    int index = Assignments[i];
                    if (index > -1)
                    {
                        if(Parts.ContainsKey(index))
                            color = Parts[index].Color;
                    }
                    Handles.color = color;
                    Handles.DrawWireCube(worldPoint, Vector3.one * 0.01f);
                }
            }

        }
        public Mesh TryGetMesh(Renderer renderer)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                return meshFilter.mesh;
            }
            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null)
            {
                Mesh mesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(mesh);
                return mesh;
            }

            UnityEngine.Debug.LogError("Couldn't retrieve Mesh");
            return null;
        }

#endif

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach (var entry in Parts)
            {
                _keys.Add(entry.Key);
                _values.Add(entry.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Parts.Clear();
            for (int i = 0; i < _keys.Count; i++)
            {
                Parts.Add(_keys[i], _values[i]);
            }
        }
    }
}
