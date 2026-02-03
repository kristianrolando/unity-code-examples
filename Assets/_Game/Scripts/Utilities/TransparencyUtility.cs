using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Utilities
{
    /// <summary>
    /// Utility class for making GameObjects transparent and restoring their original materials.
    /// Supports both URP and Built-in Render Pipeline.
    /// </summary>
    public static class TransparencyUtility
    {
        private static readonly Dictionary<Renderer, Material[]> originalMaterials = new();

        /// <summary>
        /// Makes the GameObject and all its children transparent with the desired color and alpha.
        /// </summary>
        public static void SetTransparent(GameObject target, float transparency = 0.5f, Color? color = null)
        {
            if (target == null) return;

            Color targetColor = color ?? Color.white;

            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
            {
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.materials;
                }

                Material[] newMaterials = new Material[renderer.materials.Length];

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Material newMat = new Material(renderer.materials[i]);
                    Color newColor = color ?? newMat.color;
                    newColor.a = transparency;

                    ApplyTransparencySettings(newMat);
                    SetMaterialColor(newMat, newColor);

                    newMaterials[i] = newMat;
                }

                renderer.materials = newMaterials;
            }
        }

        /// <summary>
        /// Restores original materials for the GameObject and its children.
        /// </summary>
        public static void ResetMaterial(GameObject target)
        {
            if (target == null) return;

            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
            {
                if (originalMaterials.TryGetValue(renderer, out var originalMats))
                {
                    renderer.materials = originalMats;
                    originalMaterials.Remove(renderer);
                }
            }
        }

        /// <summary>
        /// Apply transparency settings based on current render pipeline.
        /// </summary>
        private static void ApplyTransparencySettings(Material mat)
        {
            if (IsURP)
            {
                mat.SetFloat("_Surface", 1f); // Transparent
                mat.SetFloat("_Blend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)RenderQueue.Transparent;
            }
            else // Built-in RP
            {
                if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3); // 3 = Transparent
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)RenderQueue.Transparent;
            }
        }

        /// <summary>
        /// Sets the appropriate color property for the material.
        /// </summary>
        private static void SetMaterialColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color); // URP Lit
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color); // Built-in Standard
            }
        }

        /// <summary>
        /// Detects if the current pipeline is URP.
        /// </summary>
        private static bool IsURP => GraphicsSettings.currentRenderPipeline != null;
    }
}
