using System;
using TMPro;
using UI.Event;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SelectRoomUIHandler: MonoBehaviour
    {
        [SerializeField] private TMP_InputField _roomNameText;
        [SerializeField] private Button _submitButton;
        
        // public string RoomName => _roomNameText.text;

        private void OnEnable()
        {
            _submitButton.onClick.AddListener(SubmitButtonClicked);
        }

        private void OnDisable()
        {
            _submitButton.onClick.RemoveListener(SubmitButtonClicked);
        }

        private void SubmitButtonClicked()
        {
            if (string.Equals(_roomNameText.text, ""))
            {
                Debug.Log($"{_roomNameText.text} is empty");
                EventAggregator.Instance?.RaiseEvent(new OnPopupEvent()
                {
                    PopupMessage = "Please enter the name of the room",
                    PopupType = EPopupType.Error
                });
            }
            else
            {
                Debug.Log($"{_roomNameText.text}");

                UIManager.Instance.UIDataSO.RoomName = _roomNameText.text;
                EventAggregator.Instance.RaiseEvent(new OnUISubmitEvent()
                {
                    SourceObject = gameObject,
                    TargetObject = UIManager.Instance.SelectCharacterUI,
                    AnimationTransitionType = EAnimationType.Swipe
                });
            }
        }
        
        
    }
}