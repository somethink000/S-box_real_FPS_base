using System.Collections.Generic;
using Sandbox;

namespace GeneralGame;

[Group( "Arena" )]
[Title( "Ammo Container" )]
public sealed class AmmoContainer : Component
{
	private Dictionary<AmmoType, int> AmmoCount { get; set; } = new();

	public void Give( AmmoType type, int ammo )
	{
		if ( AmmoCount.TryAdd( type, ammo ) )
			return;

		AmmoCount[type] += ammo;
	}

	public bool TryTake( AmmoType type, int amount, out int taken )
	{
		var ammo = Get( type );
		if ( ammo == 0 )
		{
			taken = 0;
			return false;
		}

		if ( ammo >= amount )
		{
			taken = amount;
			AmmoCount[type] -= taken;
			return true;
		}

		taken = ammo;
		AmmoCount[type] = 0;
		return true;
	}

	public bool CanTake( AmmoType type, int amount, out int taken )
	{
		var ammo = Get( type );
		if ( ammo == 0 )
		{
			taken = 0;
			return false;
		}

		if ( ammo >= amount )
		{
			taken = amount;
			return true;
		}

		taken = ammo;
		return true;
	}

	public int Get( AmmoType type )
	{
		return CollectionExtensions.GetValueOrDefault( AmmoCount, type, 0 );
	}
}
