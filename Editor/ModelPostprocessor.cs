﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class ModelPostprocessor : AssetPostprocessor
    {
        private ModelImporter _modelImporter;

        private void Init()
        {
            _modelImporter = assetImporter as ModelImporter;
        }

        void OnPostprocessModel(GameObject g)
        {
            Init();
        }

        void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] names, object[] values)
        {
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var value = values[i];

                switch (name)
                {
                    case "Materiator":
                        ProcessMateriatorTag(value.ToString(), go);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ProcessMateriatorTag(string value, GameObject go)
        {
            var mic = CreateFromJSON(value.Remove(0, 1));

            var ms = go.AddComponent<MateriaSetter>();
            ms.MateriaSetterSlots = new List<MateriaSetterSlot>();

            var i = 0;
            foreach (var data in mic.Data)
            {
                var name = data.M;
                var rect = GetRectFromFloatArray(data.R);

                var mesh = MeshUtils.GetSharedMesh(go);
                var tag = SystemData.MateriaTags.MateriaTags.Where(tag => tag.Name == name).FirstOrDefault();
                var slot = new MateriaSetterSlot(i, rect, name, tag != null ? tag : SystemData.Settings.DefaultTag);

                slot.MeshData = GetMeshData(rect, mesh);

                ms.MateriaSetterSlots.Add(slot);

                i++;
            }
        }

        private MeshData GetMeshData(Rect rect, Mesh mesh)
        {
            var meshData = new MeshData();

            var indices = new List<int>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();

            for (int i = 0; i < mesh.uv.Length; i++)
            {
                var uv = mesh.uv[i];
                var color = mesh.colors[i];

                if (rect.Contains(uv))
                {
                    indices.Add(i);
                    colors.Add(color);
                    uvs.Add(uv);
                }
            }

            meshData.Mesh = mesh;
            meshData.Indices = indices.ToArray();
            meshData.Colors = colors.ToArray();
            meshData.UVs = uvs.ToArray();

            return meshData;
        }

        public static MateriatorInfoCollection CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<MateriatorInfoCollection>(jsonString);
        }

        private Rect GetRectFromFloatArray(float[] array)
        {
            if (array.Length != 4)
                return new Rect();

            return new Rect(array[0], array[1], array[2], array[3]);
        }

        [System.Serializable]
        public struct MateriatorInfoCollection
        {
            [System.Serializable]
            public struct MateriatorInfo
            {
                public string M;
                public float[] R;
            }

            public MateriatorInfo[] Data;
        }
    }
}