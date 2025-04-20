
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
	[Property, Category( "Relatives" )] public CitizenAnimationHelper Animator { get; set; }

	[Property] public bool IsFirstPerson { get; set; } = true;

	public bool IsAlive => HealthController.IsAlive;
	public List<ChatEntry> StoredChat { get; set; } = new();
	
	protected override void OnStart()
	{
		base.OnStart();


		if ( IsProxy )
		{
			if ( Camera is not null )
				Camera.Enabled = false;

			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.On;
		}


		if ( !IsProxy && IsFirstPerson )
		{

			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}

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
