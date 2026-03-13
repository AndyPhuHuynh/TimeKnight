using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace TimeKnight.Interaction
{
    public class SelectorButtonPool
    {
        private const int StartSize = 5;
        private const int MaxSize = 5;

        private readonly MonoBehaviour _parent;
        private readonly Button _buttonPrefab;
        private readonly GameObject _buttonContainer;
        private readonly GameObject _buttonPoolContainer;
        public IObjectPool<SelectorButton> Pool { get; }

        public SelectorButtonPool(
            Button buttonPrefab,
            GameObject buttonContainer,
            GameObject buttonPoolContainer
        ) 
        {   
            _buttonPrefab = buttonPrefab;
            _buttonContainer = buttonContainer;
            _buttonPoolContainer = buttonPoolContainer;

            Pool = new ObjectPool<SelectorButton>(
                createFunc: CreateFunc,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroy,
                defaultCapacity: StartSize,
                maxSize: MaxSize
            );
            Prewarm();
        }
        
        private SelectorButton CreateFunc()
        {
            var button = Object.Instantiate(_buttonPrefab, _buttonPoolContainer.transform);
            var selectorButton = new SelectorButton(button);
            selectorButton.Hide();
            return selectorButton;
        }

        private void OnGet(SelectorButton button)
        {
            button.Button.transform.SetParent(_buttonContainer.transform);
            button.Button.transform.SetAsLastSibling();
        }

        private void OnRelease(SelectorButton button)
        {
            button.Hide();
            button.Button.transform.SetParent(_buttonPoolContainer.transform);
            button.Button.onClick.RemoveAllListeners();
        }

        private static void OnDestroy(SelectorButton button)
        {
            Object.Destroy(button.Button.gameObject);
        }

        
        private void Prewarm()
        {
            var buttons = new List<SelectorButton>();
            for (int i = 0; i < StartSize; i++)
            {
                buttons.Add(Pool.Get());
            }

            foreach (var button in buttons)
            {
                Pool.Release(button);
            }
        }
    }
}