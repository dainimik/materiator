﻿using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaPreset))]
    public class MateriaPresetEditor : MateriatorEditor
    {
        private MateriaPreset _materiaPreset;

        private ReorderableList _tagList;

        private void OnEnable()
        {
            _materiaPreset = (MateriaPreset)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaPreset>();

            _tagList = new ReorderableList(serializedObject, serializedObject.FindProperty("MateriaPresetItemList"), true, true, true, true);
            SetUpTagListUI();

            IMGUIContainer materiaTagsReorderableList = new IMGUIContainer(() => ExecuteIMGUI());
            root.Add(materiaTagsReorderableList);

            return root;
        }

        private void ExecuteIMGUI()
        {
            serializedObject.Update();
            _tagList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetUpTagListUI()
        {
            _tagList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x + 25f, rect.y, 50f, 20f), new GUIContent("Tag", "Tag"), EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 200f, rect.y, 200f, 20f), new GUIContent("Materia", "Materia"), EditorStyles.boldLabel);
            };

            _tagList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _tagList.serializedProperty.GetArrayElementAtIndex(index);
                var materiaTag = element.FindPropertyRelative("Tag");
                var materia = element.FindPropertyRelative("Materia").objectReferenceValue as Materia;

                Rect r = new Rect(rect.x, rect.y, 150f, 22f);


                serializedObject.Update();
                int _materiaTagIndex = 0;
                _materiaTagIndex = EditorGUI.Popup(new Rect(r.x + 15, r.y, r.width, r.height), Utils.MateriaTags.MateriaTagsList.IndexOf(materiaTag.stringValue), Utils.MateriaTags.MateriaTagsArray, EditorStyles.popup);
                if (EditorGUI.EndChangeCheck())
                {
                    var newTag = Utils.MateriaTags.MateriaTagsList[_materiaTagIndex];
                    var canSetTag = true;
                    for (int i = 0; i < _materiaPreset.MateriaPresetItemList.Count; i++)
                    {
                        if (_materiaPreset.MateriaPresetItemList[i].Tag == newTag)
                        {
                            canSetTag = false;
                        }
                    }

                    if (canSetTag)
                    {
                        Undo.RegisterCompleteObjectUndo(_materiaPreset, "Set Preset Materia Tag");
                        _materiaPreset.MateriaPresetItemList[index].Tag = newTag;
                    }
                }

                /*materia = (Materia)EditorGUI.ObjectField(new Rect(r.x + 185, r.y, rect.width - 185f, r.height), materia, typeof(Materia), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(_materiaPreset, "Change Materia");
                    if (materia == null)
                        materia = Utils.Settings.DefaultMateria;

                    serializedObject.Update();
                }*/

                EditorGUI.PropertyField(new Rect(r.x + 185, r.y, rect.width - 185f, r.height), element.FindPropertyRelative("Materia"), GUIContent.none);
            };

            _tagList.onAddCallback = (ReorderableList List) =>
            {
                _materiaPreset.MateriaPresetItemList.Add(new MateriaPresetItem("-"));
            };
        }
    }
}