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
        public string _unique;
        public CSLocalizedText _locName = new CSLocalizedText();
        public CSLocalizedText _locDesc = new CSLocalizedText();
        public Sprite _iconGot;
        public Sprite _iconNotGot;
        public float _max;
        public bool _integer;
        public string GetName { get => _locName.Resolve(); }
        public string GetDesc { get => _locDesc.Resolve(); }
    }
}