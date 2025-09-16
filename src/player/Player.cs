using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] public float FlySpeed = 20f;
	[Export] public float MouseSensitivity = 0.1f;

	private Camera3D camera;

	private float yaw = 0f;
	private float pitch = 0f;

	private Vector3 cameraOffset = new(0, 5, -10); // height=2, behind=4
	
	public override void _EnterTree()
	{
		GD.Print("Player entered tree");
	}

	public override void _Ready()
	{
		camera = GetNode<Camera3D>("Camera3D");
		GD.Print("Camera Initialized: " + camera.Name);

		camera.MakeCurrent();
		camera.Current = true; // ensure camera is active
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	/// <summary>
	/// Handles all unhandled input events for the player.
	/// 
	/// 1. Mouse movement (InputEventMouseMotion):
	///    - Adjusts the player's yaw (horizontal rotation) based on X-axis mouse movement.
	///    - Adjusts the camera's pitch (vertical rotation) based on Y-axis mouse movement.
	///    - Clamps the pitch to prevent the camera from flipping over when looking straight up or down.
	/// 
	/// 2. Game close action (InputEvent):
	///    - Toggles mouse visibility when pressing the "game_close" action.
	///    - Quits the game if the mouse is already visible.
	/// 
	/// Notes:
	/// - The camera is a child of the player; yaw rotates the parent, pitch rotates the camera locally.
	/// - Uses manual degree-to-radian conversion because Mathf.Deg2Rad is not available in C#.
	/// - This method allows the player to freely look around even before moving.
	/// </summary>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseEvent)
		{
			yaw -= mouseEvent.Relative.X * MouseSensitivity; // rotate cube
			pitch -= mouseEvent.Relative.Y * MouseSensitivity; // rotate camera
			pitch = Mathf.Clamp(pitch, -30f, 60f); // limit vertical orbit
		}

		if (@event.IsActionPressed("game_close"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible; // Show cursor
			}
			else
			{
				GetTree().Quit(); // Quit game
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 direction = Vector3.Zero;

		if (Input.IsActionPressed("click_move"))
		{
			Vector3 camPos = camera.GlobalPosition;
			Vector3 dragonPos = GlobalTransform.Origin;

    		Vector3 forward = -GlobalTransform.Basis.Z;
    		Velocity = forward.Normalized() * FlySpeed;

			// Calculate direction from camera to dragon
			Vector3 lookDir = (dragonPos - camPos).Normalized();
			// Look in the direction opposite to the camera
			LookAt(dragonPos + lookDir, Vector3.Up);
		}
		else
		{
			Velocity = Vector3.Zero;
		}

		MoveAndSlide();

		// Rotate offset by yaw and pitch
		Vector3 rotatedOffset = cameraOffset;

		// Horizontal rotation (yaw)
		rotatedOffset = rotatedOffset.Rotated(Vector3.Up, yaw * (Mathf.Pi / 180f));

		// Vertical rotation (pitch)
		Vector3 right = Vector3.Up.Cross(rotatedOffset).Normalized();
		rotatedOffset = rotatedOffset.Rotated(right, pitch * (Mathf.Pi / 180f));

		// Set camera position
		camera.GlobalPosition = GlobalTransform.Origin + rotatedOffset;

		// Make camera look at cube center
		Vector3 dragonCentre = GlobalTransform.Origin + new Vector3(0, 0.5f, 0);
		camera.LookAt(dragonCentre, Vector3.Up);
	}
}
