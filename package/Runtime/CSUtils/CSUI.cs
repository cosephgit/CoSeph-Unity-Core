using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoSeph.Core
{
    public class CSUI : MonoBehaviour
    {
        // find all UI objects under the provided screen space point
        public static List<RaycastResult> GetUIObjects(Vector2 pos)
        {
            // check if the object is the interactionmenu
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { pointerId = -1, };

            pointerData.position = pos;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            return results;
        }

        // this takes a text string and converts it to a string that will show the desired characters from a TextMeshPro sprite asset
        // because TMP doesn't play with custom raster fonts
        // SAMPLES
        // scoreStringSprite = GameManagerScript.instance.StringToSprite(scoreString, "uifontscore");
        // spriteasset is a reference to the asset - location and name
        // str is the source string which must be converted to sprites
        public static string StringToSprites(string str, string spriteAsset)
        {
            string spriteString = "";

            for (int i = 0; i < str.Length; i++)
            {
                spriteString += SpriteAssetToSprite(spriteAsset, str[i]);
            }

            return spriteString;
        }

        // spriteAsset is the sprite sheet to take an entry from, index is the reference on the sheet
        public static string SpriteAssetToSprite(string spriteAsset, char index)
        {
            return "<sprite=\"" + spriteAsset + "\" index=" + index + ">";
        }
        public static string SpriteAssetToSprite(string spriteAsset, int fontOverride = 0, int index = 0)
        {
            string result = "<sprite=\"" + spriteAsset + "\" index=" + index + ">";
            if (fontOverride > 0)
                result = StringAddSizeTags(result, fontOverride);
            return result;
        }
        public static string StringAddSizeTags(string original, int fontSize)
        {
            return "<size=" + fontSize + ">" + original + "</size>";
        }
        public static string StringAddVOffset(string original, string offset)
        {
            return "<voffset=" + offset + ">" + original + "</voffset>";
        }

        public static string ApplyGlyphAdjustments(string original, string fontoffset = "", string padding = "")
        {
            string glyph = original;

            if (fontoffset.Length > 0) glyph = StringAddVOffset(glyph, fontoffset);

            if (padding.Length > 0) glyph = padding + glyph + padding;

            return glyph;
        }

    }
}
