using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSAchievements
// Stores unlocked achievements and unlocks
// Has integrations for platform-specific achievement integration
// created 27/5/25
// modified 27/5/25

namespace CoSeph.Core
{
    public class SaveAchievement
    {
        public string _id;
        public float _value;
    }


    public class CSAchievementManager : MonoBehaviour
    {
        protected static CSAchievementManager _instance;
        private Dictionary<CSAchievement, float> _achievementsGot;
        private CSAchievement[] _achievementsCache;
        // for platform-specific achievement integration
        public delegate void AchievementSet(CSAchievement achievement, float value);
        public static event AchievementSet _achievementSet = delegate { };

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

        private void InitialiseInstance()
        {
            DontDestroyOnLoad(gameObject);
            _achievementsGot = new Dictionary<CSAchievement, float>();
            _achievementsCache = Resources.FindObjectsOfTypeAll<CSAchievement>();

            for (int i = 0; i < _achievementsCache.Length - 1; i++)
            {
                for (int j = 1; j < _achievementsCache.Length; j++)
                {
                    if (i != j)
                    {
                        if (_achievementsCache[i].unique == _achievementsCache[j].unique)
                            Debug.LogError("Found duplicate CSAchivement unique " + _achievementsCache[j].unique);
                    }
                }
            }
        }

        public static CSAchievementManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    return new GameObject("AchievementManager").AddComponent<CSAchievementManager>();
                }
                else
                {
                    return _instance;
                }
            }
        }

        public Dictionary<CSAchievement, float> AchievementsGot()
        {
            return new Dictionary<CSAchievement, float>(_achievementsGot);
        }

        public CSAchievement GetAchievementByName(string nameFind)
        {
            for (int i = 0; i < _achievementsCache.Length; i++)
            {
                if (_achievementsCache[i].unique == nameFind)
                    return _achievementsCache[i];
            }
            return null;
        }

        // add an amount to the progress on the achievement
        public void AchievementProgressAdd(CSAchievement achievement, float progress)
        {
            if (_achievementsGot.ContainsKey(achievement))
            {
                _achievementsGot[achievement] += progress;
            }
            else
            {
                _achievementsGot.Add(achievement, progress);
            }
            //Debug.Log("Achievement " + achievement.unique + " progress is " + achievementsGot[achievement]);

            // set the achievement to the platform
            _achievementSet(achievement, _achievementsGot[achievement]);
        }

        // set the achievement to the new value
        // if increaseOnly, it will only increase the achievement progress to this value not reduce it
        public void AchievementProgressSet(CSAchievement achievement, float progress, bool increaseOnly)
        {
            if (achievement == null)
            {
                Debug.Log("AchievementProgressSet called with null achievement");
                return;
            }
            if (_achievementsGot.ContainsKey(achievement))
            {
                if (increaseOnly)
                {
                    if (_achievementsGot[achievement] < progress)
                        _achievementsGot[achievement] = progress;
                }
                else
                    _achievementsGot[achievement] = progress;
            }
            else
            {
                _achievementsGot.Add(achievement, progress);
            }
            //Debug.Log("Achievement " + achievement.unique + " progress is " + achievementsGot[achievement]);

            // set the achievement to the platform
            _achievementSet(achievement, _achievementsGot[achievement]);
        }

        public void AchievementInitialise(List<SaveAchievement> savedAchievements)
        {
            for (int i = 0; i < savedAchievements.Count; i++)
            {
                CSAchievement achievement = GetAchievementByName(savedAchievements[i]._id);
                if (achievement)
                    AchievementProgressSet(achievement, savedAchievements[i]._value, true);
                else
                    Debug.LogError("<color=green>Achievement</color> can't find achievement with saved id " + savedAchievements[i]._id);
            }
        }

        public bool IsAchievementComplete(CSAchievement achievement)
        {
            if (_achievementsGot.ContainsKey(achievement))
            {
                return (_achievementsGot[achievement] >= achievement.max);
            }
            return false;
        }
    }
}