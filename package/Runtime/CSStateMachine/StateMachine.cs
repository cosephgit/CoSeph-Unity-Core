using System.Collections.Generic;
using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// Abstract base class for a simple finite state machine (FSM).
    ///
    /// Intended usage:
    /// - Inherit from this class to define a concrete state machine.
    /// - Create and register all required states during Initialise().
    /// - Perform state transitions via EnterState<T>().
    ///
    /// Notes:
    /// - This class should never be used directly.
    /// - States are identified by their concrete type.
    /// - The initial state is entered during Awake().
    /// </summary>
    public abstract class CSStateMachine : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private bool debugStateGUI;
#endif
        private BaseState _currentState = null;
        private readonly Dictionary<System.Type, BaseState> states = new();
        /// <summary>
        /// The currently active state.
        /// Read-only access intended for queries and debugging.
        /// </summary>
        public BaseState CurrentState => _currentState;

        protected virtual void Awake()
        {
            Initialise();
        }

        /// <summary>
        /// Called during Awake().
        /// Override this method to create and register all required states.
        ///
        /// For each state:
        /// RegisterState(new T(this));
        /// 
        /// The initial state will be entered immediately after this method completes.
        /// </summary>
        protected virtual void Initialise()
        {
            ChangeState(GetInitialState());
        }

        /// <summary>
        /// Registers a state instance with the state machine.
        ///
        /// Constraints:
        /// - Only one state per concrete type may be registered.
        /// - States must be registered before they can be entered.
        /// </summary>
        /// <param name="state">The state instance to register.</param>
        protected void RegisterState(BaseState state)
        {
            if (state == null)
            {
                Debug.LogError("RegisterState called with null state", this);
                return;
            }
            var type = state.GetType();
            if (!states.TryAdd(type, state))
            {
                Debug.LogError($"RegisterState called with duplicate state {type}", this);
            }
        }

        protected void Deactivate()
        {
            ChangeState(GetDisabledState());
        }

        protected virtual void Update()
        {
            if (_currentState != null)
            {
                _currentState.UpdateLogic();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (_currentState != null)
            {
                _currentState.FixedUpdateLogic();
            }
        }

        /// <summary>
        /// Performs an internal state transition.
        /// Enforces exit/enter ordering and prevents redundant transitions.
        /// </summary>
        private void ChangeState(BaseState newState)
        {
            if (ReferenceEquals(_currentState, newState))
            {
                Debug.LogWarning($"StateMachine attempting to enter the current state {newState.GetType()}.");
                return;
            }
            if (_currentState != null)
                _currentState.Exit();
            _currentState = newState;
            if (_currentState != null)
                _currentState.Enter();
        }

        /// <summary>
        /// Requests a transition into a registered state of type T.
        ///
        /// This is the only supported way to change state from outside the state machine.
        /// Logs an error if the requested state has not been registered.
        /// </summary>
        /// <typeparam name="T">The concrete state type to enter.</typeparam>
        public void EnterState<T>() where T : BaseState
        {
            if (states.TryGetValue(typeof(T), out BaseState state))
            {
                ChangeState(state);
            }
            else
            {
                Debug.LogError($"State {typeof(T)} has not been registered.", this);
            }
        }

        /// <summary>
        /// Defines the initial state should be entered in Awake(). Defaults to null.
        /// Override to set an initial state.
        /// Ensure that the state's OnEnter method does not depend on anything else in Awake().
        /// </summary>
        protected virtual BaseState GetInitialState()
        {
            return null;
        }

        protected virtual BaseState GetDisabledState()
        {
            return null;
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (debugStateGUI)
            {
                string content = _currentState != null ? _currentState.Name : "(no current state)";
                GUILayout.Label($"<color='black'><size=40>{content}</size></color>");
            }
        }
    }
#endif
}