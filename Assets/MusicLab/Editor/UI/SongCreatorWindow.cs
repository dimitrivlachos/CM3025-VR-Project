using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using InspectorClip;
using SavWav;

namespace WindowClasses
{
    public class SongCreatorWindow : EditorWindow
    {
        private static WelcomeClass _welcomeClass;
        private static SongCreatorClass _creatorClass;
        private static SaveClass _saveClass;

        [MenuItem("Tools/Music Lab/Song Creator")]
        public static void ShowExample()
        {
            SongCreatorWindow wnd = GetWindow<SongCreatorWindow>();
            wnd.titleContent = new GUIContent("Song Creator");
        }

        public void CreateGUI()
        {
            _saveClass = new SaveClass();
            _creatorClass = new SongCreatorClass(_saveClass);
            _welcomeClass = new WelcomeClass("Welcome to ", _creatorClass);
            _saveClass.SongCreatorClass = _creatorClass;
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;


            _welcomeClass.AddToContainer(root);


        }

        private void OnDestroy()
        {
            InspectorClip.InspectorClip.StopAllClips();
        }
    }
}