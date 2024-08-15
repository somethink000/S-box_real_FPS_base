
using Sandbox;
using System;
using System.Diagnostics;
using System.Numerics;
namespace GeneralGame;


public class BaseThrow : WeaponComponent
{
	[Property] public float prepareTime { get; set; }
	[Property] public float releaseTime { get; set; }
	[Property] public SoundEvent activateSound { get; set; }
	[Property] public SoundEvent throwSound { get; set; }
	[Property] public GameObject throwPrefab { get; set; }

	public TimeUntil curPrepareTime { get; set; }
	public TimeUntil curReleaseTime { get; set; }
	public bool isPreparing { get; set; } = false;
	public bool waitingThrow { get; set; } = false;

	public override void primaryAction()
	{
		startPrepare();
	}
	public override void primaryActionRelease()
	{
		
		activateThrow();
	}
	public override void seccondaryAction()
	{
		
	}


	public virtual void startPrepare()
	{
		if ( isPreparing ) return;

		
		isPreparing = true;
		curPrepareTime = prepareTime;
		curReleaseTime = releaseTime;

		Sound.Play( activateSound, Transform.Position );
		EffectRenderer.Set( "b_prepare", true );

	}
	public virtual void activateThrow()
	{
		if ( !isPreparing || curPrepareTime ) return;
		waitingThrow = true;
	}

	public virtual void createThrow(bool imidiantly)
	{
		var obj = throwPrefab.Clone( this.Transform.World );
		obj.NetworkSpawn();
		obj.Transform.Position = owner.Camera.Transform.Position + owner.Camera.Transform.Rotation.Forward * 50;
		obj.Transform.Rotation = owner.Camera.Transform.Rotation;
		obj.Components.Get<Rigidbody>().Velocity = owner.Camera.Transform.Rotation.Forward * 1000;
		obj.Components.Get<EntThrow>().explodeTime = imidiantly ? 0f : curPrepareTime;
	}

	

	protected override void OnUpdate()
	{
		if ( curPrepareTime && isPreparing )
		{
			isPreparing = false;
			createThrow( true );
		}
	

		if ( waitingThrow && curReleaseTime )
		{
			isPreparing = false;
			waitingThrow = false;
			Sound.Play( throwSound, Transform.Position );
			EffectRenderer.Set( "b_throw", true );

			createThrow( false );
		}
		

		base.OnUpdate();
	}
}
