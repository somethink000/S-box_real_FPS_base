using Microsoft.VisualBasic;
using Sandbox;
using System.Collections.Generic;

namespace GeneralGame;

public static class GameObjectExtensions
{

	public static void DestroyAsync( this GameObject self, float seconds = 1.0f )
	{
		var component = self.Components.Create<TimedDestroyComponent>();
		component.Time = seconds;
	}

}

public sealed class TimedDestroyComponent : Component
{
	[Property] public float Time { get; set; } = 1f;
	[Property, ReadOnly] TimeUntil TimeUntilDestroy { get; set; } = 0;

	protected override void OnStart()
	{
		TimeUntilDestroy = Time;
	}

	protected override void OnUpdate()
	{
		if ( TimeUntilDestroy )
		{
			GameObject.Destroy();
		}
	}
}
