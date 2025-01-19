using UI.Event;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChooseCharacterUIHandler : MonoBehaviour
    {
        // [SerializeField] private Button[] _characterButtons;
        [SerializeField] private Button _submitCharacterButton;
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
                UIManager.Instance.UIDataSO.CharacterId = ButtonSelectionHandler.SelectedButton.GetInstanceID().ToString();
                EventAggregator.Instance?.RaiseEvent(new SubmitCharacterEvent());
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

