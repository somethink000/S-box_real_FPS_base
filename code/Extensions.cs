using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GeneralGame;
using Sandbox;
using Sandbox.Internal;

namespace GeneralGame;

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

		}

		particles.Delete();
	}



	public static void ApplyWithComponent( this ClothingContainer self, SkinnedModelRenderer body )
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		self.Reset( body );
		SkinnedModelRenderer skinnedModelRenderer = null;
		Material material = (from x in self.Clothing?.Select( ( ClothingContainer.ClothingEntry x ) => x?.Clothing.SkinMaterial )
							 where !string.IsNullOrWhiteSpace( x )
							 select Material.Load( x )).FirstOrDefault();
		Material material2 = (from x in self.Clothing?.Select( ( ClothingContainer.ClothingEntry x ) => x?.Clothing.EyesMaterial )
							  where !string.IsNullOrWhiteSpace( x )
							  select Material.Load( x )).FirstOrDefault();
		if ( material != null )
		{
			body.SetMaterialOverride( material, "skin" );
		}

		if ( material2 != null )
		{
			body.SetMaterialOverride( material2, "eyes" );
		}

		foreach ( ClothingContainer.ClothingEntry item in self.Clothing )
		{
			Clothing clothing = item.Clothing;
			string model = clothing.GetModel( self.Clothing.Select( ( ClothingContainer.ClothingEntry x ) => x.Clothing ).Except( new Clothing[1] { clothing } ) );
			if ( !string.IsNullOrEmpty( model ) && string.IsNullOrEmpty( clothing.SkinMaterial ) && !(Model.Load( model )?.IsError ?? true) )
			{
				GameObject gameObject = new GameObject( enabled: false, "Clothing - " + clothing.ResourceName );
				RuntimeHelpers.EnsureSufficientExecutionStack();
				gameObject.Parent = body.GameObject;
				RuntimeHelpers.EnsureSufficientExecutionStack();
				var component = gameObject.Components.Create<ClothingComponent>();
				component.Category = clothing.Category;
				gameObject.Tags.Add( "clothing" );
				SkinnedModelRenderer skinnedModelRenderer2 = gameObject.Components.Create<SkinnedModelRenderer>();
				RuntimeHelpers.EnsureSufficientExecutionStack();
				skinnedModelRenderer2.Model = Model.Load( clothing.Model );
				RuntimeHelpers.EnsureSufficientExecutionStack();
				skinnedModelRenderer2.BoneMergeTarget = body;
				if ( material != null )
				{
					skinnedModelRenderer2.SetMaterialOverride( material, "skin" );
				}

				if ( material2 != null )
				{
					skinnedModelRenderer2.SetMaterialOverride( material2, "eyes" );
				}

				if ( !string.IsNullOrEmpty( clothing.MaterialGroup ) )
				{
					skinnedModelRenderer2.MaterialGroup = clothing.MaterialGroup;
				}

				if ( clothing.Category == Clothing.ClothingCategory.Skin )
				{
					RuntimeHelpers.EnsureSufficientExecutionStack();
					skinnedModelRenderer = skinnedModelRenderer2;
				}

				if ( clothing.AllowTintSelect )
				{
					RuntimeHelpers.EnsureSufficientExecutionStack();
					skinnedModelRenderer2.Tint = clothing.TintSelection.Evaluate( item.Tint?.Clamp( 0f, 1f ) ?? clothing.TintDefault );
				}

				RuntimeHelpers.EnsureSufficientExecutionStack();
				gameObject.Enabled = true;
			}
		}

		foreach ( var (name, value) in self.GetBodyGroups() )
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			body.SetBodyGroup( name, value );
		}
	}

}
