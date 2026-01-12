using UnityEngine;

namespace CoSeph.Core
{
    public class CSSimpleCounter : MonoBehaviour
    {
        [SerializeField] private TextMesh _textLegacy;

        public virtual void SetCounter(int count)
        {
            if (_textLegacy)
            {
                _textLegacy.text = count.ToString();
            }
        }

        public virtual void ClearCounter()
        {
            if (_textLegacy)
            {
                _textLegacy.text = "";
            }
        }
    }
}
