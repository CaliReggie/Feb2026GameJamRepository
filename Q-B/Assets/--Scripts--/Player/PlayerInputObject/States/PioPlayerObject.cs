
public class PioPlayerObject : PlayerInputObject.NewPlayerInputObjectState
{
    public PioPlayerObject(PlayerInputObject.PlayerInputObjectContext context,
        PlayerInputObject.EPlayerInputObjectState key,
        PlayerInputObject.EPlayerInputObjectState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    
    public override void EnterState()
    {
        // Switch to the Player action map
        Context.SetCurrentInputActionMap(PlayerInputObject.PlayerInputObjectContext.EInputActionMap.Player);
    }
    
    public override void UpdateState()
    {
    }
    
    public override void ExitState()
    {
    }
}
