using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    public class MateriaSetterData : ScriptableObject
    {
        public List<MateriaSlot> MateriaSlots;
        public MateriaPreset MateriaPreset;
        public MaterialData MaterialData;
        public Material Material;
        public Textures Textures;

        public MateriaAtlas MateriaAtlas;
        public Mesh NativeMesh;
        public Mesh AtlasedMesh;
        public int AtlasedGridSize;
        public int NativeGridSize;
        public Rect AtlasedUVRect;
    }
}