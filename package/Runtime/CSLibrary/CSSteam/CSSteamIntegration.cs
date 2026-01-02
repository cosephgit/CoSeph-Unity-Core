using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_ANDROID
// android-specific includes here
#else
// PC-specific includes here
using Steamworks;
#endif

// CSSteamIntegration
// this is attached to the CSSteamManager
// connects the CSAchievementManager to CSSteamManager

public class CSSteamIntegration : MonoBehaviour
{
    private Dictionary<CSAchievement, float> achievementBacklog;
    private Coroutine steamUpdate;

    private void Awake()
    {
        achievementBacklog = new();
        CSAchievementManager.achievementSet += SetAchievement;
    }

    private void SetAchievementNow(CSAchievement achievement, float value)
    {
#if !UNITY_ANDROID
        //Debug.Log("Setting Steam achievement: " + achievement.unique + " to " + value);

        if (achievement.integer && achievement.max == 1f)
        {
            // then this is an achievement
            if (!SteamUserStats.SetAchievement(achievement.unique))
                Debug.LogError("SetAchievement failed with achievement name " + achievement.unique);
        }
        else
        {
            // then this is a stat
            if (achievement.integer)
            {
                if (!SteamUserStats.SetStat(achievement.unique, (int)value))
                    Debug.LogError("SetStats failed with INT stat name " + achievement.unique + " value " + value);
            }
            else
            {
                if (!SteamUserStats.SetStat(achievement.unique, value))
                    Debug.LogError("SetStats failed with FLOAT stat name " + achievement.unique + " value " + value);
            }
            //if (value >= achievement.max)
            //SteamUserStats.SetAchievement(achievement.unique);
        }
#endif
    }

    // event hook to set the Steam achievements
    public void SetAchievement(CSAchievement achievement, float value)
    {
        if (CSSteamManager.Initialized && CSSteamManager.userStatsReceived)
        {
            SetAchievementNow(achievement, value);
        }
        else
        {
            // not intialised and loaded yet - store up all achievements to apply when it completes
            if (achievementBacklog.Keys.Contains(achievement))
            {
                if (achievementBacklog[achievement] < value)
                    achievementBacklog[achievement] = value;
            }
            else
                achievementBacklog.Add(achievement, value);
            //Debug.Log("Steam not ready for achievements, achievements backlog: " + achievementBacklog.Count);
        }

        // update Steam next frame after all achievements for this frame are processed
        if (steamUpdate == null)
            steamUpdate = StartCoroutine(SteamUpdateDelay());
    }

    private void SetAchievementBacklog()
    {
        foreach (CSAchievement achievementBack in achievementBacklog.Keys)
        {
            SetAchievementNow(achievementBack, achievementBacklog[achievementBack]);
        }
        achievementBacklog.Clear();
    }

    private void OnDestroy()
    {
        CSAchievementManager.achievementSet -= SetAchievement;
    }

    // delay updating the store stats for a frame in case lots get added at once
    private IEnumerator SteamUpdateDelay()
    {
        // wait until Steam ready for stats
        while (!(CSSteamManager.Initialized && CSSteamManager.userStatsReceived))
            yield return new WaitForEndOfFrame();

#if !UNITY_ANDROID
        SetAchievementBacklog();

        yield return new WaitForEndOfFrame();

        SteamUserStats.StoreStats();
        steamUpdate = null;
#endif
    }
}
