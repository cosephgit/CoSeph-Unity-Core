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
        public string _fallback;        // default / English
        public LocalizedString _loc;

        public string Resolve()
        {
            if (_loc == null || _loc.IsEmpty)
                return _fallback;

            return _loc.GetLocalizedString();
        }

    }
    #endregion

    public class CSInterface : MonoBehaviour
    {
    }
}
