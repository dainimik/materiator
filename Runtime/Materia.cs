﻿#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif
using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Materia", fileName = "Materia")]
    public class Materia : MateriatorScriptableObject
    {
        public MaterialData MaterialData;

        [SerializeReference]
        [ShaderProperty(typeof(ShaderProperty))]
        public List<ShaderProperty> Properties = new List<ShaderProperty>();

#if UNITY_EDITOR
        public Texture2D PreviewIcon;

        public void AddProperties(List<ShaderProperty> properties)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                ShaderProperty materiaProperty = null;

                if (property.GetType() == typeof(ColorShaderProperty))
                {
                    var colorProp = (ColorShaderProperty)property;
                    materiaProperty = new ColorShaderProperty(colorProp.Name, colorProp.PropertyName, colorProp.Value);
                }
                else if (property.GetType() == typeof(FloatShaderProperty))
                {
                    var floatProp = (FloatShaderProperty)property;
                    materiaProperty = new FloatShaderProperty
                    (
                        floatProp.Name,
                        floatProp.PropertyName,
                        new Vector4(floatProp.RValue, floatProp.GValue, floatProp.BValue, floatProp.AValue),
                        new string[] { floatProp.RName, floatProp.GName, floatProp.BName, floatProp.AName }
                    );
                }

                Properties.Add(materiaProperty);
            }
        }
#endif
    }
}