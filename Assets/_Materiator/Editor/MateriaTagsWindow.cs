﻿using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class MateriaTagsWindow : EditorWindow
    {
        private Editor _editor = null;
        private MateriaTags _materiaTags;

        [MenuItem("Tools/Materiator/Tag Editor")]
        public static void OpenWindow()
        {
            GetWindow<MateriaTagsWindow>("Materia Tag Editor");
        }

        private void OnEnable()
        {
            if (_materiaTags == null) CreateDefaultTagsData();
            if (!_editor) Editor.CreateCachedEditor(_materiaTags, null, ref _editor);
        }

        private void OnGUI()
        {
            if (_editor) _editor.OnInspectorGUI();
        }

        private void CreateDefaultTagsData()
        {
            var editorScriptPath = AssetUtils.GetEditorScriptDirectory(this);
            AssetUtils.CheckDirAndCreate(editorScriptPath, "Resources");
            var path = editorScriptPath + "/Resources";
            _materiaTags = AssetUtils.CreateScriptableObjectAsset<MateriaTags>(path, "MateriaTags");

            _materiaTags.MateriaTagDictionary.Add(0, "-");
            _materiaTags.MateriaTagDictionary.Add(1, "Metal");
            _materiaTags.MateriaTagDictionary.Add(2, "Plastic");

            return;
        }
    }
}