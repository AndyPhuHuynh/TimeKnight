using UnityEngine;

namespace TimeKnight.Utils
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance { get; private set; } = null!;

		protected void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}
			
			Instance = (this as T)!;
			DontDestroyOnLoad(gameObject);
		}
	}
}