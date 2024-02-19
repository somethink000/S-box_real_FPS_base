using Sandbox;

namespace Facepunch.Arena;

[Group( "Arena" )]
[Title( "Spawn Point Stripper" )]
public sealed class SpawnPointStripper : Component, Component.ExecuteInEditor
{
	[Property] public MapInstance Map { get; set; }
	
	protected override void OnAwake()
	{
		Map.OnMapLoaded += StripSpawnPoints;

		if ( Map.IsLoaded )
		{
			StripSpawnPoints();
		}
		
		base.OnAwake();
	}

	protected override void OnDestroy()
	{
		if ( Map.IsValid() )
		{
			Map.OnMapLoaded -= StripSpawnPoints;
		}
		
		base.OnDestroy();
	}

	private void StripSpawnPoints()
	{
		var spawnpoints = Map.Components.GetAll<SpawnPoint>();

		foreach ( var spawnpoint in spawnpoints )
		{
			spawnpoint.GameObject.Destroy();
		}
	}
}
