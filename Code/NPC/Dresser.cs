using Sandbox;

namespace GeneralGame;

public enum DressGender
{
	Mal,
	Fem,
	All
}

public struct ClothStruct
{
	public ClothStruct( ClothingContainer.ClothingEntry cloth, DressGender gender )
	{
		Cloth = cloth;
		Gender = gender;
	}

	public ClothingContainer.ClothingEntry Cloth { get; set; }
	public DressGender Gender { get; set; }
}

[GameResource( "GroupedCloth", "grpclths", "Clothes" )]
public partial class GroupedCloth : GameResource
{
	[Property] public List<ClothStruct> Jackets { get; set; }
	[Property] public List<ClothStruct> Shirts { get; set; }
	[Property] public List<ClothStruct> Trousers { get; set; }
	[Property] public List<ClothStruct> Shoes { get; set; }
}

public sealed class Dresser : Component, Component.ExecuteInEditor
{
	[Property]
	public SkinnedModelRenderer BodyTarget { get; set; }

	[Property]
	public bool ApplyLocalUserClothes { get; set; } = true;

	[Property]
	public bool ApplyHeightScale { get; set; } = true;

	[Property]
	public List<ClothingContainer.ClothingEntry> Clothing { get; set; }

	//protected override void OnAwake()
	//{
	//	if ( IsProxy )
	//		return;

	//	Apply();
	//}

	public void Apply()
	{
		if ( !BodyTarget.IsValid() )
			return;

		var clothing = ApplyLocalUserClothes ? ClothingContainer.CreateFromLocalUser() : new ClothingContainer();

		if ( !ApplyHeightScale )
			clothing.Height = 1;

		clothing.AddRange( Clothing );
		clothing.Normalize();

		clothing.Apply( BodyTarget );

		BodyTarget.PostAnimationUpdate();
	}

	protected override void OnValidate()
	{
		if ( IsProxy )
			return;

		base.OnValidate();

		using var p = Scene.Push();

		if ( !BodyTarget.IsValid() )
		{
			BodyTarget = GetComponentInChildren<SkinnedModelRenderer>();
		}

		Apply();
	}
}
