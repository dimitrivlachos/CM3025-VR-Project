using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MusicForge;

namespace WindowClasses
{
    public class WelcomeClass : WindowClass
    {

        private Image _logo_Img;
        private Label _welcome_Lbl;
        private Button _continue_Btn;
        private Label _sindriContact_Lbl;

        private SongCreatorClass _creatorClass;

        public WelcomeClass(string welcome, SongCreatorClass creatorClass)
        {
            VisualElements = new List<VisualElement>();
            Texture logo = AssetDatabase.LoadAssetAtPath<Texture>(AppConstants.RELATIVEASSETPATH + "/Editor/Images/01LogoBlanco.png");

            _creatorClass = creatorClass;
            _welcome_Lbl = new Label(welcome);
            _logo_Img = new Image();
            _logo_Img.image = logo;
            _logo_Img.scaleMode = ScaleMode.ScaleToFit;
            _continue_Btn = new Button { text = "Continue" };
            _sindriContact_Lbl = new Label("Support at: sindri.studios.info@gmail.com");


            _continue_Btn.RegisterCallback<MouseUpEvent>(moveToCreatorClass);

            _welcome_Lbl.AddToClassList("title");
            _logo_Img.AddToClassList("imgTitle");
            _sindriContact_Lbl.AddToClassList("info");
            _continue_Btn.AddToClassList("nextBtn");
            _logo_Img.AddToClassList("LogoImg");

            VisualElements.Add(_welcome_Lbl);

            if (_logo_Img != null)
                VisualElements.Add(_logo_Img);

            VisualElements.Add(_continue_Btn);
            VisualElements.Add(_sindriContact_Lbl);

        }

        void moveToCreatorClass(MouseUpEvent evt)
        {
            RemoveFromContainer();
            _creatorClass.AddToContainer(root);
        }

    }
}