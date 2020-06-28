﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Materiator
{
    public class MateriaSetter : MonoBehaviour
    {
        public bool IsInitialized = false;

        public MateriaSetterData MateriaSetterData;
        public Mesh Mesh;
        public MeshFilter MeshFilter;
        public Renderer Renderer;
        public MeshRenderer MeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        public List<MateriaSlot> MateriaSlots = new List<MateriaSlot>();

        public SerializableDictionary<int, Rect> FilteredRects;
        public Rect[] Rects;

        public MateriaPreset MateriaPreset;
        public ShaderData ShaderData;
        public Material Material;

        public Textures Textures = new Textures();


        public void Initialize()
        {
            IsInitialized = false;

            GetMeshReferences();
            SetUpRenderer();
            InitializeTextures();
            AnalyzeMesh();
            GenerateMateriaSlots(true);

            IsInitialized = true;
        }

        public void Refresh()
        {
            GetMeshReferences();
            SetUpRenderer();
            InitializeTextures();
            AnalyzeMesh();
            GenerateMateriaSlots(false);
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

        private void AnalyzeMesh()
        {
            Rects = MeshAnalyzer.CalculateRects(Utils.Settings.GridSize);
            FilteredRects = MeshAnalyzer.FilterRects(Rects, Mesh.uv);
        }

        public void GenerateMateriaSlots(bool reset)
        {
            var materiaSlotsCount = 0;

            if (MateriaSlots != null)
            {
                materiaSlotsCount = MateriaSlots.Count;
            }

            if (materiaSlotsCount == 0 || FilteredRects.Count != materiaSlotsCount)
            {
                if (reset)
                {
                    MateriaSlots = new List<MateriaSlot>();
                }

                var keyArray = new int[MateriaSlots.Count];
                for (int i = 0; i < keyArray.Length; i++)
                {
                    keyArray[i] = MateriaSlots[i].ID;
                }

                foreach (var rect in FilteredRects)
                {
                    if (!keyArray.Contains(rect.Key))
                    {
                        MateriaSlots.Add(new MateriaSlot(rect.Key));
                    }
                }
            }
        }

        public void UpdateColorsOfAllTextures()
        {
            Textures.UpdateColors(FilteredRects, MateriaSlots);
        }

        public void LoadPreset(MateriaPreset preset)
        {
            if (preset != null)
            {
                for (int i = 0; i < MateriaSlots.Count; i++)
                {
                    for (int j = 0; j < preset.MateriaPresetItemList.Count; j++)
                    {
                        if (MateriaSlots[i].MateriaTag == preset.MateriaPresetItemList[j].Tag)
                        {
                            MateriaSlots[i].Materia = preset.MateriaPresetItemList[j].Materia;
                        }
                    }
                }
            }
        }
    }
}