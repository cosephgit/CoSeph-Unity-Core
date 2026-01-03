using System;
using UnityEngine;
using UnityEngine.Localization;


// Framework with plugin integration
// CSInterface
// An individual achievement with data to display
// created 27/5/25
// modified 03/01/26

namespace CoSeph.Core
{
    #region Localization classes
    [Serializable]
    public class CSLocalizedText
    {
        [TextArea]
        public string fallback;        // default / English
        public LocalizedString loc;

        public string Resolve()
        {
            if (loc == null || loc.IsEmpty)
                return fallback;

            return loc.GetLocalizedString();
        }

    }
    #endregion

    public class CSInterface : MonoBehaviour
    {
    }
}
