using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Arena;

[Hide]
public sealed class ClothingComponent : Component
{
	[Property] public Clothing.ClothingCategory Category { get; set; }
	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }

	protected override void OnAwake()
	{
		ModelRenderer = Components.Get<SkinnedModelRenderer>( true );
		base.OnAwake();
	}
}
