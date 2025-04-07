
using Sandbox.Citizen;
using Sandbox.Services;
using Sandbox;
using System;
using System.Linq;
using static Sandbox.Connection;

namespace GeneralGame;


public partial class Player : Component, Component.INetworkSpawn
{
	[RequireComponent] public MovementController MovementController { get; set; }
	[RequireComponent] public CameraController CameraController { get; set; }
	[RequireComponent] public HealthController HealthController { get; set; }
	
	[Property, Category( "Relatives" )] public GameObject Head { get; set; }
	[Property, Category( "Relatives" )] public GameObject Body { get; set; }
	[Property, Category( "Relatives" )] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property, Category( "Relatives" )] public CameraComponent Camera { get; set; }
	[Property, Category( "Relatives" )] public PanelComponent RootDisplay { get; set; }
	[Property, Category( "Relatives" )] public Voice Voice { get; set; }
	[Property, Category( "Relatives" )] public ModelPhysics ModelPhysics { get; set; }

	public RagdollManager RagdollManager { get; set; }

	public bool IsAlive => HealthController.IsAlive;
	public Ray ViewRay => new( CameraController.EyePos, Camera.WorldRotation.Forward );

	protected override void OnAwake()
	{
		RagdollManager = new RagdollManager( ModelPhysics );
	}
	
	public void OnNetworkSpawn( Connection connection )
	{
		
	}

	protected override void OnStart()
	{

		if ( IsProxy )
		{
			if ( Camera is not null )
				Camera.Enabled = false;
		}

		base.OnStart();
	}
	
	protected override void OnUpdate()
	{
		
		if ( !IsProxy )
		{
			BodyRenderer.SetBodyGroup( "head", 2 );
		}

	}

	protected override void OnFixedUpdate()
	{
		if ( !HealthController.IsAlive ) return;


		if (IsProxy) return;

		UpdateInteractions();
	}
}
