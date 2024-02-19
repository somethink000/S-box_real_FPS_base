using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox;
using Sandbox.Citizen;
namespace GeneralGame;
public sealed class NavMeshController : Component
{

	public NavMeshAgent agent;
	private Vector3 _destination;
	public CharacterController playerController;
	RealTimeSince timeSinceUpdate = 0;
	protected override void OnAwake()
	{
		agent = Components.Get<NavMeshAgent>();
		/*playerController = Scene.GetAllComponents<CharacterController>().FirstOrDefault();
		_destination = playerController.Transform.Position;*/
	}

	protected override void OnUpdate()
	{
		playerController = Scene.GetAllComponents<CharacterController>().FirstOrDefault();
		_destination = playerController.Transform.Position;
	}

	protected override void OnFixedUpdate()
	{


		GameObject.Transform.Rotation = Rotation.LookAt( _destination - GameObject.Transform.Position );
		if ( timeSinceUpdate > 0.1 && agent != null )
		{
			timeSinceUpdate = 0;
			agent.MoveTo( _destination );

		}
		/*if ( Vector3.DistanceBetween( _destination, GameObject.Transform.Position ) < 150 && agent != null )
		{
			agent.Stop();
			Log.Info( "Stopped" );


		}*/

	}
}
