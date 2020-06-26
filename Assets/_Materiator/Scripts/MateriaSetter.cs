﻿using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    public class MateriaSetter : MonoBehaviour
    {
        public bool IsInitialized = false;

        public Mesh Mesh;
        public MeshFilter MeshFilter;
        public Renderer Renderer;
        public MeshRenderer MeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        public SerializableIntMateriaDictionary Materia;
        public int[] MateriaIndices;
        public Dictionary<int, int> MateriaCategory;
        public MateriaPreset MateriaPreset;

        public SerializableDictionary<int, Rect> FilteredRects;
        public Rect[] Rects;

        public ShaderData ShaderData;
        public Material Material;

        public Textures Textures = new Textures();


        public void Initialize()
        {
            IsInitialized = false;

            GetMeshReferences();
            SetUpRenderer();
            InitializeTextures();

            IsInitialized = true;
        }

        private void GetMeshReferences()
        {
            Renderer = GetComponent<Renderer>();
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();
            SkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

            if (MeshFilter == null)
            {
                if (SkinnedMeshRenderer != null)
                {
                    Mesh = SkinnedMeshRenderer.sharedMesh;
                }
            }
            else
            {
                Mesh = MeshFilter.sharedMesh;
            }
        }

        private void SetUpRenderer()
        {
            if (ShaderData == null)
                ShaderData = Utils.Settings.DefaultShaderData;

            if (Renderer.sharedMaterial == null)
            {
                Material = Utils.CreateMaterial(ShaderData.Shader, gameObject.name);
                UpdateRenderer(false);
            }
            else
            {
                Material = Renderer.sharedMaterial;
            }
        }

        public void UpdateRenderer(bool updateMesh = true, bool updateMaterial = true)
        {
            if (updateMesh)
            {
                if (MeshFilter != null)
                    MeshFilter.sharedMesh = Mesh;

                if (SkinnedMeshRenderer != null)
                    SkinnedMeshRenderer.sharedMesh = Mesh;

                //_isUVPresent = GetUVCoordinates(out _colorUVCoord);
            }

            if (updateMaterial)
                Renderer.sharedMaterial = Material;
        }

        private void InitializeTextures()
        {
            if (Textures.Color == null || Textures.MetallicSmoothness == null || Textures.Emission == null)
            {
                Textures.CreateTextures(Utils.Settings.GridSize, Utils.Settings.GridSize);
            }

            SetTextures();
        }

        public void SetTextures()
        {
            if (Material == null) return;

            Textures.SetTextures(Material, ShaderData);
        }

        public void UpdateTexturePixelColors()
        {
            var tex = Textures.Color;

            foreach (var rect in FilteredRects)
            {
                int minX = (int)(Utils.Settings.GridSize * rect.Value.x);
                int minY = (int)(Utils.Settings.GridSize * rect.Value.y);
                int maxX = (int)(Utils.Settings.GridSize * rect.Value.width);
                int maxY = (int)(Utils.Settings.GridSize * rect.Value.height);

                var colors = new Color32[maxX * maxY];

                Materia materia;

                if (Materia.TryGetValue(rect.Key, out materia))
                {
                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = materia.BaseColor;
                    }

                }
                tex.SetPixels32(minX, minY, maxX, maxY, colors);
            }

            tex.Apply();
        }
    }
}