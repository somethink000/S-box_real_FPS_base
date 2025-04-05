
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;

namespace GeneralGame;

//Body visability shit 
public class PlayerBody : Component
{
	[Property] public SkinnedModelRenderer ModelRenderer { get; private set; }
	private PlayerObject ply { get; set; }
	private CameraComponent Camera { get; set; }
	private PlayerController Controller { get; set; }


	protected override void OnAwake()
	{
		base.OnAwake();
		ply = GameObject.Components.Get<PlayerObject>();
		Camera = ply.CameraController.Camera;
		Controller = ply.Controller;
	}

	private void UpdateModelVisibility()
	{

		if ( !ModelRenderer.IsValid() )
			return;

		//damn this shit works
		if ( IsProxy ) Camera.Enabled = false;


		var deployedWeapon = ply.Weapons.Deployed;
		var shadowRenderer = Controller.ShadowAnimator.Components.Get<SkinnedModelRenderer>( true );
		var hasViewModel = deployedWeapon.IsValid() && deployedWeapon.HasViewModel;
		var clothing = ModelRenderer.Components.GetAll<ClothingComponent>( FindMode.EverythingInSelfAndDescendants );


		if ( hasViewModel )
		{
			shadowRenderer.Enabled = false;

			ModelRenderer.Enabled = ply.Ragdoll.IsRagdolled;
			ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;

			foreach ( var c in clothing )
			{
				c.ModelRenderer.Enabled = ply.Ragdoll.IsRagdolled;
				c.ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;
			}

			return;
		}

		ModelRenderer.SetBodyGroup( "head", IsProxy ? 0 : 1 );
		ModelRenderer.Enabled = true;

		if ( ply.Ragdoll.IsRagdolled )
		{
			ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;
			shadowRenderer.Enabled = false;
		}
		else
		{
			ModelRenderer.RenderType = IsProxy
				? Sandbox.ModelRenderer.ShadowRenderType.On
				: Sandbox.ModelRenderer.ShadowRenderType.Off;

			shadowRenderer.Enabled = true;
		}

		foreach ( var c in clothing )
		{
			c.ModelRenderer.Enabled = true;

			if ( c.Category is Clothing.ClothingCategory.Hair or Clothing.ClothingCategory.Facial or Clothing.ClothingCategory.Hat )
			{
				c.ModelRenderer.RenderType = IsProxy ? Sandbox.ModelRenderer.ShadowRenderType.On : Sandbox.ModelRenderer.ShadowRenderType.ShadowsOnly;
			}
		}
	}
	protected override void OnPreRender()
	{
		base.OnPreRender();

		if ( !Scene.IsValid() || !Camera.IsValid() )
			return;

		UpdateModelVisibility();

	}

}

