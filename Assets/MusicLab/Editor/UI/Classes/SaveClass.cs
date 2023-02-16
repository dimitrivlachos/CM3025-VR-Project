using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using MusicForge;
using SavWav;
using WindowClasses;

namespace SavWav
{
    public class SaveClass : WindowClass
    {
        private static string _path;

        private static TextField _songPATH;
        private VisualElement _firtsRow;
        private VisualElement _secondRow;
        private VisualElement _thirdRow;
        private PlayPauseBtn _playPauseBtn;
        private Button _returnBtn;
        private Button _saveBtn;
        private Label _tile;
        private Label _sindriContact_Lbl;

        private SongCreatorClass _songCreatorClass;
        public SongCreatorClass SongCreatorClass { set { _songCreatorClass = value; } }
        public Image songResult;

        public SaveClass()
        {
            VisualElements = new List<VisualElement>();

            _firtsRow = new VisualElement();
            _secondRow = new VisualElement();
            _thirdRow = new VisualElement();
            _playPauseBtn = new PlayPauseBtn(_firtsRow);
            songResult = new Image();
            _saveBtn = new Button() { text = "Save" };
            _returnBtn = new Button() { text = "Return" };
            _songPATH = new TextField();
            _songPATH.value = AppConstants.RELATIVEASSETPATH + "/Songs/01Song.wav";
            _tile = new Label("Music Lab");
            _sindriContact_Lbl = new Label("Support at: sindri.studios.info@gmail.com");

            songResult.scaleMode = ScaleMode.ScaleToFit;

            _returnBtn.RegisterCallback<MouseUpEvent>(ReturnToSongCreator);
            _saveBtn.RegisterCallback<MouseUpEvent>(OpenExamineWindow);

            _tile.AddToClassList("title");
            _sindriContact_Lbl.AddToClassList("info");
            _returnBtn.AddToClassList("prevBtn");
            _saveBtn.AddToClassList("nextBtn");

            _firtsRow.AddToClassList("horizontal-container");
            _secondRow.AddToClassList("horizontal-container");
            _thirdRow.AddToClassList("horizontal-container");

            _firtsRow.Add(songResult);
            _secondRow.Add(_songPATH);
            _thirdRow.Add(_returnBtn);
            _thirdRow.Add(_saveBtn);

            VisualElements.Add(_tile);
            VisualElements.Add(_firtsRow);
            VisualElements.Add(_secondRow);
            VisualElements.Add(_thirdRow);
            VisualElements.Add(_sindriContact_Lbl);
        }

        #region CallBacks
        static void SaveSong()
        {
            if (_path == null || _path.CompareTo("") == 0)
                _path = _songPATH.value;
            if (MusicForge.Music_Forge.Clip != null)
            {
                SavWav.Save(_path, Music_Forge.Clip);
                AssetDatabase.Refresh();
            }
            else Debug.Log("No song created");
        }

        private void ReturnToSongCreator(MouseUpEvent evt)
        {
            InspectorClip.InspectorClip.StopAllClips();
            if (_playPauseBtn.GetIsPlaying)
                _playPauseBtn.ChangeStateOfSong();
            RemoveFromContainer();
            _songCreatorClass.AddToContainer(root);
        }

        private void OpenExamineWindow(MouseUpEvent evt)
        {
            _path = EditorUtility.SaveFilePanel(
                "Save texture as PNG",
                "",
                "Song.wav",
                "wav");
            if (_path != null && _path != "")
            {
                _songPATH.value = _path;
            }
            else
            {
                _path = _songPATH.value;
            }
            SaveSong();
        }
        #endregion
    }
}