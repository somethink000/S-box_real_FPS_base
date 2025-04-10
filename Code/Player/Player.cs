
using Sandbox.Citizen;
using Sandbox.Services;
using Sandbox;
using System;
using System.Linq;
using static Sandbox.Connection;
using GeneralGame.UI;

namespace GeneralGame;


public partial class Player : Component
{
	[RequireComponent] public MovementController MovementController { get; set; }
	[RequireComponent] public CameraController CameraController { get; set; }
	[RequireComponent] public HealthController HealthController { get; set; }
	[RequireComponent] public InteractionController InteractionController { get; set; }
	[RequireComponent] public InventoryController InventoryController { get; set; }

	[Property, Category( "Relatives" )] public GameObject Head { get; set; }
	[Property, Category( "Relatives" )] public GameObject Body { get; set; }
	[Property, Category( "Relatives" )] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property, Category( "Relatives" )] public CameraComponent Camera { get; set; }
	[Property, Category( "Relatives" )] public PanelComponent RootDisplay { get; set; }
	[Property, Category( "Relatives" )] public Voice Voice { get; set; }
	[Property, Category( "Relatives" )] public ModelPhysics ModelPhysics { get; set; }

	[Property] public bool IsFirstPerson { get; set; } = true;
	public RagdollManager RagdollManager { get; set; }
	
	public bool IsAlive => HealthController.IsAlive;
	public List<ChatEntry> StoredChat { get; set; } = new();
	public List<CitizenAnimationHelper> Animators { get; private set; } = new(); // List of animators, one for shadow, one for yor view


	protected override void OnAwake()
	{
		RagdollManager = new RagdollManager( ModelPhysics );
	}
	

	protected override void OnStart()
	{

		if ( IsProxy )
		{
			if ( Camera is not null )
				Camera.Enabled = false;
		}else if ( IsFirstPerson )
		{

			// Create shadow model only on client
			SkinnedModelRenderer ShadowBodyRenderer = GameObject.Components.Create<SkinnedModelRenderer>();
			ShadowBodyRenderer.Model = BodyRenderer.Model;
			ShadowBodyRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;

			BodyRenderer.SetBodyGroup( "head", 2 );

			CitizenAnimationHelper ShadowAnimator = GameObject.Components.Create<CitizenAnimationHelper>();
			ShadowAnimator.Target = ShadowBodyRenderer;

			Animators.Add( ShadowAnimator );

			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
		}

		Animators.Add( Components.Get<CitizenAnimationHelper>() );

		
		base.OnStart();
	}
	
	protected override void OnUpdate()
	{
		

	}

	

	[Rpc.Broadcast]
	public void NewEntry( string author, string message )
	{
		UI.Chat.Instance.AddTextLocal( author, message );
	}


	public void AddEntry( string author, string message )
	{
		if ( IsProxy ) return;

		StoredChat.Add( new( author, message, 0f ) );

	}
}
