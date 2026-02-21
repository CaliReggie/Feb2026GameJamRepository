
public class PioOff : PlayerInputObject.NewPlayerInputObjectState
{
    public PioOff(PlayerInputObject.PlayerInputObjectContext context,
        PlayerInputObject.EPlayerInputObjectState key,
        PlayerInputObject.EPlayerInputObjectState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    
    public override void EnterState()
    {
        // Disable all input by setting to None action map
        Context.SetCurrentInputActionMap(PlayerInputObject.PlayerInputObjectContext.EInputActionMap.None);
    }
    
    public override void UpdateState()
    {
    }
    
    public override void ExitState()
    {
    }
}
