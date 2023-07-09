//DELETEME
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Template : Panel
{
	
	public Label Temp;

	public Template()
	{
		Temp = Add.Label( "value" );
		
	}

	public override void Tick()
	{
		

		Temp.Text = "link on github reposytory on asset.party project page";
		
	}
}
//DELETEME
