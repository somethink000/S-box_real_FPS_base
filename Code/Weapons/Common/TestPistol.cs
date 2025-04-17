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

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public override void Deploy( Player player )
	{

		base.Deploy( player );

		//if ( !IsProxy )
		//	ViewModelRenderer?.Set( IsReady ? DeployAnim : ReadyAnim, true );
	}

	public override bool CanHolster()
	{
		return true;
	}

}
