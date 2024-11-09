using Sandbox;
using System;
using static Sandbox.SerializedProperty;

namespace General;

public partial class Player
{
	[Property] public int MaxHealth { get; set; } = 100;
	[Sync] public int Health { get; set; } = 100;
	[Sync] public int Kills { get; set; }
	[Sync] public int Deaths { get; set; }
	[Sync] public bool GodMode { get; set; }

	public bool IsAlive => Health > 0;

}
