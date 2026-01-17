using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// Represents a single achievement definition.
    /// This is a data-only ScriptableObject intended to be immutable at runtime.
    /// Referenced by UniqueID for save/load and runtime lookup.
    /// </summary>

    [CreateAssetMenu(fileName = "Achievement", menuName = "CS/Achievements/Achievement", order = 0)]
    public class CSAchievement : ScriptableObject
    {
        // Serialized definition data (authored in editor, read-only at runtime)
        [SerializeField, Tooltip("Unique, stable identifier for lookup")] private string uniqueID;
        [SerializeField] private string textName;
        [SerializeField] private string textDesc;
        [SerializeField] private Sprite iconGot;
        [SerializeField] private Sprite iconNotGot;
        [SerializeField] private float max;
        [SerializeField, Tooltip("If progress is a discrete integer or not")] private bool integer;

        // Accessors
        /// <summary>
        /// Unique, stable identifier used for persistence and lookup.
        /// Must not be changed after shipping.
        /// </summary>
        public string UniqueID => uniqueID;
        public Sprite IconGot => iconGot; 
        public Sprite IconNotGot => iconNotGot; 
        public float Max => max;
        public bool IsInteger => integer;
        // Virtual to allow localization or dynamic overrides
        public virtual string Name { get => textName; }
        public virtual string Desc { get => textDesc; }
    }
}