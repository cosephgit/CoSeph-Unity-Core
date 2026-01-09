namespace CoSeph.Core
{
    /// <summary>
    /// Abstract base class for all states used by a StateMachine.
    /// States encapsulate behaviour for a specific mode of operation
    /// and are not intended to be used directly.
    /// </summary>
    public abstract class CSBaseState
    {
        /// <summary>
        /// Owning state machine.
        /// </summary>
        protected CSStateMachine _stateMachine;
        public virtual string Name { get => "INVALID STATE"; }

        public CSBaseState(CSStateMachine stateMachine)
        {
            _stateMachine = stateMachine
                ?? throw new System.ArgumentNullException(nameof(stateMachine));
        }

        /// <summary>
        /// Called when the state becomes active.
        /// </summary>
        public virtual void Enter() { }
        /// <summary>
        /// Called every Update() while the state is active.
        /// Intended for non-physics gameplay logic.
        /// </summary>
        public virtual void UpdateLogic() { }
        /// <summary>
        /// Called every FixedUpdate() while the state is active.
        /// Intended for physics or other strictly timed gameplay logic.
        /// </summary>
        public virtual void FixedUpdateLogic() { }
        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public virtual void Exit() { }
    }
}