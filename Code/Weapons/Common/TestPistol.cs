using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class BasePistol : Carriable
{

	protected override void OnStart()
	{
		base.OnStart();

	}

	protected override void OnPickUp( Player ply )
	{
		
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

	}


	public override void Deploy()
	{
		base.Deploy();
	}
	

	public override bool CanHolster()
	{
		
		return true;
	}

}
