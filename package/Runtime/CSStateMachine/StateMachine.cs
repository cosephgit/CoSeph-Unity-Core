using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// StateMachine
// basic FSM class
// version 0.1
// last update: 19/10/23

namespace CoSeph.Core
{
    public class StateMachine : MonoBehaviour
    {
        protected BaseState _currentState;

        protected virtual void Start()
        {
            Initialise();
        }

        public void Initialise()
        {
            if (_currentState != null)
                _currentState.Exit();
            _currentState = GetInitialState();
            if (_currentState != null)
                _currentState.Enter();
        }

        public void Disable()
        {
            if (_currentState != null)
                _currentState.Exit();
            _currentState = GetDisabledState();
            if (_currentState != null)
                _currentState.Enter();
        }

        protected virtual void Update()
        {
            if (_currentState != null)
            {
                _currentState.UpdateLogic();
            }
        }

        public void ChangeState(BaseState newState)
        {
            _currentState.Exit();

            _currentState = newState;
            _currentState.Enter();
        }

        protected virtual BaseState GetInitialState()
        {
            return null;
        }

        protected virtual BaseState GetDisabledState()
        {
            return null;
        }

        // kept for reference
        private void OnGUI()
        {
            if (Application.isEditor)
            {
                //string content = currentState != null ? currentState.name : "(no current state)";
                //GUILayout.Label($"<color='black'><size=40>{content}</size></color>");
            }
        }
    }
}