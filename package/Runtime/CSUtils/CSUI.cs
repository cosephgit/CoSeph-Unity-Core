using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoSeph.Core
{
    /// <summary>
    /// UI utility methods, such as for interacting with Unity UI and
    /// generating TextMeshPro sprite-based glyph strings.
    /// Intended for cases where raster/custom fonts are represented via TMP sprite assets
    /// (e.g. file paths, symbols, or non-standard glyph sets).
    /// </summary>
    public static class CSUI
    {
        /// <summary>
        /// Finds all UI elements under a given screen-space position using the current EventSystem
        /// Returns an empty list if there is no current EventSystem
        /// </summary>
        /// <param name="pos">The screen space position to check</param>
        /// <returns></returns>
        public static List<RaycastResult> GetUIObjects(Vector2 pos)
        {
            if (EventSystem.current)
            {
                // Create generic pointer event data for UI raycasting
                PointerEventData pointerData = new PointerEventData(EventSystem.current) { pointerId = -1, };

                pointerData.position = pos;

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                return results;
            }

            Debug.LogWarning("CSUI.GetUIObjects requires an EventSystem.current.");
            return new List<RaycastResult>();
        }

        /// <summary>
        /// Converts a plain string into a TextMeshPro sprite-tag string,
        /// mapping each character to a corresponding sprite index.
        /// Intended for use with TMP sprite assets representing custom glyphs/raster fonts
        /// (e.g. file paths, symbols, or non-standard characters).
        /// Assumes the spriteAsset is set up with sprite indices matching character code values
        /// (e.g. ASCII-compatible layouts)
        /// EXAMPLE
        /// stringSprites = CSUI.StringToSprite(string, "assetname");
        /// </summary>
        /// <param name="str">Source string to convert</param>
        /// <param name="spriteAsset">font sprite asset path</param>
        public static string StringToSprites(string str, string spriteAsset)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(spriteAsset))
                return string.Empty;

            // each tag string will be at least: 18 (tags) + spriteasset.Length + 1 (minimum per char)
            System.Text.StringBuilder sb =
                new System.Text.StringBuilder(str.Length * (19 + spriteAsset.Length));

            for (int i = 0; i < str.Length; i++)
                sb.Append(SpriteAssetToSprite(spriteAsset, str[i]));

            return sb.ToString();
        }

        /// <summary>
        /// Converts a sprite asset and character into a TMP sprite tag.
        /// Assumes the character's numeric value maps directly to a sprite index.
        /// </summary>
        /// <param name="spriteAsset">path of the sprite asset</param>
        /// <param name="index">index of desired sprite in the sprite asset</param>
        public static string SpriteAssetToSprite(string spriteAsset, char index)
        {
            return "<sprite=\"" + spriteAsset + "\" index=" + index + ">";
        }
    }
}
