using Sandbox;
using Sandbox.UI;

[Library]
public partial class SandboxHud : HudEntity<RootPanel>
{
	public SandboxHud()
	{
		if ( !Game.IsClient )
			return;

		RootPanel.StyleSheet.Load( "/Styles/sandbox.scss" );

		RootPanel.AddChild<Chat>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<VoiceSpeaker>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
		RootPanel.AddChild<Health>();
		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<Crosshair>();
		RootPanel.AddChild<Ammo>();


		//DELETE ME
		RootPanel.AddChild<Template>();
		//DELETE ME
	}
}
