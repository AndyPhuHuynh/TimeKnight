using System.Collections;
using UnityEngine;

namespace TimeKnight.Utils
{
	public class CoWrapper
	{
		private readonly MonoBehaviour _owner;
		private Coroutine? _routine;

		public bool IsRunning => _routine != null;
		
		public CoWrapper(MonoBehaviour owner)
		{
			_owner = owner;
		}

		private IEnumerator Wrap(IEnumerator routine)
		{
			yield return routine;
			_routine = null;
		}
		
		public void Start(IEnumerator routine)
		{
			if (IsRunning) Stop();
			_routine = _owner.StartCoroutine(Wrap(routine));
		}

		public void Stop()
		{
			if (!IsRunning) return;  
			_owner.StopCoroutine(_routine);
			_routine = null;
		}
	}
}