﻿
using Microsoft.VisualBasic;
using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Linq;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.DecalDefinition;
using static Sandbox.SerializedProperty;
using static Sandbox.Services.Inventory;

namespace GeneralGame;

public partial class Carriable : Component
{
	[RequireComponent] public SkinnedModelRenderer WorldModelRenderer { get; set; }
	[Property] public string Name { get; set; }
	[Property] public Texture Icon { get; set; }
	[Property] public Model ViewModel { get; set; }
	[Property] public Model ViewModelHands { get; set; }
	[Property] public Model WorldModel { get; set; }
	[Property] public AngPos RunAnimData { get; set; }
	[Property] public float TuckRange { get; set; } = 30f;
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.Pistol;
	[Property] public CitizenAnimationHelper.Hand Hand { get; set; } = CitizenAnimationHelper.Hand.Both;
	[Property] public float AnimSpeed { get; set; } = 1;

	public ViewModel ViewModelHandler { get; protected set; }
	public SkinnedModelRenderer ViewModelRenderer { get; private set; }
	public SkinnedModelRenderer ViewModelHandsRenderer { get; private set; }

	public bool CanSeeViewModel => !IsProxy && Owner.IsFirstPerson;
	[Sync( SyncFlags.FromHost )] public Player Owner { get; set; }
	
	public bool IsRunning => Owner != null && Owner.MovementController.IsRunning && Owner.MovementController.IsOnGround && Owner.MovementController.Velocity.Length >= 200;
	public bool IsCrouching => Owner.MovementController.IsCrouching;

	protected bool _deployed { get; set; } = false;//i have this variable because of s&box bug

	protected override void OnStart()
	{
		var interactions = Components.GetOrCreate<Interactions>();

		interactions.AddInteraction( new Interaction()
		{
			Description = "Pickup",
			Key = "use",
			Action = ( Player player, GameObject obj ) =>
			{
				player.InventoryController.GiveItem( this );
				OnPickUp( player );
			},
			//Disabled = () => !PlayerBase.GetLocal().Inventory.HasSpaceInBackpack(),
		} );


		if ( IsProxy )
		{
			WorldModelRenderer.RenderType = ModelRenderer.ShadowRenderType.On;
			
		}
	}
	protected virtual void OnPickUp(Player ply) { }
	protected virtual void SetupAnimEvents(){}


	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public virtual void Deploy( )
	{
		WorldModelRenderer.Tint = WorldModelRenderer.Tint.WithAlpha( 1 );
		Owner.Animator.HoldType = HoldType;
		Owner.Animator.Handedness = Hand;
		
		if ( !IsProxy && WorldModelRenderer is not null )
		{
			WorldModelRenderer.RenderType = Owner.IsFirstPerson && ViewModel is not null ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;
		}

		_deployed = true;
		SetupModels();
	}

	public virtual bool CanHolster()
	{
		return true;
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public virtual void Holster()
	{
		_deployed = false;
		Owner.Animator.HoldType = CitizenAnimationHelper.HoldTypes.None;
		WorldModelRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
		WorldModelRenderer.Tint = WorldModelRenderer.Tint.WithAlpha( 0 );

		if ( !IsProxy && ViewModelHandler != null )
		{
			
			ViewModelHandler.OnHolster();
			ViewModelRenderer.GameObject.Destroy();
			ViewModelHandler = null;
			ViewModelRenderer = null;
			ViewModelHandsRenderer = null;
			
		
		}

	}

	protected virtual void SetupViewModel(GameObject viewModelGO )
	{
		ViewModelHandler = viewModelGO.Components.Create<ViewModel>();
		ViewModelHandler.Carriable = this;
		ViewModelHandler.ViewModelRenderer = ViewModelRenderer;
		ViewModelHandler.Camera = Owner.Camera;
	}

	void SetupModels()
	{
		

		if ( !IsProxy && ViewModel is not null && ViewModelRenderer is null && Owner.IsFirstPerson )
		{

			var viewModelGO = new GameObject( true, "Viewmodel" );
			viewModelGO.SetParent( Owner.GameObject, false );
			viewModelGO.Tags.Add( TagsHelper.ViewModel );
			viewModelGO.NetworkMode = NetworkMode.Never;
			viewModelGO.Flags |= GameObjectFlags.NotNetworked;

			ViewModelRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
			ViewModelRenderer.Model = ViewModel;
			ViewModelRenderer.AnimationGraph = ViewModel.AnimGraph;
			ViewModelRenderer.CreateBoneObjects = true;
			ViewModelRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
			ViewModelRenderer.Enabled = false;
			SetupViewModel( viewModelGO );
			

			if ( ViewModelHands is not null )
			{
				ViewModelHandsRenderer = viewModelGO.Components.Create<SkinnedModelRenderer>();
				ViewModelHandsRenderer.Model = ViewModelHands;
				ViewModelHandsRenderer.BoneMergeTarget = ViewModelRenderer;
				ViewModelHandsRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
				ViewModelHandsRenderer.Enabled = true;
			}
			ViewModelRenderer.Enabled = true;
			ViewModelHandler.ViewModelHandsRenderer = ViewModelHandsRenderer;

			
			

			SetupAnimEvents();
		}


		var bodyRenderer = Owner.BodyRenderer;
		WorldModelRenderer.BoneMergeTarget = bodyRenderer;
		Network.ClearInterpolation();
	}

	public virtual SceneTraceResult MakeTrace( Vector3 start, Vector3 end, float radius = 2.0f )
	{
	    var startsInWater = SurfaceUtil.IsPointWater( start );
		List<string> withoutTags = new() { TagsHelper.Trigger, TagsHelper.PlayerClip, TagsHelper.PassBullets, TagsHelper.ViewModel };

		if ( startsInWater )
			withoutTags.Add( TagsHelper.Water );

		var tr = Scene.Trace.Ray( start, end )
				.UseHitboxes()
				.WithoutTags( withoutTags.ToArray() )
				.Size( radius )
				.IgnoreGameObjectHierarchy( Owner.GameObject )
				.Run();

		return tr;
	}

	public virtual float GetTuckDist()
	{
		if ( TuckRange == -1 )
			return -1;

		if ( !Owner.IsValid ) return -1;

		var pos = Owner.CameraController.EyePos;
		var forward = Owner.CameraController.EyeAngles.ToRotation().Forward;
		var trace = MakeTrace( Owner.CameraController.EyePos, pos + forward * TuckRange );

		if ( !trace.Hit )
			return -1;

		return trace.Distance;
	}

	public bool ShouldTuck()
	{
		return GetTuckDist() != -1;
	}

	public bool ShouldTuck( out float dist )
	{
		dist = GetTuckDist();
		return dist != -1;
	}


	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	protected void PlaySound( int resourceID )
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
			Sound.Play( sound, WorldPosition );
		}
	}

}

