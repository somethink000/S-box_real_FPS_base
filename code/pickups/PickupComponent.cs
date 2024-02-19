using System;
using Sandbox;

namespace Facepunch.Arena;

[Hide]
public abstract class PickupComponent : Component
{
	[Property] public Collider Collider { get; set; }
	[Property] public SoundEvent PickupSound { get; set; }
	
	private Vector3 StartPosition { get; set; }
	
	public void Pickup( GameObject picker )
	{
		if ( !Network.IsOwner )
			return;
		
		if ( !GameObject.IsValid() )
			return;

		PlayPickupSound();
		OnPickup( picker.Id );
		
		GameObject.Destroy();
	}
	
	protected virtual void OnPickup( Guid pickerId )
	{
		
	}

	protected override void OnStart()
	{
		
		StartPosition = Transform.Position;
		//Collider.IsTrigger = true;
		
		base.OnStart();
	}

	
	[Broadcast]
	private void PlayPickupSound()
	{
		if ( PickupSound is null )
			return;
		
		Sound.Play( PickupSound, Transform.Position );
	}
}
