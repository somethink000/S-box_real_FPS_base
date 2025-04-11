using Editor;
using Editor.Inspectors;
using Sandbox;
using System;
using System.Drawing;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace IconsEditor;


[Dock( "Editor", "IconEditor", "local_fire_department" )]
public class IconEditor : GraphicsView
{
	public const int RENDER_RESOLUTION = 128;

	private SceneObject _obj;
	private Model _model;
	private LineEdit _material;
	private Color _color = Color.White;
	private Angles _angles;
	private Vector3 _position;
	private SceneCamera _camera;
	private SceneLight _light;

	public IconEditor( Widget parent ) : base( parent )
	{

		//// Scene
		var world = new SceneWorld();
		_camera = new SceneCamera()
		{
			World = world,
			AmbientLightColor = Color.White,
			AntiAliasing = false,
			BackgroundColor = Color.Transparent,
			FieldOfView = 40,
			ZFar = 5000,
			ZNear = 2
		};

		_light = new SceneLight( world, Vector3.Forward * 15f, 1000f, Color.White * 0.7f );
		_ = new SceneDirectionalLight( world, global::Rotation.From( 45, -45, 45 ), Color.White * 10f );


		//// Layout
		Layout = Layout.Column();
		
		Layout.Margin = 4;
		Layout.Spacing = 4;
		
		
		
		// Apply some CSS styling
		SetStyles( "background-color: #303445; color: white; font-weight: 600;" );
		{

			// Properties
			var angles = Layout.Add( new AnglesControlWidget( this.GetSerialized().GetProperty( nameof( _angles ) ) ) , 0 );

			Layout.AddSpacingCell( 4 );

			var position = Layout.Add( new VectorControlWidget( this.GetSerialized().GetProperty( nameof( _position ) ) ), 0 );

			Layout.AddSpacingCell( 4 );
			
			var model = Layout.Add( new ResourceControlWidget( this.GetSerialized().GetProperty( nameof( _model ) ) ), 0 );

			var color = Layout.Add( new ColorControlWidget( this.GetSerialized().GetProperty( nameof( _color ) ) ), 0 );

			_material = Layout.Add( new LineEdit(), 1 );
			_material.PlaceholderText = "Material group...";
			_material.TextEdited += ( text ) =>
			{
				_obj.SetMaterialGroup( text );
				
			};

			
		}

		Layout.AddSpacingCell( 4 );
		{
			// Scene
			var renderer = Layout.Add( new NativeRenderingWidget( this )
			{
				Camera = _camera,
				TranslucentBackground = true,

			}, 1 );
		}

		Layout.AddSpacingCell( 4 );
		{
			// Save Button
			var button = Layout.Add( new global::Editor.Button( this )
			{
				Text = "Save Icon",
				Clicked = () =>
				{

					var pixmap = new Pixmap( RENDER_RESOLUTION, RENDER_RESOLUTION );
					var path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/assets/ui/icons/{_model.ResourceName}I.png";
					_camera.RenderToPixmap( pixmap );
					pixmap.SavePng( path );
					Log.Info( path );
				}
			}, 1 );
		}

		// Object
		var mdl = _model;
		_obj = new SceneObject(
			world,
			(mdl?.IsError ?? true)
				? Model.Load( "models/dev/box.vmdl" )
				: mdl
		);
		_obj.SetMaterialGroup( _material.Value );

	}

	//Vector2 lastPos;
	//float spinVelocity;

	//protected override void OnMouseMove( MouseEvent e )
	//{
	//	base.OnMouseMove( e );

	//	Update();

	//	var delta = e.LocalPosition - lastPos;
	//	lastPos = e.LocalPosition;

	//	if ( (e.ButtonState & MouseButtons.Left) != 0 )
	//	{
	//		spinVelocity += delta.x * 0.1f;
	//	}
	//}

	protected override void Signal( WidgetSignal signal )
	{
		base.Signal( signal );

		if ( _model != null )
		{
			var mdl = _model;
			_obj.Model = mdl?.ResourcePath == "models/dev/error.vmdl"
				? Model.Load( "models/dev/box.vmdl" )
				: mdl;
		}

	}

	[EditorEvent.Frame]
	private void Frame()
	{
		if ( _obj == null )
			return;

		_camera.FitModel( _obj );
		_light.Position = _camera.Position + _camera.Rotation.Backward * 20f;
		_obj.Position = _position;
		_obj.Rotation = _angles;
		_obj.ColorTint = _color;
	}
}


