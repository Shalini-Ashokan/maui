namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue34898 : _IssuesUITest
{
	public override string Issue => "Shell.Items.Clear does not disconnect handlers correctly";

	public Issue34898(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellItemsClearDisconnectsHandlers()
	{
		App.WaitForElement("ClearAndNavigateButton");
		App.Tap("ClearAndNavigateButton");

		App.WaitForElement("CheckHandlersButton");
		App.Tap("CheckHandlersButton");

		var pageStatus = App.WaitForElement("PageHandlerStatus").GetText();
		var labelStatus = App.WaitForElement("LabelHandlerStatus").GetText();
		var entryStatus = App.WaitForElement("EntryHandlerStatus").GetText();
		var buttonStatus = App.WaitForElement("ButtonHandlerStatus").GetText();

		Assert.That(pageStatus, Is.EqualTo("Disconnected"), "Page handler should be disconnected after Shell.Items.Clear()");
		Assert.That(labelStatus, Is.EqualTo("Disconnected"), "Label handler should be disconnected after Shell.Items.Clear()");
		Assert.That(entryStatus, Is.EqualTo("Disconnected"), "Entry handler should be disconnected after Shell.Items.Clear()");
		Assert.That(buttonStatus, Is.EqualTo("Disconnected"), "Button handler should be disconnected after Shell.Items.Clear()");
	}
}
