using System;
using System.Collections.Generic;
using UI.Event;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class UIManager : UnitySingleton<UIManager>
    {
        [SerializeField] private GameObject _popupCanvasObjectPrefabs;
        
        private Queue<GameObject> _popupPool = new Queue<GameObject>();

        private UnityAction _yesPopUpCallback;
        private UnityAction _noPopUpCallback;

        public void OnEnable()
        {
            EventAggregator.Instance.AddEventListener<OnPopupEvent>(Popup);
        }

        public void OnDisable()
        {
            EventAggregator.Instance.RemoveEventListener<OnPopupEvent>(Popup);
        }
        private void Popup(OnPopupEvent evt)
        {
            if (_popupPool.Count == 0)
            {
                CreatePopup();
            }
            var popup = _popupPool.Dequeue();
            var popupHandler = popup.GetComponent<PopupUIHandler>();
            popupHandler.Popup(
                evt.PopupType.ToString(),
                evt.PopupMessage
            );
                        
        }

        public void YesActionCb()
        {
            Debug.Log("Assign yes action cb");
        }

        public void NoActionCb()
        {
            Debug.Log("Assign no yes action cb");
        }

        private void CreatePopup()
        {
            var popup = Instantiate(_popupCanvasObjectPrefabs);
            _popupPool.Enqueue(popup);
        }
        
    }
}