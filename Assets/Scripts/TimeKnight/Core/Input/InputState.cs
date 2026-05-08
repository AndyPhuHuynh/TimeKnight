using System;

namespace TimeKnight.Core.Input
{
	public enum InputStatus
	{
		Enabled,
		Disabled,
	}
	
	[Flags]
	public enum ActionMaps
	{
		None = 0,
		
		Gameplay = 1 << 0,
		Dialogue = 1 << 1,
		Global   = 1 << 2,
		
		Every = -1
	}

	[Flags]
	public enum GameplayActions
	{
		None = 0,
		
		MoveHorizontal = 1 << 0,
		MoveJump       = 1 << 1,
		Move = MoveHorizontal | MoveJump,
		
		Attack = 1 << 2,
		
		GrappleFire    = 1 << 3,
		GrappleStop    = 1 << 4,
		
		InteractionInteract = 1 << 5,
		InteractionNavigate = 1 << 6,
		
		SlowTime = 1 << 7,

		Every = -1
	}

	[Flags]
	public enum DialogueActions
	{
		None = 0,
		Advance = 1 << 0
	}

	[Flags]
	public enum GlobalActions
	{
		None = 0,
		
		OpenPause  = 1 << 0,
		ClosePause = 1 << 1,
		
		Every = -1
	}

	public struct InputState
	{
		public ActionMaps ActionMaps;
		public GameplayActions GameplayActions;
		public DialogueActions DialogueActions;
		public GlobalActions GlobalActions;
	}
}