using Sandbox;
using System.Collections.Generic;


namespace MyGame;
public partial class Melee : Weapon
{
	[Net, Predicted] public TimeSince TimeSinceSecondaryAttack { get; set; }
	public virtual float SecondaryRate => 5.0f;

	public override void Simulate( IClient player )
	{

		base.Simulate( player );

		if ( CanSecondaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSinceSecondaryAttack = 0;
				SecondaryAttack();
			}
		}

	}

	public override void Spawn()
	{
		base.Spawn();

	}



	public virtual bool CanSecondaryAttack()
	{

		if ( !Owner.IsValid() || !Input.Down( "attack2" ) ) return false;


		var rate = SecondaryRate;
		if ( rate <= 0 ) return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	public virtual void SecondaryAttack() { }


	public IEnumerable<TraceResult> TraceMelee( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( this );

		var tr = trace.Run();

		if ( tr.Hit )
		{
			yield return tr;
		}
		else
		{
			trace = trace.Size( radius );

			tr = trace.Run();

			if ( tr.Hit )
			{
				yield return tr;
			}
		}
	}
}
