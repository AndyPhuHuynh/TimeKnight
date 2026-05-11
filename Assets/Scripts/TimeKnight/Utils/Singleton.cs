using System;
using UnityEngine;

namespace TimeKnight.Utils
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T? _instance;
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					throw new InvalidOperationException(
						"Attempting to access singleton that has not yet been initialized. " +
					    "Do you have a copy of the MonoBehavior in your scene?");
				}
				return _instance;
			}
		}

		protected virtual void Awake()
		{
			var self = this as T;
			
			if (_instance != null && _instance != self)
			{
				Destroy(gameObject);
				return;
			}
			
			_instance = self;
			DontDestroyOnLoad(gameObject);
		}
	}
}