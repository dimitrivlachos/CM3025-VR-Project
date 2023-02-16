using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using MusicForge;

namespace WindowClasses
{
    public abstract class WindowClass
    {
        public List<VisualElement> VisualElements { get; protected set; }
        protected VisualElement root;
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AppConstants.RELATIVEASSETPATH + "/Editor/UI/Editor Defaut Resources/WindowClassStyle.uss");
        public virtual void AddToContainer(VisualElement container)
        {
            root = container;
            root.AddToClassList("background");
            root.styleSheets.Add(styleSheet);
            for (int i = 0; i < VisualElements.Count; i++)
            {
                container.Add(VisualElements[i]);
            }
        }

        public virtual void RemoveFromContainer()
        {
            for (int i = 0; i < VisualElements.Count; i++)
            {
                root.Remove(VisualElements[i]);
            }
        }
    }
}