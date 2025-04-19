
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
	//Ѕулка ты чЄ тут делаешь?
	[Property, Category( "Relatives" )] public GameObject Head { get; set; }
	[Property, Category( "Relatives" )] public GameObject Body { get; set; }
	[Property, Category( "Relatives" )] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property, Category( "Relatives" )] public CameraComponent Camera { get; set; }
	[Property, Category( "Relatives" )] public PanelComponent RootDisplay { get; set; }
	[Property, Category( "Relatives" )] public Voice Voice { get; set; }
	[Property, Category( "Relatives" )] public ModelPhysics ModelPhysics { get; set; }

	[Property] public bool IsFirstPerson { get; set; } = true;

	GameObject firstPersonBodyGO { get; set; }

	public bool IsAlive => HealthController.IsAlive;
	public List<ChatEntry> StoredChat { get; set; } = new();
	public List<CitizenAnimationHelper> Animators { get; private set; } = new(); // List of animators, one for shadow, one for yor view

	protected override void OnStart()
	{
		base.OnStart();


		if ( IsProxy )
		{
			if ( Camera is not null )
				Camera.Enabled = false;
				
		}


		BodyRenderer.SetBodyGroup( "head", IsProxy ? 0 : 2 );

		//This is so fucked up
		if ( !IsProxy && IsFirstPerson )
		{
			// Create shadow model only on client
			firstPersonBodyGO = new GameObject( true, "Viewmodel" );
			firstPersonBodyGO.SetParent( GameObject, false );
			firstPersonBodyGO.NetworkMode = NetworkMode.Never;
			
			SkinnedModelRenderer ShadowBodyRenderer = firstPersonBodyGO.Components.Create<SkinnedModelRenderer>();
			ShadowBodyRenderer.Model = BodyRenderer.Model;
			ShadowBodyRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;

			CitizenAnimationHelper ShadowAnimator = firstPersonBodyGO.Components.Create<CitizenAnimationHelper>();
			ShadowAnimator.Target = ShadowBodyRenderer;
			ShadowAnimator.Enabled = false;
			ShadowAnimator.OnComponentEnabled += () =>
			{
				
				Animators.Add( ShadowAnimator );

				BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
			};

			ShadowAnimator.Enabled = true;
			
		}

		Animators.Add( Components.Get<CitizenAnimationHelper>() );

	}

	protected override void OnUpdate()
	{

	}



	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
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
