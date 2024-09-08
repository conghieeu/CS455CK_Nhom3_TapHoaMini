using UnityEngine;

namespace CuaHang.UI
{
    public class UIPanel : HieuBehavior
    {
        [Header("UI PANEL")]
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected RectTransform _panelContent;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void ShowContents(bool value)
        {
            if (_panelContent)
            {
                _panelContent.gameObject.SetActive(value);
            }
        }

        protected virtual void SetActiveCanvasGroup(bool isOn)
        {
            if (isOn)
            {
                _canvasGroup.alpha = 1;
            }
            else
            {
                _canvasGroup.alpha = 0;
            }

            _canvasGroup.interactable = isOn;
            _canvasGroup.blocksRaycasts = isOn;
        }
    }
}