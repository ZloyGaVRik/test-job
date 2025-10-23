using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Task2.Scripts
{
    public class ScrollManager : MonoBehaviour
    {
        [SerializeField] private ScrollItem _itemPrefab;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private RectTransform _contentTransform;

        private Dictionary<string, ScrollItem> _items = new Dictionary<string, ScrollItem>();

        private int itemId = 0;

        void Start()
        {
            if (_addButton != null) _addButton.onClick.AddListener(OnAddButtonClicked);
            if (_clearButton != null) _clearButton.onClick.AddListener(ClearItems);
        }

        private void OnAddButtonClicked()
        {
            if (_items != null)
            {
                AddItem(itemId);
                itemId++;
            }
        }

        private void RemoveItem(ScrollItem item)
        {
            if (item != null)
            {
                item.RemoveButton.onClick.RemoveAllListeners();
                Destroy(item.gameObject);
                _items.Remove(item.ItemKey);
            }
        }

        private void AddItem(int Id)
        {
            var item = Instantiate(_itemPrefab, _contentTransform);
            string itemKey = $"SD_{Id}";
            item.ItemKey = itemKey;

            if (item.RemoveButton != null) item.RemoveButton.onClick.AddListener(() => RemoveItem(item));

            _items.Add(itemKey, item);

        }

        private void ClearItems()
        {
            foreach (var item in _items.Values)
            {
                item.RemoveButton.onClick.RemoveAllListeners();
                Destroy(item.gameObject);
            }
            _items.Clear();
        }


        private void OnDestroy()
        {
            _addButton.onClick.RemoveListener(OnAddButtonClicked);
            _clearButton.onClick.RemoveListener(ClearItems);

        }

    }
}

