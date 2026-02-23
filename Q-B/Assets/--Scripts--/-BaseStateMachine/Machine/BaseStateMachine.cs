using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// State Machine base class. Uses generic enum system for the base class, context, and state classes for a modular but
/// customizable state machine.
/// </summary>
// EXAMPLE INHERITED CLASS LOGIC VISUALIZATION
//
//        > FooStateMachine (inherits from BaseStateMachine)
//            |
//            V
//            > FooStateMachineContext (inherits from BaseStateMachineContext)
//            |
//            V
//            > FooStateMachineState (inherits from BaseState)
//
//        (See BaseState.cs for more information on how to extend the BaseState class)
// 
//        > FooStateA (inherits from FooStateMachineState) \
//        > FooStateB (inherits from FooStateMachineState) - > See BaseState.cs
//        > FooStateC (inherits from FooStateMachineState) /
public abstract class BaseStateMachine<EState> : MonoBehaviour where EState : Enum
{
    #region Base Variables, Properties, Fields
    
    // TODO:
    //  In derived classes, declare an enum and values to pass to the generic EState type parameter.
    //  Example:
    //  public enum EFooState
    //  {
    //      StateA,
    //      StateB,
    //      StateC
    //  }
    
    // TODO:
    //  In derived classes, declare and serialize a "context" field in the corresponding type of the context class defined
    //  in that derived class. Acts as a bridge between the state machine and its states.
    //  Example:
    //  [Serialize Field] private BaseStateMachineContext context;
    
    [Header("Base State Machine Settings")]

    [Tooltip("If true, state machine will log state changes and any additional behaviour desired in code")]
    [SerializeField] private bool debugMode; 
    
    /// <summary>
    /// Flag for debug mode. If true, logs state changes and updates to the console.
    /// </summary>
    protected bool DebugMode => debugMode;
    
    /// <summary>
    /// Flag indicating if the state machine has started.
    /// </summary>
    public bool Started { get; set; }
    
    /// <summary>
    /// Flag indicating if the state machine is currently changing state.
    /// Changed during the ChangeState method to prevent updates during state transitions.
    /// </summary>
    protected bool IsChangingState { get; set; } 
    
    /// <summary>
    /// Container for initialized states as key-value pairs,
    /// where key is the state enum and value is the corresponding state instance.
    /// </summary>
    protected Dictionary<EState, BaseState<EState>> States { get; set; }
    
    /// <summary>
    /// The current state of the state machine from those defined in the States dictionary.
    /// Changed using ChangeState method.
    /// </summary>
    public BaseState<EState> CurrentState { get; private set; } 
    
    /// <summary>
    /// The previous state of the state machine from those defined in the States dictionary.
    /// Changed during the ChangeState method.
    /// </summary>
    public BaseState<EState> PreviousState { get; private set; }
    
    /// <summary>
    /// Public event for listeners to subscribe to and be notified before a state change begins to the next state.
    /// </summary>
    public event Action<EState> OnBeforeStateChange;
    
    /// <summary>
    /// Public event for listeners to subscribe to and be notified after a state change has finished to a new state.
    /// </summary>
    public event Action<EState> OnAfterStateChange;

    #endregion

    #region Standard Monobehaviour Methods

    /// <summary>
    /// Base awake function that ensures the state machine is enabled if the GameObject is active.
    /// In further classes, dealing with instancing of derived classes and other logic can occur.
    /// </summary>
    protected virtual void Awake()
    {
        if (DebugMode)
        {
            Debug.Log($"Awake {GetType().Name}");
        }
        
        if (gameObject.activeSelf && !enabled)
        {
            Debug.LogWarning($"{GetType().Name} is disabled but GameObject is active. Enabling for state management.");
            
            enabled = true; // Enable if it is active but disabled
        }
    }

    /// <summary>
    /// Base Initialize function that initializes the context states,
    /// then sets and starts with the first state in the States dictionary.
    /// Can be added to inherited classes to define specific context initialization logic, to subscribe to other machine
    /// instance events, etc.
    /// Can be overriden in derived classes to delay initialization and start logic, change the first state entered,
    /// etc.
    /// </summary>
    protected virtual void Start()
    {
        if (DebugMode)
        {
            Debug.Log($"Initialize {GetType().Name}");
        }
        
        Initialize();
        
        if (States == null || States.Count == 0)
        {
            Debug.LogError($"No states defined for {GetType().Name} on Initialize, disabling self.");

            gameObject.SetActive(false);
            
            return;
        }
        
        CurrentState = States.First().Value;
        
        if (CurrentState == null)
        {
            Debug.LogError($"No current state defined for {GetType().Name} on Initialize, disabling state machine.");
            
            gameObject.SetActive(false);
            
            return;
        }
        
        OnBeforeStateChange?.Invoke(CurrentState.State);
        
        CurrentState.EnterState();
        
        OnAfterStateChange?.Invoke(CurrentState.State);

        Started = true;
        
        if (DebugMode)
        {
            Debug.Log($"Started {GetType().Name} with state: {CurrentState.State}");
        }
    }
    
    //Context initialization logic, to be defined in derived classes
    
    /// <summary>
    /// Context initialization method to be defined in derived classes.
    ///
    /// At a base level, this method will be used to connect a state machine and it's corresponding context,
    /// along with getting the states dictionary initialized.
    /// </summary>
    // Example:
    // States = context.ContextInitialize(this);
    protected abstract void Initialize();
    
    /// <summary>
    /// Calls Update function of the current state if not changing state. Can be overridden or added to in derived
    /// classes should additional logic be needed during the update cycle.
    /// </summary>
    protected virtual void Update()
    {
        if (!Started || IsChangingState) return;
        
        CurrentState.UpdateState();
        
        if (DebugMode)
        {
            Debug.Log($"Update {GetType().Name} - Current State: {CurrentState.State}");
        }
    }

    #endregion

    #region State Management
    
    /// <summary>
    /// Changes current state, manages validity of transitions, calls OnBefore/AfterStateChange events at correct times.
    ///
    /// A situation requiring and override or addition is possible, but do so with caution as this method is the core
    /// of the state machine logic.
    /// </summary>
    /// <param name="toState"></param> The state that is being transitioned to, defined in the EState enum.
    protected virtual void ChangeState(EState toState)
    {
        if (!CanChangeToState(toState)) return;
        
        if (DebugMode)
        {
            Debug.Log($"Starting state change from {CurrentState.State} to {toState} in {GetType().Name}");
        }
        
        IsChangingState = true;
        
        OnBeforeStateChange?.Invoke(toState);
        
        CurrentState.ExitState();
        
        PreviousState = CurrentState;
        
        CurrentState = States[toState];
        
        CurrentState.EnterState();
        
        OnAfterStateChange?.Invoke(CurrentState.State);
        
        IsChangingState = false;
        
        if (DebugMode)
        {
            Debug.Log($"State change finished to {CurrentState.State} in {GetType().Name}");
        }
    }
    
    /// <summary>
    /// Method to determine if a target state can currently be transitioned to. At base, Denies if States dict is null
    /// or doesn't contain the target state; Denies transitions to the same state; and Denies if the target state
    /// is listed as an invalid transition state in the current state's InvalidTransitionStates list.
    ///
    /// A situation requiring an override or addition is possible, but do so with caution as this method is a core
    /// part of state machine framework and logic.
    /// </summary>
    /// <param name="toState"></param> The target state to transition to, defined in the EState enum.
    /// <returns></returns> True if the transition is valid, false otherwise.
    protected virtual bool CanChangeToState(EState toState)
    {
        if (States == null || !States.ContainsKey(toState))
        {
            Debug.LogError($"State {toState} not found in {GetType().Name} States dictionary.");
            
            return false;
        }
        
        if (CurrentState.InvalidTransitionStates.Contains(toState))
        {
            Debug.LogWarning($"Invalid state transition from {CurrentState.State} to {toState}");
            
            return false;
        }
        
        if (CurrentState.State.Equals(toState))
        {
            Debug.LogWarning($"Blocked duplicate transition from {CurrentState.State} to {toState}");
            
            return false;
        }
        
        return true;
    }

    #endregion

    #region Context Class

    /// <summary>
    /// The base class for the serialized corresponding "context" to be defined in derived StateMachine classes.
    /// Being privately declared, everything besides logic for the context itself can be public as it will only be
    /// accessed by the machine and its states.
    /// </summary>
    [Serializable]
    public abstract class BaseStateMachineContext
    {
        // TODO: // In derived context classes, declare a reference to the state machine this context belongs to
        //  to be set in the ContextInitialize method.
        //  Example:
        //      public BaseStateMachine stateMachine; 
        
        /// <summary>
        /// Method to be defined in derived classes. Consists of a dictionary where the keys are a state enum and
        /// the values are arrays of states that the key state cannot transition to. Returns that dictionary.
        /// </summary>
        // TODO:
        //  Example:
        //      return new Dictionary<EFooState, EFooState[]>
        //      {
        //          { EFooState.StateA, new [] { } }, // StateA can transition to any state
        //          { EFooState.StateB, new [] { EFooState.StateA } }, // StateB cannot transition to StateA
        //          { EFooState.StateC, new [] { EFooState.StateB } } // StateC cannot transition to StateB
        //      };
        protected abstract Dictionary<EState, EState[]> StatesDict();
        
        /// <summary>
        /// Creates and returns a dictionary of key value pairs where the key is a state enum and the value is with a
        /// corresponding created state instance. Utilizes the StatesDict method to set states correctly.
        /// </summary>
        // TODO:
        //  Example using a FooMachine class, enum, and state classes:
        //     Dictionary<EFooState, BaseState<EFooState>> states = new Dictionary<EFooState, BaseState<EFooState>>();
        //     foreach (var state in InvalidStateTransitions)
        //     {
        //          switch (state.Key)
        //          {
        //              case EFooState.StateA:
        //                 states.Add(state.Key, new FooMachineStateA(this, state.Key, state.Value));
        //                 break;
        //             case EFooState.StateB:
        //                 states.Add(state.Key, new FooMachineStateB(this, state.Key, state.Value));
        //                 break;
        //             case EFooState.StateC:
        //                 states.Add(state.Key, new FooMachineStateC(this, state.Key, state.Value));
        //                 break;
        //         }
        //     }
        //      return states;
        protected abstract Dictionary<EState, BaseState<EState>> InitializedStates();
        
        /// <summary>
        /// Assigns the corresponding state machine that should be defined in derived context classes. Additionally,
        /// returns a freshly initialized dictionary of states with their corresponding state instances for the coupled
        /// state machine to assign as its States property.
        /// </summary>
        /// <param name="targetStateMachine"></param> The state machine instance that this context is serialized with.
        /// <returns></returns> The result of the InitializedStates method.
        // TODO:
        //  Example:
        //     stateMachine = (FooStateMachine) targetStateMachine; // Set the state machine reference
        //     return InitializedStates(); // Return the initialized states dictionary
        public abstract Dictionary<EState, BaseState<EState>> 
            ContextInitialize(BaseStateMachine<EState> targetStateMachine);
        
        /// <summary>
        /// Public method to call from the state machine context or states to change the state of the state machine.
        /// For good practice, changes should only be called in the update portion of state logic,
        /// not in the enter or exit methods.
        /// Outside of that, validity should be checked before relying on a state change working.
        /// </summary>
        // TODO: 
        //  Example:
        //     stateMachine.ChangeState(newState);
        public abstract void ContextCallChangeState(EState newState);
    }

    #endregion

    #region Corresponding State Machine State Abstract Class
    
    /// <summary>
    /// EXAMPLE of the abstract base class to be created in derived state machine classes to allow creation of concrete
    /// state classes that inherit from this base state class. See BaseState.cs for example of implementation
    /// of said state class.
    /// </summary>
    public abstract class FooStateMachineState : BaseState<EState>
    {
        // Context reference to the state machine context, to be set in derived classes
        protected BaseStateMachineContext Context { get; }
        
        // Constructor that sets the context and initializes the base state with key and invalid transitions
        protected FooStateMachineState(BaseStateMachineContext context, EState key, EState[] invalidTransitions) 
            : base(key, invalidTransitions)
        {
            Context = context;
        }
    }
    
    #endregion
}
 