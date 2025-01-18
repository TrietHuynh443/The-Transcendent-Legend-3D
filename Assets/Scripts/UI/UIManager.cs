using System;
using UI.Event;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _popupCanvasObject;
        Action<OnPopupEvent> _onPopupAction;
        public void OnEnable()
        {
            EventAggregator.Instance.AddEventListener<OnPopupEvent>(_onPopupAction);
        }

        public void OnDisable()
        {
            EventAggregator.Instance.RemoveEventListener<OnPopupEvent>(_onPopupAction);
        }
    }
}