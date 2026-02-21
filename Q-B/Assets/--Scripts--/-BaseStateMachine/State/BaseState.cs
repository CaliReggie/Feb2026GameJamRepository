using System;

/// <summary>
/// The generic base state class. See BaseStateMachine for more information on how to link with a derived state machine.
/// SEE BaseStateMachine.cs FOR IMPLEMENTATION EXAMPLE
/// </summary>
public abstract class BaseState<EState> where EState : Enum
{
    #region Base Properties and Constructor

    /// <summary>
    /// The state key for this state. This is used to identify the state in the state machine.
    /// </summary>
    public EState State { get; }
    
    /// <summary>
    /// The invalid transition states for this state.
    /// These are the states that cannot be transitioned to from this state.
    /// </summary>
    public EState [] InvalidTransitionStates { get; }
    
    /// <summary>
    /// The base state constructor. This is used to initialize the state key and invalid transition states.
    /// </summary>
    /// <param name="key"></param> Corresponds to the state key for this state.
    /// <param name="invalidTransitions"></param> Corresponds to the invalid transition states for this state.
    public BaseState(EState key, EState[] invalidTransitions)
    {
        State = key;
        
        InvalidTransitionStates = invalidTransitions;
    } 

    #endregion

    #region State Method Calls

    /// <summary>
    /// Called once each time the state is entered. State changes should NOT be made in this method.
    /// </summary>
    public abstract void EnterState();
    
    /// <summary>
    /// Called once each time the current state equals this one and updated in the corresponding state machine.
    /// State changes CAN be made in this method, and are expected to be made in the state machine's update loop.
    /// </summary>
    public abstract void UpdateState();
 
    /// <summary>
    /// Called once each time the state is exited. State changes should NOT be made in this method.
    /// </summary>
    public abstract void ExitState();

    #endregion

    #region Example Implementation
    
    // See BaseStateMachine.cs for an example of extending base state to the FooMachineState seen below.
    // Once done, you can create a new state class that extends this base state class, like so:
    //
    // public class FooMachineStateA : FooMachine.FooMachineState
    // {
    //     public FooMachineStateA(FooMachine.FooMachineContext context, 
    //         FooMachine.EFooState key, 
    //         FooMachine.EFooState[] validTransitions) 
    //         : base(context, key, validTransitions)
    //     {
    //     }
    //     
    //     public override void EnterState()
    //     {
    //         Debug.Log("Entering State A");
    //     }
    //     public override void UpdateState()
    //     {
    //         Debug.Log("Updating State A");
    //     }
    //     
    //     public override void ExitState()
    //     {
    //         Debug.Log("Exiting State A");
    //     }
    // }

    #endregion
}
