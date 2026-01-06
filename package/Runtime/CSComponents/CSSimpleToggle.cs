using System.Collections;
using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// Simple utility component that repeatedly toggles a set of GameObjects
    /// on and off at fixed intervals.
    ///
    /// Intended for lightweight visual or gameplay effects where state
    /// alternates predictably over time.
    ///
    /// Guarantees:
    /// - Only one toggle coroutine can run at a time.
    /// - All target objects are returned to an active state when toggling stops.
    /// - Safe to enable/disable at runtime.
    /// </summary>
    public class CSSimpleToggle : MonoBehaviour
    {
        /// <summary>
        /// Objects that will be toggled on and off together.
        /// Null entries are ignored safely.
        /// </summary>
        [SerializeField] private GameObject[] _toggleObjects;
        /// <summary>
        /// Duration (in seconds) objects remain inactive between toggles.
        /// </summary>
        [SerializeField] private float _offTime = 1f;
        /// <summary>
        /// Duration (in seconds) objects remain active between toggles.
        /// </summary>
        [SerializeField] private float _onTime = 1f;
        /// <summary>
        /// If true, toggling begins automatically on startup.
        /// Otherwise, objects start in the active state.
        /// </summary>
        [SerializeField] private bool _startToggling = false;
        // Active toggle coroutine, if any
        private Coroutine _toggleRoutine = null;
        public bool IsToggling { get => (_toggleRoutine != null); }

        private void Start()
        {
            // Establish initial visual state
            if (_startToggling)
                StartToggle();
            else
                SetActiveAll(true);
        }

        /// <summary>
        /// Starts the toggle loop if not already running.
        /// Safe to call multiple times.
        /// </summary>
        public void StartToggle()
        {
            if (IsToggling)
                return;

            if (isActiveAndEnabled)
                _toggleRoutine = StartCoroutine(Toggler());
        }

        /// <summary>
        /// Stops the toggle loop and restores all objects
        /// to the active state.
        /// </summary>
        public void StopToggle()
        {
            if (IsToggling)
            {
                SetActiveAll(true);
                StopCoroutine(_toggleRoutine);
                _toggleRoutine = null;
            }
        }

        /// <summary>
        /// Sets all toggle targets to the specified active state.
        /// Null references are ignored.
        /// </summary>
        private void SetActiveAll(bool active)
        {
            for (int i = 0; i < _toggleObjects.Length; i++)
            {
                if (_toggleObjects[i] != null)
                    _toggleObjects[i].SetActive(active);
            }
        }

        /// <summary>
        /// Core toggle loop. Alternates object states until explicitly stopped.
        /// Timing values are clamped to avoid zero-duration yields.
        /// </summary>
        private IEnumerator Toggler()
        {
            float onTime = Mathf.Max(0.01f, _onTime);
            float offTime = Mathf.Max(0.01f, _offTime);

            while (IsToggling)
            {
                SetActiveAll(true);

                yield return new WaitForSeconds(onTime);

                SetActiveAll(false);

                yield return new WaitForSeconds(offTime);
            }
        }

        private void OnDisable()
        {
            // Ensure objects are left in a valid state when disabled
            StopToggle();
        }
    }
}