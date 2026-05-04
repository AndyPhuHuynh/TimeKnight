using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Pause
{
	public class PauseController : MonoBehaviour
	{
		[SerializeField] private InputReader input = null!;
		
		private void OnValidate()
		{
			Validation.NotNull(this, input, nameof(input));
		}
	}
}