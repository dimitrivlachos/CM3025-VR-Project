using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MusicForge;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using MLScriptableObjs;
using SavWav;
using WFC;

namespace WindowClasses
{
    public class SongCreatorClass : WindowClass
    {
        private Label _titleLbl;
        private Label _genreLbl;
        private PopupField<Genre_Data> _genrePopup;
        private Label _moodLbl;
        private PopupField<Mood_Data> _moodPopup;
        private Button _createButton;

        private static Music_Library _currentLibrary;
        private static Genre_Data _currentGenre;
        private static Mood_Data _currentMood;
        private Label _sindriContact_Lbl;

        private SaveClass _saveClass;

        public static Music_Library CurrentLibrary { get { return _currentLibrary; } }
        public static Genre_Data CurrentGenre { get { return _currentGenre; } }
        public static Mood_Data CurrenMood { get { return _currentMood; } }



        public SongCreatorClass(SaveClass saveClass)
        {
            _saveClass = saveClass;
            VisualElements = new List<VisualElement>();
            _titleLbl = new Label("Music Lab");
            _genreLbl = new Label("Song genre");
            _moodLbl = new Label("Song mood");

            LoadLibrary();

            _createButton = new Button { text = "Create Song" };

            _createButton.RegisterCallback<MouseUpEvent>(Music_Forge.CreateSong);
            _createButton.RegisterCallback<MouseUpEvent>(moveToCreatorClass);
            _genrePopup.RegisterCallback<ChangeEvent<Genre_Data>>(OnGenreChange);
            _moodPopup.RegisterCallback<ChangeEvent<Mood_Data>>(OnMoodChange);



            _sindriContact_Lbl = new Label("Support at: sindri.studios.info@gmail.com");

            _titleLbl.AddToClassList("title");
            _sindriContact_Lbl.AddToClassList("info");
            _createButton.AddToClassList("nextBtn");
            _moodLbl.AddToClassList("subtitle");
            _genreLbl.AddToClassList("subtitle");

            VisualElements.Add(_titleLbl);
            VisualElements.Add(_genreLbl);
            VisualElements.Add(_genrePopup);
            VisualElements.Add(_moodLbl);
            VisualElements.Add(_moodPopup);
            VisualElements.Add(_createButton);
            VisualElements.Add(_sindriContact_Lbl);


        }

        #region Events

        private void LoadLibrary()
        {
            _currentLibrary = RulesLoader.LibraryFromEditorAssets("VanillaLibrary");

            _genrePopup = new PopupField<Genre_Data>(_currentLibrary.GenreList, 0);
            _currentGenre = _genrePopup.value;

            _moodPopup = new PopupField<Mood_Data>(_currentGenre.moodList, 0);
            _currentMood = _moodPopup.value;
        }

        private void OnGenreChange(ChangeEvent<Genre_Data> evt)
        {
            _currentGenre = _genrePopup.value;
            _moodPopup = new PopupField<Mood_Data>(_currentGenre.moodList, 0);
        }

        private void OnMoodChange(ChangeEvent<Mood_Data> evt)
        {
            _currentMood = _moodPopup.value;
        }

        void moveToCreatorClass(MouseUpEvent evt)
        {
            RemoveFromContainer();
            _saveClass.songResult.image = Music_Forge.ResImage;
            _saveClass.AddToContainer(root);
        }
        #endregion
    }
}