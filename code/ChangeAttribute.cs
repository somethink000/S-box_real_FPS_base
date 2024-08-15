using System;
using System.Linq;
using Sandbox;

namespace GeneralGame;

[CodeGenerator( CodeGeneratorFlags.Instance | CodeGeneratorFlags.WrapPropertySet, "Facepunch.Arena.ChangeAttribute.OnPropertySet" )]
public class ChangeAttribute : Attribute
{
	public string Callback { get; set; }

	public ChangeAttribute( string callbackName )
	{
		Callback = callbackName;
	}
	
	internal static void OnPropertySet<T>( WrappedPropertySet<T> p )
	{
		var attribute = p.Attributes.OfType<ChangeAttribute>().FirstOrDefault();
		var type = GlobalGameNamespace.TypeLibrary.GetType( p.TypeName );
		var method = type.GetMethod( attribute.Callback );
		var property = GlobalGameNamespace.TypeLibrary.GetMemberByIdent( p.MemberIdent ) as PropertyDescription;
		
		var oldValue = property.GetValue( p.Object );
		
		p.Setter( p.Value );

		try
		{
			method.Invoke( p.Object, new[] { oldValue, p.Value } );
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}
}
