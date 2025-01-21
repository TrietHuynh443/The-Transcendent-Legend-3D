using UI.Event;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChooseCharacterUIHandler : MonoBehaviour
    {
        // [SerializeField] private Button[] _characterButtons;
        [SerializeField] private Button _submitCharacterButton;
        [SerializeField] private UIDataSO _uiDataSO;
        // Start is called before the first frame update
        private void OnEnable()
        {
            _submitCharacterButton.onClick.AddListener(OnSubmit);
        }

        private void OnDisable()
        {
            _submitCharacterButton.onClick.RemoveListener(OnSubmit);
        }

        private void OnSubmit()
        {
            if (ButtonSelectionHandler.SelectedButton != null)
            {
                UIManager.Instance.UIDataSO.CharacterId = ButtonSelectionHandler.SelectedButton.transform.GetSiblingIndex();
                _uiDataSO.CharacterId = ButtonSelectionHandler.SelectedButton.transform.GetSiblingIndex();
                EventAggregator.Instance?.RaiseEvent(new SubmitCharacterEvent()
                {
                    CharacterId = _uiDataSO.CharacterId,
                    RoomName = _uiDataSO.RoomName
                });
                EventAggregator.Instance?.RaiseEvent(new OnUISubmitEvent()
                {
                    SourceObject = gameObject,
                    TargetObject = UIManager.Instance.RoomInfoUI,
                    AnimationTransitionType = EAnimationType.Swipe
                });
                Debug.Log($"Network: Connecting {ButtonSelectionHandler.SelectedButton.transform.GetSiblingIndex()} {_uiDataSO.RoomName}");
            }
            else
            {
                Debug.Log("You need to select a character");
                //Popup error
                EventAggregator.Instance?.RaiseEvent(new OnPopupEvent()
                {
                    PopupMessage = "You need to select a character",
                    PopupType = EPopupType.Error
                });
            }
        }
    }
}

