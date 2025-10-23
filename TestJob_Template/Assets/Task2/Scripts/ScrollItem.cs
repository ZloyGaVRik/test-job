using UnityEngine;
using UnityEngine.UI;

namespace Task2.Scripts
{
    public class ScrollItem : MonoBehaviour
    {

        [SerializeField] private TMPro.TextMeshProUGUI _itemNameText;
        [SerializeField] private Button _clickButton;
        [SerializeField] private Button _removeButton;

        public string ItemKey { get; set; }

        private int _clickCount = 0;
        public Button RemoveButton => _removeButton;

        private void Start()
        {
            UpdateText();
            if (_clickButton != null) _clickButton.onClick.AddListener(ClickButton);
        }

        private void ClickButton()
        {
            _clickCount++;
            UpdateText();
        }


        private void UpdateText()
        {
            if (_clickCount > 1)
            {
                _itemNameText.text = $"- Item - {_clickCount} clicks";
            }
            else
            {
                _itemNameText.text = $"- Item - {_clickCount} click";
            }

        }

        private void OnDestroy()
        {
            _clickButton.onClick.RemoveListener(ClickButton);
        }


                
    }
}
