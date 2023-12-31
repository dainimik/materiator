﻿using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    public abstract class MateriatorEditorWindow : EditorWindow
    {
        protected VisualElement root;
        protected VisualTreeAsset tree;
        protected StyleSheet styleSheet;

        protected SerializedObject serializedObject;

        protected void InitializeEditorWindow<T>()
        {
            root = rootVisualElement;

            tree = Resources.Load<VisualTreeAsset>(typeof(T).Name);
            styleSheet = Resources.Load<StyleSheet>("Materiator");
            root.styleSheets.Add(styleSheet);
            tree.CloneTree(root);

            GetProperties();
            BindProperties();
            RegisterButtons();
        }

        protected virtual void GetProperties() { }

        protected virtual void BindProperties() { }

        protected virtual void RegisterButtons() { }
    }
}
