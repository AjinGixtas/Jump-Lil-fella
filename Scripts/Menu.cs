using Godot;

public partial class Menu : Node
{
	public void OnPlayButtonPressed() {
		GetTree().ChangeSceneToFile("res://Scenes/Levels/Test.tscn");
	}
	public void OnOptionsButtonPressed() {
		return;
	}
	public void OnQuitButtonPressed() {
		GetTree().Quit();
	}
}
