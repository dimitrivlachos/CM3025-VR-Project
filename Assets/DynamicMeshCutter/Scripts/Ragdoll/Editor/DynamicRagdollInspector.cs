using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DynamicMeshCutter
{
    [CustomEditor(typeof(DynamicRagdoll))]
    public class DynamicRagdollInspector : Editor
    {
        DynamicRagdoll _rd;

        private void OnEnable()
        {
            _rd = (DynamicRagdoll)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Auto Calculate"))
            {
                Cleanse();
                FetchPhysics();
                AssignVerticesToGroups();
                GuessPartNames();
            }
            if (GUILayout.Button("Auto Color"))
            {
                AutoColor();
            }
            if (GUILayout.Button("Clean All"))
            {
                CleanseAll();
            }

            _rd.Debug = EditorGUILayout.Toggle("Debug Vertices",_rd.Debug);
            if (_rd.Debug)
            {
                EditorGUILayout.LabelField("Editor can slow down when debugging mesh with high amount of vertices");
            }
             bool isKinematic = EditorGUILayout.Toggle("IsRagdollKinematic",_rd.IsRagdollKinematic);
            if(isKinematic != _rd.IsRagdollKinematic)
            {
                _rd.IsRagdollKinematic = isKinematic;
                _rd.SetRagdollKinematic(isKinematic);
            }



            _rd.Scale = EditorGUILayout.FloatField("Grace Scale",_rd.Scale);
            _rd.Renderer = EditorGUILayout.ObjectField("Renderer", _rd.Renderer, typeof(Renderer), true) as Renderer;


            int verticesInside = 0;
            foreach (var part in _rd.Parts)
            {
                if(part.Value != null)
                {
                    if(part.Value.Vertices != null)
                        verticesInside += part.Value.Vertices.Length;
                }
            }

            if(_rd.Assignments != null)
            {
                float percent = ((float)verticesInside / (float)_rd.Assignments.Length) * 100f;
                EditorGUILayout.LabelField($"Assigned vertices : {verticesInside}/{_rd.Assignments.Length}. Parts: {_rd.Parts.Count}. Covered vertices: {percent}%");
            }

            foreach(var part in _rd.Parts)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                part.Value.Name = EditorGUILayout.TextField("Name", part.Value.Name);
                part.Value.Rigidbody = EditorGUILayout.ObjectField("Rigidbody", part.Value.Rigidbody, typeof(Rigidbody), true) as Rigidbody;
                part.Value.Joint = EditorGUILayout.ObjectField("CharacterJoint", part.Value.Joint, typeof(CharacterJoint), true) as CharacterJoint;
                if (part.Value.Colliders != null)
                {
                    for(int j =0;j<part.Value.Colliders.Length;j++)
                    {
                        part.Value.Colliders[j] = EditorGUILayout.ObjectField("Collider", part.Value.Colliders[j], typeof(Collider), true) as Collider;
                    }
                }
                part.Value.Color = EditorGUILayout.ColorField(part.Value.Color);
                int vLength = part.Value.Vertices != null ? part.Value.Vertices.Length : 0;
                EditorGUILayout.LabelField($"Amount of vertices: {vLength}");
                EditorGUILayout.EndVertical();
            }


            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_rd);
        }

        void Cleanse()
        {
            foreach(var key in _rd.Parts.Keys.ToArray())
            {
                if(_rd.Parts[key] == null)
                {
                    _rd.Parts.Remove(key);
                }

                if (_rd.Parts[key].Colliders == null)
                    _rd.Parts.Remove(key);
            }
        }

        void CleanseAll()
        {
            foreach (var key in _rd.Parts.Keys.ToArray())
            {
                DestroyImmediate(_rd.Parts[key]);
            }
            _rd.Parts.Clear();
        }
        void AutoColor()
        {
            foreach(var part in _rd.Parts)
            {
                Color c = UnityEngine.Random.ColorHSV(0, 1, 0.5f, 1, 1, 1);
                part.Value.Color = c;
            }
        }

        public void AssignVerticesToGroups()
        {
            if(_rd.Renderer == null)
            {
                _rd.Renderer = _rd.GetComponentInChildren<Renderer>();
            }
            if(_rd.Renderer == null)
            {
                Debug.Log("Can't continue since no mesh was found");
                return;
            }

            Mesh mesh = _rd.TryGetMesh(_rd.Renderer);
            if(mesh != null)
            {
                Dictionary<int, List<Vector3>> grouping = new Dictionary<int, List<Vector3>>();

                Matrix4x4 localToWorld = _rd.Renderer.transform.localToWorldMatrix;

                Vector3[] vertices = mesh.vertices;
                _rd.Assignments = new int[vertices.Length];

                for(int i = 0;i< vertices.Length; i++)
                {
                    Vector3 worldPoint = localToWorld.MultiplyPoint3x4(vertices[i]);

                    _rd.Assignments[i] = -1;
                    foreach(var entry in _rd.Parts)
                    {
                        int index = entry.Key;
                        var part = entry.Value;
                        if (part.Colliders == null)
                            continue;

                        if (IsInsideColliders(part.Colliders, worldPoint,_rd.Scale))
                        {
                            if (!grouping.ContainsKey(index))
                                grouping.Add(index, new List<Vector3>());
                            grouping[index].Add(vertices[i]);
                            _rd.Assignments[i] = index;
                            break;
                        }
                    }
                }
                
                foreach(var group in grouping)
                {
                    _rd.Parts[group.Key].Vertices = group.Value.ToArray();
                }
            }
        }

        void GuessPartNames()
        {
            foreach(var part in _rd.Parts)
            {
                if (string.IsNullOrEmpty(part.Value.Name))
                {
                    if(part.Value.Rigidbody != null)
                    {
                        part.Value.Name = part.Value.Rigidbody.gameObject.name;
                    }
                }
            }
        }

        public bool IsInsideColliders(Collider[] colliders,Vector3 worldPoint, float scale = 1f)
        {
            for(int i = 0; i < colliders.Length; i++)
            {
                if (IsInsideCollider(colliders[i], worldPoint, scale))
                    return true;
            }
            return false;
        }
        public bool IsInsideCollider(Collider collider, Vector3 worldPoint, float scale = 1f)
        {
            if (collider == null)
                return false;

            Transform t = collider.transform;
            Vector3 center = collider.bounds.center;

            if (float.IsNaN(center.x) || float.IsNaN(center.y) || float.IsNaN(center.z))
            {
                Debug.LogWarning("Aborting");
                return false;
            }

            CapsuleCollider capsule = collider as CapsuleCollider;
            BoxCollider box = collider as BoxCollider;
            SphereCollider sphere = collider as SphereCollider;
            if (capsule != null)
            {
                //center = t.TransformPoint(capsule.center);
                capsule.radius *= scale;
            }
            else if(box != null)
            {
                //center = t.TransformPoint(box.center);
                box.size *= scale;
            }
            else if(sphere != null)
            {
                //center = t.TransformPoint(sphere.center);
                sphere.radius *= scale;
            }

            Vector3 worldOffset = center - worldPoint;

            if(worldOffset.magnitude == 0)
            {
                worldOffset = Vector3.up;
            }


            Ray inputRay = new Ray(worldPoint, Vector3.Normalize(worldOffset));
            //Ray inputRay = new Ray(t.InverseTransformPoint(worldPoint), t.InverseTransformDirection(worldOffset));

             bool isInside = !collider.Raycast(inputRay, out RaycastHit rHit, worldOffset.magnitude * 1.1f);

            if (capsule != null)
            {
                capsule.radius /= scale;
            }
            else if (box != null)
            {
                box.size /= scale;
            }
            else if (sphere != null)
            {
                sphere.radius /= scale;
            }
            return isInside;

        }
        public void FetchPhysics()
        {
            
            Rigidbody[] rbs = _rd.GetComponentsInChildren<Rigidbody>();
            for(int i = 0; i < rbs.Length; i++)
            {
                bool hasBeenAdded = false;
                foreach(var entry in _rd.Parts)
                {
                    if (entry.Value.Rigidbody == rbs[i])
                    {
                        hasBeenAdded = true;
                        entry.Value.Colliders = GetMyComponentsInChildren<Collider, Rigidbody>(rbs[i]);
                        entry.Value.Joint = rbs[i].GetComponent<CharacterJoint>();
                        entry.Value.Index = i;
                        break;
                    }
                }

                if (!hasBeenAdded)
                {
                    var myColliders = GetMyComponentsInChildren<Collider, Rigidbody>(rbs[i]);
                    var joint = rbs[i].GetComponent<CharacterJoint>();

                    var part = rbs[i].gameObject.GetComponent<DynamicRagdollPart>();
                    if(part == null)
                        part = rbs[i].gameObject.AddComponent<DynamicRagdollPart>();

                    part.Joint = joint;
                    part.Rigidbody = rbs[i];
                    part.Colliders = myColliders;
                    part.Index = i;
                    _rd.Parts.Add(i, part);
                }

                
            }
        }


        T[] GetMyComponentsInChildren<T, P>(Component comp, bool includeInactive = false)
        {
            List<T> found = new List<T>();
            if (comp == null)
                return found.ToArray();

            T[] children = comp.GetComponentsInChildren<T>(includeInactive);

            for (int i = 0; i < children.Length; i++)
            {
                Component childComp = children[i] as Component;
                if (childComp == null)
                    continue;

                var parents = childComp.GetComponentsInParent<P>(includeInactive);
                if (parents.Length > 0)
                {
                    P foundParent = parents[0];
                    if (foundParent is Component)
                    {
                        Component c = foundParent as Component;
                        if (comp.transform == c.transform)
                            found.Add(children[i]);
                    }

                }
            }
            return found.ToArray();
        }

    }
}

