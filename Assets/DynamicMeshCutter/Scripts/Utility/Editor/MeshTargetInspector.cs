using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace DynamicMeshCutter
{
    [CustomEditor(typeof(MeshTarget))]
    public class MeshTargetInspector : Editor
    {
        MeshTarget _meshTarget;
        int _selection;

        public GUIStyle RichStyle
        {
            get
            {
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.richText = true;
                return style;
            }
        }
        protected virtual void OnEnable()
        {
            _meshTarget = (MeshTarget)target;
        }

        public override void OnInspectorGUI()
        {
            if (_meshTarget.DynamicRagdoll == null)
                _meshTarget.DynamicRagdoll = _meshTarget.GetComponentInParent<DynamicRagdoll>();

            if (_meshTarget.Animator == null)
                _meshTarget.Animator = _meshTarget.GetComponentInParent<Animator>();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Basic");
            _meshTarget.GameobjectRoot = EditorGUILayout.ObjectField("GameObject Root", _meshTarget.GameobjectRoot, typeof(GameObject), true) as GameObject;
            _meshTarget.OverrideFaceMaterial = EditorGUILayout.ObjectField("Face Material", _meshTarget.OverrideFaceMaterial, typeof(Material), false) as Material;
            _meshTarget.SeparateMeshes = EditorGUILayout.Toggle("Seperate Meshes", _meshTarget.SeparateMeshes);
            _meshTarget.ApplyTranslation = EditorGUILayout.Toggle("Apply Translation", _meshTarget.ApplyTranslation);
            EditorGUILayout.EndVertical();

            string[] side = new string[2] { "Lower Sides", "Upper Sides" };
            for(int i = 1; i >= 0; i--)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{side[i]}");
                GUILayout.FlexibleSpace();
                EditorGUIUtility.labelWidth = 45;
                _meshTarget.Inherit[i] = EditorGUILayout.Toggle("Inherit", _meshTarget.Inherit[i]);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
                _meshTarget.DefaultBehaviour[i] = (Behaviour)EditorGUILayout.EnumPopup("Default Behaviour", _meshTarget.DefaultBehaviour[i]);

                switch (_meshTarget.DefaultBehaviour[i])
                {
                    case Behaviour.Stone:
                        _meshTarget.CreateRigidbody[i] = EditorGUILayout.Toggle("Add Rigidbody", _meshTarget.CreateRigidbody[i]);
                        _meshTarget.CreateMeshCollider[i] = EditorGUILayout.Toggle("Add MeshCollider", _meshTarget.CreateMeshCollider[i]);
                        break;
                    case Behaviour.Ragdoll:
                        _meshTarget.DynamicRagdoll = EditorGUILayout.ObjectField("Dynamic Ragdoll", _meshTarget.DynamicRagdoll, typeof(DynamicRagdoll), false) as DynamicRagdoll;
                        if (_meshTarget.DynamicRagdoll == null)
                        {
                            EditorGUILayout.LabelField("<color=red> Dynamic Ragdoll in parent missing.</color>", RichStyle);
                        }
                        else
                        {
                            _meshTarget.Physics[i] = (RagdollPhysics) EditorGUILayout.EnumPopup("Physics", _meshTarget.Physics[i]);
                        }
                        break;
                    case Behaviour.Animation:
                        _meshTarget.Animator = EditorGUILayout.ObjectField("Animator", _meshTarget.Animator, typeof(Animator), false) as Animator;
                        if (_meshTarget.Animator == null)
                        {
                            EditorGUILayout.LabelField("<color=red> Animator in parent missing.</color>", RichStyle);
                        }
                        break;
                }

                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Group Behaviours");

            if (_meshTarget.DynamicRagdoll == null)
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField("Add a dynamic ragdoll to define group behaviours");
            }
            else
            {
                List<DynamicRagdollPart> parts = _meshTarget.DynamicRagdoll.Parts.Values.ToList();
                _selection = EditorGUILayout.Popup("Parts", _selection, parts.Select(p => p.name).ToArray());

                foreach (var group in _meshTarget.GroupBehaviours)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    group.Name = EditorGUILayout.TextField(group.Name);
                    group.Condition = (GroupCondition)EditorGUILayout.EnumPopup(group.Condition);
                    group.Behaviour = (Behaviour)EditorGUILayout.EnumPopup(group.Behaviour);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add Part"))
                    {
                        var part = parts[_selection];
                        if (!group.Parts.Contains(part))
                            group.Parts.Add(part);
                    }
                    EditorGUILayout.EndHorizontal();

                    int remove = -1;
                    for (int i = 0; i < group.Parts.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{i}. {group.Parts[i].Name}");
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Remove"))
                        {
                            remove = i;
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (remove > -1)
                        group.Parts.RemoveAt(remove);


                    group.Indices = group.Parts.Select(p => p.Index).ToList();

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+"))
                {
                    _meshTarget.GroupBehaviours.Add(new GroupBehaviours());
                }
                if (_meshTarget.GroupBehaviours.Count == 0)
                    GUI.enabled = false;
                if (GUILayout.Button("-"))
                {
                    if (_meshTarget.GroupBehaviours.Count > 0)
                        _meshTarget.GroupBehaviours.RemoveAt(_meshTarget.GroupBehaviours.Count - 1);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            GUI.enabled = true;

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_meshTarget);
        }
    }

}

