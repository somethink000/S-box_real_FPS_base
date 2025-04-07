using Microsoft.VisualBasic;
using Sandbox;
using System.Collections.Generic;

namespace GeneralGame;

public static class GameObjectExtensions
{

	public static IEnumerable<Interaction> GetInteractions( this GameObject obj )
	{
		return obj.Components.Get<Interactions>( FindMode.EverythingInSelf )?.AllInteractions;
	}


}
