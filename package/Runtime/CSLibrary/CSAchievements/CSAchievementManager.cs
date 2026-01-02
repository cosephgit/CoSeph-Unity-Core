using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSAchievements
// Stores unlocked achievements and unlocks
// Has integrations for platform-specific achievement integration
// created 27/5/25
// modified 27/5/25

public class CSAchievementManager : MonoBehaviour
{
    protected static CSAchievementManager instanceTrue;
    private Dictionary<CSAchievement, float> achievementsGot;
    private CSAchievement[] achievementsCache;
    // for platform-specific achievement integration
    public delegate void AchievementSet(CSAchievement achievement, float value);
    public static event AchievementSet achievementSet = delegate { };

    private void Awake()
    {
        if (instanceTrue)
        {
            if (instanceTrue != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            instanceTrue = this;
            DontDestroyOnLoad(gameObject);
            achievementsGot = new Dictionary<CSAchievement, float>();
            achievementsCache = Resources.FindObjectsOfTypeAll<CSAchievement>();

            for (int i = 0; i < achievementsCache.Length - 1; i++)
            {
                for (int j = 1; j < achievementsCache.Length; j++)
                {
                    if (i != j)
                    {
                        if (achievementsCache[i].unique == achievementsCache[j].unique)
                            Debug.LogError("Found duplicate CSAchivement unique " + achievementsCache[j].unique);
                    }
                }
            }
        }
    }

    public static CSAchievementManager instance
    {
        get
        {
            if (instanceTrue == null)
            {
                return new GameObject("AchievementManager").AddComponent<CSAchievementManager>();
            }
            else
            {
                return instanceTrue;
            }
        }
    }

    public Dictionary<CSAchievement, float> AchievementsGot()
    {
        return new Dictionary<CSAchievement, float>(achievementsGot);
    }

    public CSAchievement GetAchievementByName(string nameFind)
    {
        for (int i = 0; i < achievementsCache.Length; i++)
        {
            if (achievementsCache[i].unique == nameFind)
                return achievementsCache[i];
        }
        return null;
    }

    // add an amount to the progress on the achievement
    public void AchievementProgressAdd(CSAchievement achievement, float progress)
    {
        if (achievementsGot.ContainsKey(achievement))
        {
            achievementsGot[achievement] += progress;
        }
        else
        {
            achievementsGot.Add(achievement, progress);
        }
        //Debug.Log("Achievement " + achievement.unique + " progress is " + achievementsGot[achievement]);

        // set the achievement to the platform
        achievementSet(achievement, achievementsGot[achievement]);
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
        if (achievementsGot.ContainsKey(achievement))
        {
            if (increaseOnly)
            {
                if (achievementsGot[achievement] < progress)
                    achievementsGot[achievement] = progress;
            }
            else
                achievementsGot[achievement] = progress;
        }
        else
        {
            achievementsGot.Add(achievement, progress);
        }
        //Debug.Log("Achievement " + achievement.unique + " progress is " + achievementsGot[achievement]);

        // set the achievement to the platform
        achievementSet(achievement, achievementsGot[achievement]);
    }

    public void AchievementInitialise(List<SaveAchievement> savedAchievements)
    {
        for (int i = 0; i < savedAchievements.Count; i++)
        {
            CSAchievement achievement = GetAchievementByName(savedAchievements[i].id);
            if (achievement)
                AchievementProgressSet(achievement, savedAchievements[i].value, true);
            else
                Debug.LogError("<color=green>Achievement</color> can't find achievement with saved id "+ savedAchievements[i].id);
        }
    }

    public bool IsAchievementComplete(CSAchievement achievement)
    {
        if (achievementsGot.ContainsKey(achievement))
        {
            return (achievementsGot[achievement] >= achievement.max);
        }
        return false;
    }
}
