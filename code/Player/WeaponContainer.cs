using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace GeneralGame;


public sealed class WeaponContainer : Component
{
	[Property] public PrefabScene StartingWeapon { get; set; }
	[Property] public GameObject WeaponBone { get; set; }
	[Property] public AmmoContainer Ammo { get; set; }
	[Property] public PlayerController PlayrControl { get; set; }

	public WeaponComponent Deployed => Components.GetAll<WeaponComponent>( FindMode.EverythingInSelfAndDescendants ).FirstOrDefault( c => c.IsDeployed );
	public IEnumerable<WeaponComponent> All => Components.GetAll<WeaponComponent>( FindMode.EverythingInSelfAndDescendants );
	public bool HasAny => All.Any();

	public bool Has( GameObject prefab )
	{
		//TODO make this beter
		return All.Any( w => w.GameObject.Components.GetInDescendantsOrSelf<WeaponComponent>( true ).DisplayName == prefab.Components.GetInDescendantsOrSelf<WeaponComponent>( true ).DisplayName );
	}

	public void Clear()
	{
		if ( IsProxy ) return;

		foreach ( var weapon in All )
		{
			weapon.GameObject.Destroy();
		}
	}

	public void GiveDefault()
	{
		if ( IsProxy ) return;
		if ( !StartingWeapon.IsValid() ) return;
		
		Give( StartingWeapon, true );
	}
	
	public void Give( GameObject prefab, bool shouldDeploy = false )
	{
		if ( IsProxy ) return;

		var weaponGo = prefab.Clone();
		var weapon = weaponGo.Components.GetInDescendantsOrSelf<WeaponComponent>( true );
		weapon.owner = PlayrControl;
		if ( !weapon.IsValid() )
		{
			weaponGo.DestroyImmediate();
			return;
		}

		if ( shouldDeploy )
		{
			foreach ( var w in All )
			{
				w.Holster();
			}
		}

		weaponGo.SetParent( WeaponBone );
		weaponGo.Transform.Position = WeaponBone.Transform.Position;
		weaponGo.Transform.Rotation = WeaponBone.Transform.Rotation;

		var nextWeponGo = weaponGo.Components.GetInDescendantsOrSelf<BaseGun>( true );
		if ( nextWeponGo.IsValid() ) {
			
			nextWeponGo.AmmoInClip = nextWeponGo.ClipSize;
			nextWeponGo.IsDeployed = !Deployed.IsValid();
			
			var ammoToGive = nextWeponGo.DefaultAmmo - Ammo.Get( nextWeponGo.AmmoType );

			if ( ammoToGive > 0 )
			{
				Ammo.Give( nextWeponGo.AmmoType, ammoToGive );
			}
		}

		weaponGo.NetworkSpawn();
		weaponGo.Components.Get<ModelCollider>().Destroy();
		weaponGo.Components.Get<Rigidbody>().Destroy();
	}
	
	public void Next()
	{
		if ( !HasAny ) return;
		
		var weapons = All.ToList();
		var currentIndex = -1;
		var deployed = Deployed;

		if ( deployed.IsValid() )
		{
			currentIndex = weapons.IndexOf( deployed );
		}

		var nextIndex = currentIndex + 1;
		if ( nextIndex >= weapons.Count )
			nextIndex = 0;
		
		var nextWeapon = weapons[nextIndex];
		if ( nextWeapon == deployed )
			return;

		foreach ( var weapon in weapons.Where( weapon => weapon != nextWeapon ) )
		{
			weapon.Holster();
		}

		nextWeapon.Deploy();
	}
	
	public void Previous()
	{
		if ( !HasAny ) return;
		
		var weapons = All.ToList();
		var currentIndex = -1;
		var deployed = Deployed;

		if ( deployed.IsValid() )
		{
			currentIndex = weapons.IndexOf( deployed );
		}

		var previousIndex = currentIndex - 1;
		if ( previousIndex < 0 )
			previousIndex = weapons.Count - 1;

		var previousWeapon = weapons[previousIndex];
		if ( previousWeapon == deployed )
			return;
		
		foreach ( var weapon in weapons.Where( weapon => weapon != previousWeapon ) )
		{
			weapon.Holster();
		}

		previousWeapon.Deploy();
	}
}
