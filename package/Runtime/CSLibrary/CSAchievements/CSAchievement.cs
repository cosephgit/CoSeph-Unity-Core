using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

// CSAchievement
// An individual achievement with data to display
// created 27/5/25
// modified 27/5/25

[CreateAssetMenu(fileName = "Achievement", menuName = "CS/SO/Achievement", order = 0)]
public class CSAchievement : ScriptableObject
{
    public string unique;
    [FormerlySerializedAs("nameName")] public LocalizedString locName;
    [FormerlySerializedAs("description")] public LocalizedString locDesc;
    public Sprite iconGot;
    public Sprite iconNotGot;
    public float max;
    public bool integer;
}
