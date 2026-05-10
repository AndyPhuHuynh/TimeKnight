using TimeKnight.Core;
using TimeKnight.Core.Audio;
using TimeKnight.Core.Dialogue;
using TimeKnight.Core.HUD;
using TimeKnight.Core.Interaction;
using TimeKnight.Core.Pause;
using TimeKnight.Core.Player;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight
{
	public class LevelInitiator : MonoBehaviour
	{
		[SerializeField] private GameObject uiContainer = null!;

		[Header("Audio")]
		[SerializeField] private AudioClip levelAudio = null!;
		
		[Header("Prefabs")]
		[SerializeField] private GameObject player = null!;
		[SerializeField] private PlayerStatsDisplay playerStatsDisplay = null!;
		[SerializeField] private DialogueManager dialogueManager = null!;
		[SerializeField] private SelectorUI selectorUI = null!;
		[SerializeField] private PauseMenu pauseMenu = null!;
		
		// Camera should already exist in the level whereas everything above should not
		[SerializeField] private CameraScript cameraScript = null!;
		
		private void OnValidate()
		{
			Validation.NotNull(this, uiContainer, nameof(uiContainer));
			Validation.NotNull(this, levelAudio, nameof(levelAudio));
			Validation.NotNull(this, player, nameof(player));
			Validation.NotNull(this, playerStatsDisplay, nameof(playerStatsDisplay));
			Validation.NotNull(this, dialogueManager, nameof(dialogueManager));
			Validation.NotNull(this, selectorUI, nameof(selectorUI));
			Validation.NotNull(this, pauseMenu, nameof(pauseMenu));
			Validation.NotNull(this, cameraScript, nameof(cameraScript));
		}

		private void Awake()
		{
			InitializeLevel();
		}

		private void InitializeLevel()
		{
			player = Instantiate(player, Vector3.zero, Quaternion.identity);
			var playerManager = player.GetComponentInChildren<PlayerCombatManager>();
			
			playerStatsDisplay = Instantiate(playerStatsDisplay, uiContainer.transform);
			playerStatsDisplay.Initialize(playerManager);
			
			dialogueManager = Instantiate(dialogueManager, uiContainer.transform);
			selectorUI = Instantiate(selectorUI, uiContainer.transform);
			
			pauseMenu = Instantiate(pauseMenu, uiContainer.transform);
			pauseMenu.CloseMenuImmediate();
			
			cameraScript.Initialize(player);
			
			AudioManager.Instance.FadeInMusic(levelAudio);
		} 
	}
}