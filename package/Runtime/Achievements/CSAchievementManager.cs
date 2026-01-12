using System.Collections.Generic;
using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// Serializable data container for persisting achievement progress.
    /// Contains no logic and references achievements by UniqueID only.
    /// </summary>
    public class AchievementSaveData
    {
        public string UniqueID;
        public float Value;
    }

    /// <summary>
    /// Central runtime authority for achievement progress.
    /// 
    /// Responsibilities:
    /// - Owns runtime progress state (by UniqueID)
    /// - Maps saved data to immutable achievement definitions
    /// - Emits progress change events for platform integrations
    /// - Caches all achievement data for quick access
    /// 
    /// This manager persists across scenes and must exist exactly once.
    /// </summary>

    public class CSAchievementManager : MonoBehaviour
    {
        // Singleton instance. Exactly one must exist at runtime.
        protected static CSAchievementManager _instance;
        // Lookup of achievement definitions by UniqueID.
        // Built once at startup to avoid repeated linear searches.
        private Dictionary<string, CSAchievement> _achievementsCache;
        /// <summary>
        /// Fired whenever achievement progress changes.
        /// Intended for platform-specific or external integrations.
        /// </summary>
        public delegate void AchievementSet(CSAchievement achievement, float value);
        public static event AchievementSet AchievementProgressChanged = delegate { };
        // actual achievement progress
        private Dictionary<string, float> _achievementsGot;

        // Enforces singleton instance and initializes persistent state.
        private void Awake()
        {
            if (_instance)
            {
                if (_instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                _instance = this;
                InitialiseInstance();
            }
        }

        /// <summary>
        /// Performs one-time initialization:
        /// - Marks the manager as persistent
        /// - Builds fast lookup tables for achievements
        /// - Validates UniqueID uniqueness
        /// </summary>
        private void InitialiseInstance()
        {
            DontDestroyOnLoad(gameObject);
            _achievementsGot = new Dictionary<string, float>();
            CSAchievement[] allAchievements = Resources.FindObjectsOfTypeAll<CSAchievement>();

            _achievementsCache = new Dictionary<string, CSAchievement>();

            for (int i = 0; i < allAchievements.Length; i++)
            {
                if (!_achievementsCache.TryAdd(allAchievements[i].UniqueID, allAchievements[i]))
                {
                    Debug.LogError($"Found duplicate CSAchivement uniqueID {allAchievements[i].UniqueID}", this);
                }
            }
        }

        public static CSAchievementManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("CSAchievementManager not found. Ensure one exists.");

                return _instance;
            }
        }

        /// <summary>
        /// Returns a snapshot of all achievement progress suitable for saving.
        /// Returned data is a copy and safe to modify by the caller.
        /// </summary>
        public List<AchievementSaveData> GetAllProgress()
        {
            var list = new List<AchievementSaveData>();

            foreach (var kvp in _achievementsGot)
            {
                list.Add(new AchievementSaveData
                {
                    UniqueID = kvp.Key,
                    Value = kvp.Value
                });
            }

            return list;
        }

        /// <summary>
        /// Resolves an achievement definition by UniqueID.
        /// Returns null if no matching definition exists.
        /// </summary>
        public CSAchievement GetAchievementByUnique(string uniqueGet)
        {
            if (_achievementsCache.TryGetValue(uniqueGet, out CSAchievement achievement))
            {
                return achievement;
            }

            Debug.LogError($"GetAchievementByUnique invalid uniqueGet {uniqueGet}", this);
            return null;
        }

        /// <summary>
        /// Adds progress to an achievement, clamped to its maximum.
        /// Emits a progress-changed event if successful.
        /// </summary>
        public void AchievementProgressAdd(CSAchievement achievement, float progress)
        {
            if (_achievementsGot.ContainsKey(achievement.UniqueID))
            {
                _achievementsGot[achievement.UniqueID] = Mathf.Clamp(_achievementsGot[achievement.UniqueID] + progress, 0f, achievement.Max);
            }
            else
            {
                _achievementsGot.Add(achievement.UniqueID, progress);
            }
            //Debug.Log("Achievement " + achievement.UniqueID + " progress is " + _achievementsGot[achievement.UniqueID]);

            // set the achievement to the platform
            AchievementProgressChanged(achievement, _achievementsGot[achievement.UniqueID]);
        }

        /// <summary>
        /// Sets achievement progress to a specific value.
        /// If increaseOnly is true, progress will never be reduced.
        /// Emits a progress-changed event if the value changes.
        /// </summary>
        public void AchievementProgressSet(CSAchievement achievement, float progress, bool increaseOnly)
        {
            if (achievement == null)
            {
                Debug.LogError("AchievementProgressSet called with null achievement", this);
                return;
            }
            float newValue = Mathf.Clamp(progress, 0f, achievement.Max);
            if (_achievementsGot.ContainsKey(achievement.UniqueID))
            {
                if (increaseOnly && _achievementsGot[achievement.UniqueID] >= newValue)
                    return;
                _achievementsGot[achievement.UniqueID] = newValue;
            }
            else
            {
                _achievementsGot.Add(achievement.UniqueID, newValue);
            }
            //Debug.Log("Achievement " + achievement.unique + " progress is " + achievementsGot[achievement]);

            // set the achievement to the platform
            AchievementProgressChanged(achievement, _achievementsGot[achievement.UniqueID]);
        }

        /// <summary>
        /// Applies previously saved achievement progress.
        /// Intended to be called once during load/bootstrap.
        /// </summary>
        public void AchievementInitialise(List<AchievementSaveData> savedAchievements)
        {
            for (int i = 0; i < savedAchievements.Count; i++)
            {
                CSAchievement achievement = GetAchievementByUnique(savedAchievements[i].UniqueID);
                if (achievement)
                    AchievementProgressSet(achievement, savedAchievements[i].Value, true);
            }
        }

        /// <summary>
        /// Returns true if the achievement has reached or exceeded its maximum value.
        /// </summary>
        public bool IsAchievementComplete(CSAchievement achievement)
        {
            if (_achievementsGot.ContainsKey(achievement.UniqueID))
            {
                return (_achievementsGot[achievement.UniqueID] >= achievement.Max);
            }
            return false;
        }
    }
}