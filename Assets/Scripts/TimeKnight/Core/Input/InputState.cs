using System;

namespace TimeKnight.Core.Input
{
	public enum InputStatus
	{
		Enabled,
		Disabled,
	}
	
	[Flags]
	public enum InputMapState
	{
		None = 0,
		Gameplay = 1 << 0,
		Dialogue = 1 << 1,
		Every = -1
	}

	[Flags]
	public enum GameplayActions
	{
		None = 0,
		
		MoveHorizontal = 1 << 0,
		MoveJump       = 1 << 1,
		Move = MoveHorizontal | MoveJump,
		
		GrappleFire    = 1 << 2,
		GrappleStop    = 1 << 3,
		
		Every = -1
	}
	
	
}