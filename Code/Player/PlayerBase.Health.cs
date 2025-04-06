

namespace GeneralGame;

public partial class PlayerBase
{
	[Sync, Property] public float MaxHealth { get; set; } = 100f;
	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	//private set;
	[Sync] public float Health { get; set; } = 100f;


	[Sync] public int Kills { get; set; }
	[Sync] public int Deaths { get; set; }
	[Sync] public bool GodMode { get; set; }
	[Property] public ColorAdjustments Adjustments { get; set; }
	[Property] public Vignette Vignette { get; set; }

	public bool IsAlive => Health > 0;



	public void FixedHealthEffectUpdate()
	{
		var health = (1f / MaxHealth) * Health;

		Adjustments.Saturation = 1f - (1f - health) * 0.3f;
		Vignette.Intensity = 0.5f * (1f - health);
	}

	public void OnDamage( in DamageInfo damage )
	{
		if ( !IsAlive || GodMode )
			return;

		Health -= damage.Damage;


		if ( Health <= 0 )
			OnDeath( damage );
	}


}
