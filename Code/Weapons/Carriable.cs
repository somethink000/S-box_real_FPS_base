
using Microsoft.VisualBasic;
using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public partial class Carriable : Component, IInteractable
{
	[Property, Group( "Models" )] public Model ViewModel { get; set; }
	[Property, Group( "Models" )] public Model ViewModelHands { get; set; }
	[Property, Group( "Models" )] public Model WorldModel { get; set; }
	[Property, Group( "Animations" ), Title( "Run Offset" )] public AngPos RunAnimData { get; set; }
	[Property, Group( "General" )] public float TuckRange { get; set; } = 30f;
	[Property, Group( "General" )] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.Pistol;
	public bool IsRunning => Owner != null && Owner.MovementController.IsRunning && Owner.MovementController.IsOnGround && Owner.MovementController.Velocity.Length >= 200;
	public ViewModel ViewModelHandler { get; private set; }
	public SkinnedModelRenderer ViewModelRenderer { get; private set; }
	public SkinnedModelRenderer ViewModelHandsRenderer { get; private set; }
	public SkinnedModelRenderer WorldModelRenderer { get; private set; }
	public bool CanSeeViewModel => !IsProxy && Owner.CameraController.IsFirstPerson;
	public Player Owner { get; set; }
	public List<Interaction> Interactions { get; set; } = new List<Interaction>();


	protected override void OnAwake()
	{
		base.OnAwake();

		WorldModelRenderer = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>();
	}

	protected override void OnStart()
	{

		Interactions.Add(
			new Interaction()
			{
				Key = "use",
				Action = ( Player player, GameObject obj ) =>
				{
					player.InventoryController.GiveItem( this );
				},

			}
		);

	}

	protected override void OnUpdate()
	{
		if ( Owner == null ) return;

			if ( !IsProxy && WorldModelRenderer is not null )
			{
				WorldModelRenderer.RenderType = Owner.CameraController.IsFirstPerson ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;
			}
	}

	[Rpc.Broadcast]
	public virtual void Deploy( Player player )
	{
		Owner = player;
		GameObject.Enabled = true;
		Log.Info( "wd" );
		SetupModels();
		if ( IsProxy ) return;


	}
	public virtual bool CanHolster()
	{
		return false;
	}
	public virtual void Holster()
	{
		GameObject.Enabled = false;

		if ( !IsProxy )
		{

			ViewModelHandler.OnHolster();

			WorldModelRenderer.RenderType = ModelRenderer.ShadowRenderType.On;
			ViewModelRenderer.GameObject.Destroy();
			ViewModelHandler = null;
			ViewModelRenderer = null;
			ViewModelHandsRenderer = null;
		}

		Owner = null;
	}


	void SetupModels()
	{

		if ( !IsProxy && ViewModel is not null && ViewModelRenderer is null )
		{

			var viewModelGO = new GameObject( true, "Viewmodel" );
			viewModelGO.SetParent( Owner.GameObject, false );
			viewModelGO.Tags.Add( TagsHelper.ViewModel );
			viewModelGO.Flags |= GameObjectFlags.NotNetworked;

			ViewModelRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
			ViewModelRenderer.Model = ViewModel;
			ViewModelRenderer.AnimationGraph = ViewModel.AnimGraph;
			ViewModelRenderer.CreateBoneObjects = true;
			ViewModelRenderer.Enabled = false;
			ViewModelRenderer.OnComponentEnabled += () =>
			{
				// Prevent flickering when enabling the component, this is controlled by the ViewModelHandler

				ViewModelRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
				// Start drawing
				ViewModelHandler.ShouldDraw = true;
			};

			ViewModelHandler = viewModelGO.Components.Create<ViewModel>();
			ViewModelHandler.Carriable = this;
			ViewModelHandler.ViewModelRenderer = ViewModelRenderer;
			ViewModelHandler.Camera = Owner.Camera;

			if ( ViewModelHands is not null )
			{
				ViewModelHandsRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
				ViewModelHandsRenderer.Model = ViewModelHands;
				ViewModelHandsRenderer.BoneMergeTarget = ViewModelRenderer;
				ViewModelHandsRenderer.OnComponentEnabled += () =>
				{
					// Prevent flickering when enabling the component, this is controlled by the ViewModelHandler
					ViewModelHandsRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
				};
			}

			ViewModelHandler.ViewModelHandsRenderer = ViewModelHandsRenderer;

		}

		var bodyRenderer = Owner.Body.Components.Get<SkinnedModelRenderer>();
		WorldModelRenderer.BoneMergeTarget = bodyRenderer;
		Network.ClearInterpolation();
	}




	[Rpc.Broadcast]
	void PlaySound( int resourceID )
	{
		if ( !IsValid ) return;

		var sound = ResourceLibrary.Get<SoundEvent>( resourceID );
		if ( sound is null ) return;

		var isScreenSound = CanSeeViewModel;
		sound.UI = isScreenSound;

		if ( isScreenSound )
		{
			sound.Volume = 0.7f;
			Sound.Play( sound );
		}
		else
		{
			Sound.Play( sound, Transform.Position );
		}
	}

}





