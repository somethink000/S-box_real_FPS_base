global using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TestInit
{
	[AssemblyInitialize]
	public static void ClassInitialize( TestContext context )
	{
		Rinten.Application.InitUnitTest();
	}
}
