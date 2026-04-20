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
		
		InteractionInteract = 1 << 4,
		InteractionNavigate = 1 << 5,
		
		Every = -1
	}

	[Flags]
	public enum DialogueActions
	{
		None = 0,
		Advance = 1 << 0
	}

	public struct InputState
	{
		public ActionMaps ActionMaps;
		public GameplayActions GameplayActions;
		public DialogueActions DialogueActions;
	}
}