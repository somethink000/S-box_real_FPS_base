using Sandbox;

namespace GeneralGame;
public enum LifeState
{
	Dead,
	Alive
}


public interface IHealthComponent
{
	public LifeState LifeState { get; }
	void OnDamage( in DamageInfo damage );

}
