using System.Collections.Generic;
using Sandbox;

namespace GeneralGame;

public class WeaponManager : Component
{

	[Property] public List<PrefabScene> Prefabs { get; set; }

	public static WeaponManager Instance { get; private set; }
	public List<GameObject> Weapons { get; set; } = new();
	
	
	protected override void OnAwake()
	{
		Instance = this;

		foreach ( var prefab in Prefabs )
		{
			Weapons.Add( prefab );
		}
		
		base.OnAwake();
	}

	protected override void OnDestroy()
	{
		Instance = null;
		base.OnDestroy();
	}
}
