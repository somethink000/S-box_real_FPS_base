using System;
using System.Threading.Tasks;

namespace GeneralGame;

public static class ParticleExtensions
{
	public static async void PlayUntilFinished( this SceneParticles particles, TaskSource source, Action<SceneParticles> OnFrame = null )
	{
		try
		{
			while ( particles.IsValid() && !particles.Finished )
			{
				await source.Frame();

				if ( OnFrame is not null )
					OnFrame( particles );

				particles?.Simulate( Time.Delta );
			}
		}
		catch ( TaskCanceledException )
		{
			// Do nothing.
		}

		particles.Delete();
	}
}
