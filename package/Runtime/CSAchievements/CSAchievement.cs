using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSAchievement
// An individual achievement with data to display
// created 27/5/25
// modified 03/01/26

namespace CoSeph.Core
{
    [CreateAssetMenu(fileName = "Achievement", menuName = "CS/SO/Achievement", order = 0)]
    public class CSAchievement : ScriptableObject
    {
        public string unique;
        public CSLocalizedText locName = new CSLocalizedText();
        public CSLocalizedText locDesc = new CSLocalizedText();
        public Sprite iconGot;
        public Sprite iconNotGot;
        public float max;
        public bool integer;
        public string getName { get => locName.Resolve(); }
        public string getDesc { get => locDesc.Resolve(); }
    }
}