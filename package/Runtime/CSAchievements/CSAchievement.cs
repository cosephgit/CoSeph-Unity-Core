using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// Represents a single achievement definition.
    /// This is a data-only ScriptableObject intended to be immutable at runtime.
    /// Referenced by UniqueID for save/load and runtime lookup.
    /// </summary>

    [CreateAssetMenu(fileName = "Achievement", menuName = "CS/SO/Achievement", order = 0)]
    public class CSAchievement : ScriptableObject
    {
        // Serialized definition data (authored in editor, read-only at runtime)
        [SerializeField] private string uniqueID;
        [SerializeField] private string textName;
        [SerializeField] private string textDesc;
        [SerializeField] private Sprite iconGot;
        [SerializeField] private Sprite iconNotGot;
        [SerializeField] private float max;
        [SerializeField] private bool integer;

        // Accessors
        /// <summary>
        /// Unique, stable identifier used for persistence and lookup.
        /// Must not be changed after shipping.
        /// </summary>
        public string UniqueID { get => uniqueID; }
        public Sprite IconGot { get => iconGot; }
        public Sprite IconNotGot { get => iconNotGot; }
        public float Max { get => max; }
        /// <summary>
        /// Whether progress should be treated as whole numbers (eg. binary or count-based achievements).
        /// Interpretation is left to consuming systems.
        /// </summary>
        public bool IsInteger { get => integer; }
        // Virtual to allow localization or dynamic overrides
        public virtual string Name { get => textName; }
        public virtual string Desc { get => textDesc; }
    }
}