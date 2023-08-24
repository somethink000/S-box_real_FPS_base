using Sandbox;

namespace MyGame;
public partial class Weapon : Carriable
{
	public Player Player => Owner as Player;
	public override string ViewModelPath => null;
	public override string WorldModelPath => null;
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	public virtual float PrimaryRate => 5.0f;

	public override void Spawn()
	{
		base.Spawn();
		
	}
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
	}
	public override void Simulate( IClient player )
	{
		


		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSincePrimaryAttack = 0;
				PrimaryAttack();
			}
		}

	}



	public virtual bool CanPrimaryAttack()
	{

		if ( !Owner.IsValid() || !Input.Down( "attack1" ) ) return false;


		var rate = PrimaryRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual void PrimaryAttack() { }


	[ClientRpc]
	public override void CreateViewModel()
	{
		base.CreateViewModel();
	}

	public override void OnActiveStart()
	{
		base.OnActiveStart();
	}
}
