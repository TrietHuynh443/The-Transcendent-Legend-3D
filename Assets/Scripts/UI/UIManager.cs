using System.Collections.Generic;
using DG.Tweening;
using UI.Event;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class UIManager : UnitySingleton<UIManager>
    {
        [SerializeField] private GameObject _selectCharacterUI ;
        [SerializeField] private GameObject _selectRoomUI;
        [SerializeField] private GameObject _popupCanvasObjectPrefabs;
        [SerializeField] private UIDataSO _uiDataSO;
        public UIDataSO UIDataSO => _uiDataSO;
        private Queue<GameObject> _popupPool = new Queue<GameObject>();

        private UnityAction _yesPopUpCallback;
        private UnityAction _noPopUpCallback;

        public GameObject SelectCharacterUI => _selectCharacterUI;
        public GameObject SelectRoomUI => _selectRoomUI;
        public void OnEnable()
        {
            EventAggregator.Instance?.AddEventListener<OnPopupEvent>(Popup);
            EventAggregator.Instance?.AddEventListener<OnUISubmitEvent>(UISubmit);
        }

        private void UISubmit(OnUISubmitEvent evt)
        {
            MakeAnimationTransition(evt.SourceObject, evt.TargetObject, evt.AnimationTransitionType);
        }

        private void MakeAnimationTransition(GameObject evtSourceObject, GameObject evtTargetObject,
            EAnimationType evtAnimationTransitionType)
        {
            var sourceRect = evtSourceObject.GetComponent<RectTransform>();
            var targetRect = evtTargetObject.GetComponent<RectTransform>();
            var screenWidth = Screen.width;
            switch (evtAnimationTransitionType)
            {
                case EAnimationType.Swipe:
                    // Swipe source to the left and target from the right
                    sourceRect.DOLocalMoveX(-screenWidth, 1f)
                        .SetEase(Ease.InOutQuad)
                        .OnComplete(() => evtSourceObject.SetActive(false));
                    
                    evtTargetObject.SetActive(true);
                    targetRect.DOLocalMoveX(0, 1f, true).SetEase(Ease.InOutQuad).From(screenWidth);
                    break;
                default:
                    evtSourceObject.SetActive(false);
                    evtTargetObject.SetActive(true);
                    break;
            }
        }

        public void OnDisable()
        {
            EventAggregator.Instance?.RemoveEventListener<OnPopupEvent>(Popup);
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