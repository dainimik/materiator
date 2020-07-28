﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public static class AtlasFactory
    {
        public static void CreateAtlas(KeyValuePair<MaterialData, List<MateriaSetter>> group, Material material, string path, MateriaAtlas existingAtlas = null)
        {
            var compatibleMateriaSettersCount = 0;
            var compatibleMateriaSetters = new List<MateriaSetter>();
            var prefabs = new List<GameObject>();
            var prefabPaths = new List<string>();

            foreach (var ms in group.Value)
            {
                if (CheckMateriaSetterCompatibility(ms))
                {
                    compatibleMateriaSetters.Add(ms);
                    compatibleMateriaSettersCount++;
                }
            }

            var rects = CalculateRects(compatibleMateriaSettersCount, SystemData.Settings.GridSize);
            var rectIndex = 0;
            var atlasGridSize = CalculateAtlasSize(compatibleMateriaSettersCount, SystemData.Settings.GridSize);

            var includeAllPrefabs = false;

            var dir = AssetUtils.GetDirectoryName(path);
            var atlasName = AssetUtils.GetFileName(path);
            MateriaAtlas atlas = null;

            if (existingAtlas == null || existingAtlas.GridSize.x < atlasGridSize.x || existingAtlas.GridSize.y < atlasGridSize.y)
            {
                atlas = CreateMateriaAtlasAsset(dir, atlasName, material, atlasGridSize);
                includeAllPrefabs = true;
            }
            else
            {
                atlas = existingAtlas;
            }

            foreach (var ms in compatibleMateriaSetters)
            {
                if (ms.MateriaSetterData.MateriaAtlas != null && !includeAllPrefabs)
                {
                    continue;
                }

                var root = ms.transform.root;
                var prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(root));
                if (!prefabs.Contains(prefabGO))
                {
                    prefabs.Add(prefabGO);
                    prefabPaths.Add(AssetDatabase.GetAssetPath(root));
                }
                compatibleMateriaSettersCount++;
            }

            var nullSlotIterator = 0;
            var nullSlotIndices = new List<int>();
            foreach (var kvp in atlas.AtlasEntries)
            {
                if (kvp.Value.MateriaSetterData == null)
                {
                    nullSlotIndices.Add(kvp.Key);
                }
            }

            var processedPrefabs = new HashSet<GameObject>();
            var skipSavingPrefab = false;

            try
            {
                AssetDatabase.StartAssetEditing();

                for (var i = 0; i < prefabs.Count; i++)
                {
                    var prefab = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefabs[i]));
                    var ms = prefab.GetComponentsInChildren<MateriaSetter>();

                    if (!processedPrefabs.Contains(prefab))
                    {
                        for (int j = 0; j < ms.Length; j++)
                        {
                            //var nearestPrefabInstanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(ms[j]);
                            //if (processedPrefabs.Contains(nearestPrefabInstanceRoot))
                            //continue;

                            //processedPrefabs.Add(nearestPrefabInstanceRoot);

                            if (ms[j].MateriaSetterData.MateriaAtlas != null && !includeAllPrefabs)
                            {
                                continue;
                            }

                            var data = ms[j].MateriaSetterData;

                            Mesh atlasedMesh;

                            if (data.AtlasedMesh != null)
                            {
                                atlasedMesh = data.AtlasedMesh;
                            }
                            else
                            {
                                atlasedMesh = Utils.CopyMesh(ms[j].MateriaSetterData.NativeMesh);
                            }


                            var remappedUVs = atlasedMesh.uv;

                            for (var k = 0; k < remappedUVs.Length; k++)
                            {
                                var uv = remappedUVs[k];

                                uv.x = rects[rectIndex].x + (uv.x * rects[rectIndex].width);
                                uv.y = rects[rectIndex].y + (uv.y * rects[rectIndex].height);

                                remappedUVs[k] = uv;
                            }

                            atlasedMesh.uv = remappedUVs;

                            var prefabMS = prefabs[i].GetComponentsInChildren<MateriaSetter>().Where(setter => setter.MateriaSetterData == data).FirstOrDefault();
                            if (nullSlotIndices.Count > 0)
                            {
                                atlas.AtlasEntries[nullSlotIndices[nullSlotIterator]].MateriaSetter = prefabMS;
                                atlas.AtlasEntries[nullSlotIndices[nullSlotIterator]].MateriaSetterData = data;

                                nullSlotIterator++;
                            }
                            else
                            {
                                if (!atlas.AtlasEntries.ContainsKey(i))
                                {
                                    atlas.AtlasEntries.Add(i, new MateriaAtlasEntry(prefabMS, data));
                                }
                            }

                            atlas.MaterialData = group.Key;
                            atlas.GridSize = atlasGridSize;

                            var baseCol = data.Textures.Color.GetPixels32();
                            var metallic = data.Textures.MetallicSmoothness.GetPixels32();
                            var emission = data.Textures.Emission.GetPixels32();

                            var rectInt = Utils.GetRectIntFromRect(atlasGridSize, rects[rectIndex]);

                            atlas.Textures.Color.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, baseCol);
                            atlas.Textures.MetallicSmoothness.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, metallic);
                            atlas.Textures.Emission.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, emission);


                            if (data.AtlasedMesh != null)
                            {
                                AssetDatabase.RemoveObjectFromAsset(data.AtlasedMesh);

                            }

                            AssetDatabase.AddObjectToAsset(atlasedMesh, data);

                            data.MateriaAtlas = atlas;
                            data.AtlasedMesh = atlasedMesh;
                            data.AtlasedUVRect = rects[rectIndex];
                            data.AtlasedGridSize = atlasGridSize;

                            ms[j].LoadAtlas(atlas);

                            rectIndex++;
                        }

                        processedPrefabs.Add(prefab);
                    }
                    else
                        skipSavingPrefab = true;

                    if (!skipSavingPrefab)
                    {
                        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPaths[i]); // Saves changes madate in unloaded scene to prefab
                        PrefabUtility.UnloadPrefabContents(prefab);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Materiator] Atlas creation was interrupted: " + ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            atlas.Textures.Apply();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            foreach (var item in prefabs)
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(item));
        }

        public static bool CheckMateriaSetterCompatibility(MateriaSetter ms)
        {
            if (ms.IsDirty == false
                && ms.MateriaSetterData != null
                && ms.MateriaSetterData.Material != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static MateriaAtlas CreateMateriaAtlasAsset(string directory, string name, Material material, Vector2Int size)
        {
            var atlas = AssetUtils.CreateScriptableObjectAsset<MateriaAtlas>(directory, name);
            atlas.Textures.CreateTextures(size.x, size.y);
            atlas.Textures.SetNames(name);
            atlas.Material = UnityEngine.Object.Instantiate(material);
            atlas.Material.name = name;

            AssetDatabase.AddObjectToAsset(atlas.Material, atlas);
            atlas.Textures.AddTexturesToAsset(atlas);

            AssetDatabase.SaveAssets();

            atlas.Textures.ImportTextureAssets();
            atlas.AtlasEntries = new SerializableDictionary<int, MateriaAtlasEntry>();

            return atlas;
        }

        #region These two functions are almost identical
        public static Rect[] CalculateRects(int number, Vector2Int gridSize) //16, 4x4
        {
            Rect[] rects = new Rect[number]; // 16
            var size = CalculateAtlasSize(number, gridSize);

            var sizeMultiplierX = gridSize.x / (float)size.x; // 0.25
            var sizeMultiplierY = gridSize.y / (float)size.y;

            for (int i = 0, y = 0; y < size.y / gridSize.y; y++) // 4
            {
                for (int x = 0; x < size.x / gridSize.x; x++, i++) // 4
                {
                    if (i >= number) break; // 16
                    rects[i] = new Rect
                    (
                        x * sizeMultiplierX, //if 10 then 2.5
                        y * sizeMultiplierY,
                        sizeMultiplierX,// 0.25
                        sizeMultiplierY
                    );
                }
            }

            return rects;
        }

        /*public static Rect[] CalculateRects(Vector2Int size, Rect offset) // 4, 0
        {
            var rects = new Rect[size.x * size.y]; // 16
            var rectSize = new Vector2();

            rectSize.x = 1 / (float)size.x * offset.width; // 0.25
            rectSize.y = 1 / (float)size.y * offset.height;

            for (int i = 0, y = 0; y < size.y; y++) // 4
            {
                for (int x = 0; x < size.x; x++, i++) // 4
                {
                    if (i >= size.x * size.y) break; // 16

                    rects[i] = new Rect
                    (
                        offset.x + (x / (float)size.x * offset.width), // if 10 then 2.5
                        offset.y + (y / (float)size.y * offset.height),
                        rectSize.x,
                        rectSize.y
                    );

                    rects[i].xMin = rects[i].x;
                    rects[i].yMin = rects[i].y;
                    rects[i].xMax = rects[i].x + rectSize.x;
                    rects[i].yMax = rects[i].y + rectSize.y;
                }
            }

            return rects;
        }*/
        #endregion

        public static Vector2Int CalculateAtlasSize(int numberOfMeshes, Vector2Int atlasEntrySize)
        {
            var ranges = new Vector2[] { new Vector2(0, 2), new Vector2(1, 5), new Vector2(4, 17), new Vector2(16, 65), new Vector2(64, 257), new Vector2(256, 1025), new Vector2(1024, 4097), new Vector2(4096, 16385), new Vector2(16384, 65537), new Vector2(65536, 262145), new Vector2(262144, 1048577), new Vector2(1048576, 4194305) };
            var size = atlasEntrySize; // Minimum atlas size
            for (int i = 0; i < ranges.Length; i++)
            {
                if (numberOfMeshes > ranges[i].x && numberOfMeshes < ranges[i].y)
                {
                    // This is temporary
                    size.x *= (int)Mathf.Pow(2, i);
                    size.y *= (int)Mathf.Pow(2, i);
                }
            }
            return size;
        }
    }
}