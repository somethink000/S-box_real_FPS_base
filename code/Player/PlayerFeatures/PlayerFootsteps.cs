using Sandbox;

namespace GeneralGame;


public sealed class PlayerFootsteps : Component
{
	[Property] private SkinnedModelRenderer ModelRenderer { get; set; }

	private TimeSince TimeSinceLastStep { get; set; }

	protected override void OnEnabled()
	{
		if ( !ModelRenderer.IsValid() )
			return;

		ModelRenderer.OnFootstepEvent += OnEvent;
	}

	protected override void OnDisabled()
	{
		if ( !ModelRenderer.IsValid() )
			return;

		ModelRenderer.OnFootstepEvent -= OnEvent;
	}

	private void OnEvent( SceneModel.FootstepEvent e )
	{
		if ( TimeSinceLastStep < 0.2f )
			return;

		var trace = Scene.Trace
			.Ray( e.Transform.Position + Vector3.Up * 20f, e.Transform.Position + Vector3.Up * -20f )
			.Run();

		if ( !trace.Hit )
			return;

		if ( trace.Surface is null )
			return;

		TimeSinceLastStep = 0f;

		var sound = e.FootId == 0 ? trace.Surface.Sounds.FootLeft : trace.Surface.Sounds.FootRight;
		if ( sound is null ) return;

		var handle = Sound.Play( sound, trace.HitPosition + trace.Normal * 5f );
		handle.Volume *= e.Volume;
	}
}
