using System;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.Arena;

public static class Extensions
{
	public static async void PlayUntilFinished( this SceneParticles particles, TaskSource source )
	{
		try
		{
			while ( !particles.Finished )
			{
				await source.Frame();
				particles.Simulate( Time.Delta );
			}
		}
		catch ( TaskCanceledException )
		{
			// Do nothing.
		}

		particles.Delete();
	}
	
	public static void ApplyWithComponent( this ClothingContainer self, SkinnedModelRenderer body )
	{
		self.Reset( body );
		
		var skinMaterial = self.Clothing?.Select( x => x?.SkinMaterial ).Where( x => !string.IsNullOrWhiteSpace( x ) ).Select( Material.Load ).FirstOrDefault();
		var eyesMaterial = self.Clothing?.Select( x => x?.EyesMaterial ).Where( x => !string.IsNullOrWhiteSpace( x ) ).Select( Material.Load ).FirstOrDefault();

		if ( skinMaterial is not null ) body.SetMaterialOverride( skinMaterial, "skin" );
		if ( eyesMaterial is not null ) body.SetMaterialOverride( eyesMaterial, "eyes" );
		
		foreach ( var c in self.Clothing )
		{
			var modelPath = c.GetModel( self.Clothing.Except( new[] { c } ) );

			if ( string.IsNullOrEmpty( modelPath ) || !string.IsNullOrEmpty( c.SkinMaterial ) )
				continue;

			var model = Model.Load( modelPath );
			if ( model is null || model.IsError )
				continue;

			var go = new GameObject( false, $"Clothing - {c.ResourceName}" )
			{
				Parent = body.GameObject
			};

			var component = go.Components.Create<ClothingComponent>();
			component.Category = c.Category;
			
			go.Tags.Add( "clothing" );
			
			var r = go.Components.Create<SkinnedModelRenderer>();
			r.Model = Model.Load( c.Model );
			r.BoneMergeTarget = body;

			if ( skinMaterial is not null ) r.SetMaterialOverride( skinMaterial, "skin" );
			if ( eyesMaterial is not null ) r.SetMaterialOverride( eyesMaterial, "eyes" );

			if ( !string.IsNullOrEmpty( c.MaterialGroup ) )
				r.MaterialGroup = c.MaterialGroup;
			
			go.Enabled = true;
		}
		
		foreach ( var (name, value) in self.GetBodyGroups() )
		{
			body.SetBodyGroup( name, value );
		}
	}
}
