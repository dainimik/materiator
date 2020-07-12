﻿using UnityEngine;

namespace Materiator
{
    public class MateriaAtlas : ScriptableObject
    {
        public MaterialData MaterialData;
        public Material Material;
        public Textures Textures = new Textures();
        public SerializableDictionary<MateriaSetter, MateriaSetterData> AtlasEntries; // Materia Setter prefab instance
        public int GridSize;
    }
}