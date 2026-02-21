
public class PioPlayerObjectUi : PlayerInputObject.NewPlayerInputObjectState
{
    public PioPlayerObjectUi(PlayerInputObject.PlayerInputObjectContext context,
        PlayerInputObject.EPlayerInputObjectState key,
        PlayerInputObject.EPlayerInputObjectState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    
    public override void EnterState()
    {
        // Switch to the Player action map
        Context.SetCurrentInputActionMap(PlayerInputObject.PlayerInputObjectContext.EInputActionMap.UI);
    }
    
    public override void UpdateState()
    {
    }
    
    public override void ExitState()
    {
    }
}
