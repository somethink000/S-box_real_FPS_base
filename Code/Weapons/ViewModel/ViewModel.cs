using Sandbox;
using System;
using System.Numerics;

namespace GeneralGame;


public sealed class ViewModel : Component
{
	public ModelRenderer ViewModelRenderer { get; set; }
	public SkinnedModelRenderer ViewModelHandsRenderer { get; set; }
	
	private Rotation CurRotation { get; set; }
	private Vector3 CurPos { get; set; }

	public Carriable Carriable { get; set; }
	private Player ply => Carriable.Owner;
	public CameraComponent Camera { get; set; }
	public bool ShouldDraw { get; set; }


	public void OnHolster()
	{

		Destroy();

	}

	protected override void OnUpdate()
	{

		if ( ply == null ) return;

		var renderType = ShouldDraw ? ModelRenderer.ShadowRenderType.Off : ModelRenderer.ShadowRenderType.ShadowsOnly;
		ViewModelRenderer.Enabled = ply.CameraController.IsFirstPerson;
		ViewModelRenderer.RenderType = renderType;

		if ( ViewModelHandsRenderer is not null )
		{
			ViewModelHandsRenderer.Enabled = ply.CameraController.IsFirstPerson;
			ViewModelHandsRenderer.RenderType = renderType;
		}

		if ( !ply.CameraController.IsFirstPerson ) return;


	
		
		WorldPosition = Camera.WorldPosition;
		WorldRotation = Camera.WorldRotation;

	}

	
}
