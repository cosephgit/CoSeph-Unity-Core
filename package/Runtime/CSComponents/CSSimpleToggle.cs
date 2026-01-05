using System.Collections;
using UnityEngine;

// CSSimpleToggle
// simple script for an object which needs to goggle on and off once triggered
// created 26/2/25
// modified 26/2/25

namespace CoSeph.Core
{
    public class CSSimpleToggle : MonoBehaviour
    {
        [SerializeField] private GameObject[] _toggleObjects;
        [SerializeField] private float _offTime = 1f;
        [SerializeField] private float _onTime = 1f;
        [SerializeField] private bool _startActive = false;
        private Coroutine _toggleRoutine = null;

        private void Start()
        {
            if (_startActive)
                StartToggle();
        }

        public void StartToggle()
        {
            if (_toggleRoutine != null)
                return;

            if (isActiveAndEnabled)
                _toggleRoutine = StartCoroutine(Toggler());
        }

        public void StopToggle()
        {
            if (_toggleRoutine != null)
            {
                StopCoroutine(_toggleRoutine);
                _toggleRoutine = null;
            }
        }

        private IEnumerator Toggler()
        {
            while (true)
            {
                for (int i = 0; i < _toggleObjects.Length; i++)
                    _toggleObjects[i].SetActive(true);

                yield return new WaitForSeconds(_onTime);

                for (int i = 0; i < _toggleObjects.Length; i++)
                    _toggleObjects[i].SetActive(false);

                yield return new WaitForSeconds(_offTime);
            }
        }
    }
}