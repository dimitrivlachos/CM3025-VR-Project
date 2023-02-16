using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using InspectorClip;

namespace WindowClasses
{
    public class PlayPauseBtn : VisualElement
    {
        private Button _playBtn;
        private Button _pauseBtn;
        private VisualElement _container;
        private bool _isPlaying;

        public bool GetIsPlaying
        {
            get { return _isPlaying; }
        }

        public PlayPauseBtn(VisualElement parent)
        {
            _isPlaying = false;
            _container = new VisualElement();
            _playBtn = new Button();
            _pauseBtn = new Button();

            _playBtn.AddToClassList("playBtn");
            _pauseBtn.AddToClassList("pauseBtn");

            _playBtn.RegisterCallback<MouseUpEvent>(ChangeStateOfSong);
            _pauseBtn.RegisterCallback<MouseUpEvent>(ChangeStateOfSong);

            _container.Add(_playBtn);
            parent.Add(_container);
        }

        private void ChangeStateOfSong(MouseUpEvent evt)
        {
            if (!_isPlaying)
            {
                InspectorClip.InspectorClip.PlayClip();
                ChangePlayIconTo(BUTTON_STATES.PAUSE);
                _isPlaying = true;
            }
            else
            {
                InspectorClip.InspectorClip.StopAllClips();
                ChangePlayIconTo(BUTTON_STATES.PLAY);
                _isPlaying = false;
            }
        }

        public void ChangeStateOfSong()
        {
            if (!_isPlaying)
            {
                InspectorClip.InspectorClip.PlayClip();
                ChangePlayIconTo(BUTTON_STATES.PAUSE);
                _isPlaying = true;
            }
            else
            {
                InspectorClip.InspectorClip.StopAllClips();
                ChangePlayIconTo(BUTTON_STATES.PLAY);
                _isPlaying = false;
            }
        }

        private void ChangePlayIconTo(BUTTON_STATES buttonState)
        {
            if (buttonState == BUTTON_STATES.PLAY)
            {
                _container.Remove(_pauseBtn);
                _container.Add(_playBtn);
            }
            if (buttonState == BUTTON_STATES.PAUSE)
            {
                _container.Remove(_playBtn);
                _container.Add(_pauseBtn);

            }
        }

    }

    public enum BUTTON_STATES
    {
        PLAY, PAUSE
    }
}