using System;
using Sandbox;

namespace Facepunch.Arena;

public interface IHealthComponent
{
	public LifeState LifeState { get; }
	public float MaxHealth { get; }
	public float Health { get; }
	public void TakeDamage( DamageType type, float damage, Vector3 position, Vector3 force, Guid attackerId );
}
