﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public static class MateriaDataFactory
    {
        public static void WriteAssetsToDisk(MateriaSetterEditor editor, string path, bool packAssets)
        {
            var materiaSetter = editor.MateriaSetter;

            if (!editor.IsDirty.boolValue) return;

            string name;
            string dir = "";

            if (path != null)
            {
                dir = AssetUtils.GetDirectoryName(path);
                name = AssetUtils.GetFileName(path);
            }
            else
            {
                name = materiaSetter.gameObject.name;
                path = dir + name + ".asset";
            }

            var IsTextureSizeDifferent = false;

            Material material = null;
            Textures outputTextures = null;

            bool isDataAssetExisting;
            var data = AssetUtils.CreateOrReplaceScriptableObjectAsset(materiaSetter.MateriaSetterData, path, out isDataAssetExisting);

            var remakeAtlas = false;

            if (isDataAssetExisting)
            {
                if (editor.EditMode.enumValueIndex == 0)
                {
                    // Figure this out because this is wrong and will be buggy
                    if ((materiaSetter.Textures.Size.x != data.Textures.Size.x) || (materiaSetter.Textures.Size.y != data.Textures.Size.y) || (materiaSetter.Textures.Texs.Count != data.Textures.Texs.Count))
                    {
                        IsTextureSizeDifferent = true;

                        if (data.MateriaAtlas)
                        {
                            // recalculate atlas textures
                            remakeAtlas = true;
                        }

                        foreach (var tex in data.Textures.Texs.ToArray())
                        {
                            if (packAssets)
                                AssetDatabase.RemoveObjectFromAsset(tex.Value);
                            else
                                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tex.Value));

                            data.Textures.Texs.Clear();
                            data.Textures.CreateTextures(data.MaterialData.ShaderData.Properties, materiaSetter.Textures.Size.x, materiaSetter.Textures.Size.y);
                        }

                        AssetDatabase.SaveAssets();
                    }
                }
                
                if (editor.EditMode.enumValueIndex == 0)
                {
                    outputTextures = data.Textures;
                    material = data.Material;

                    if (data.MaterialData != materiaSetter.MaterialData)
                    {
                        var mat = Material.Instantiate(materiaSetter.Material);
                        mat.name = material.name;

                        if (packAssets)
                        {
                            AssetDatabase.RemoveObjectFromAsset(material);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.AddObjectToAsset(mat, data);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mat));
                            material = mat;
                        }
                        else
                        {
                            AssetUtils.CheckDirAndCreate(dir, name);
                            var folderDir = dir + "/" + name + "/";
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(material));
                            AssetDatabase.CreateAsset(mat, folderDir + name + "_Material.mat");
                            material = (Material)AssetDatabase.LoadAssetAtPath(folderDir + name + "_Material.mat", typeof(Material));
                        }
                    }
                }
                else if (editor.EditMode.enumValueIndex == 1)
                {
                    outputTextures = data.MateriaAtlas.Textures;
                    material = data.MateriaAtlas.Material;
                    name = data.MateriaAtlas.name;
                }
            }
            else
            {
                outputTextures = new Textures();
                outputTextures.CreateTextures(materiaSetter.MaterialData.ShaderData.Properties, materiaSetter.Textures.Size.x, materiaSetter.Textures.Size.y);

                material = Material.Instantiate(materiaSetter.Material); // I'm instantiating here because Unity can't add an object to asset if it is already a part of an asset
            }



            if (editor.EditMode.enumValueIndex == 0)
            {
                outputTextures.CopyPixelColors(materiaSetter.Textures, materiaSetter.Textures.Size, SystemData.Settings.UVRect, outputTextures.Size, SystemData.Settings.UVRect);
                outputTextures.SetNames(name);

                if (data.MateriaAtlas != null)
                    data.MateriaAtlas.Textures.CopyPixelColors(materiaSetter.Textures, materiaSetter.Textures.Size, SystemData.Settings.UVRect, data.MateriaAtlas.Textures.Size, materiaSetter.MateriaSetterData.AtlasedUVRect);
            }
            else if (editor.EditMode.enumValueIndex == 1)
            {
                outputTextures.CopyPixelColors(materiaSetter.Textures, materiaSetter.Textures.Size, materiaSetter.UVRect, materiaSetter.Textures.Size, materiaSetter.UVRect);
                outputTextures.SetNames(name);

                data.Textures.CopyPixelColors(materiaSetter.Textures, materiaSetter.Textures.Size, materiaSetter.UVRect, data.NativeGridSize, SystemData.Settings.UVRect);
            }



            if (packAssets)
            {
                material.name = name + "_Material";

                if (!isDataAssetExisting)
                {
                    AssetDatabase.AddObjectToAsset(material, data);
                    outputTextures.AddTexturesToAsset(data);
                }
                else if (materiaSetter.EditMode == EditMode.Native && IsTextureSizeDifferent) // Figure this out because this is wrong and will be buggy
                {
                    outputTextures.AddTexturesToAsset(data);
                }
            }
            else
            {
                AssetUtils.CheckDirAndCreate(dir, name);
                var folderDir = dir + "/" + name + "/";

                if (!isDataAssetExisting)
                {
                    AssetDatabase.CreateAsset(material, folderDir + name + "_Material.mat");
                    material = (Material)AssetDatabase.LoadAssetAtPath(folderDir + name + "_Material.mat", typeof(Material));
                    outputTextures.WriteTexturesToDisk(folderDir);
                }
                else
                {
                    //AssetDatabase.CreateAsset(material, folderDir + name + "_Material.mat");
                    //material = (Material)AssetDatabase.LoadAssetAtPath(folderDir + name + "_Material.mat", typeof(Material));
                    outputTextures.WriteTexturesToDisk(folderDir);
                }
            }

            materiaSetter.Material = material;
            materiaSetter.Textures = outputTextures;

            if (editor.EditMode.enumValueIndex == 0)
            {
                data.Material = material;
                data.Textures = outputTextures;
                data.NativeMesh = materiaSetter.Mesh;
                data.NativeGridSize = materiaSetter.GridSize;

                if (data.Textures.ID == 0)
                {
                    data.Textures.ID = UnityEngine.Random.Range(-999999, 9999999);
                }
            }

            if (data.MateriaSlots != null)
            {
                data.MateriaSlots.Clear();
            }
            else
            {
                data.MateriaSlots = new List<MateriaSlot>();
            }

            data.MateriaSlots.AddRange(materiaSetter.MateriaSlots);

            data.MaterialData = (MaterialData)editor.DataSection.MaterialData.objectReferenceValue;
            data.MateriaPreset = (MateriaPreset)editor.PresetSection.MateriaPreset.objectReferenceValue;


            materiaSetter.SetTextures();
            materiaSetter.MateriaSetterData = data;

            editor.serializedObject.Update();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtils.MarkOpenPrefabSceneDirty();

            materiaSetter.UpdateRenderer(false);

            if (editor.EditMode.enumValueIndex == 0)
            {
                editor.DataSection.ReloadData(data);
            }
            else if (editor.EditMode.enumValueIndex == 1)
            {
                editor.AtlasSection.LoadAtlas(data.MateriaAtlas);
            }

            if (remakeAtlas)
            {
                var setters = new List<MateriaSetter>();
                foreach (var item in materiaSetter.MateriaSetterData.MateriaAtlas.AtlasItems.Values)
                    setters.Add(item.MateriaSetter);

                AtlasFactory.CreateAtlas(new KeyValuePair<MaterialData, List<MateriaSetter>>(materiaSetter.MateriaSetterData.MaterialData, setters), AssetDatabase.GetAssetPath(materiaSetter.MateriaSetterData.MateriaAtlas), materiaSetter.MateriaSetterData.MateriaAtlas, true);
            }
        }
    }
}