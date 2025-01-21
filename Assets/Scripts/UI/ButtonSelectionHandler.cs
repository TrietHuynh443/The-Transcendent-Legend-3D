using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonSelectionHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public static GameObject SelectedButton = null;
        // private Button _button;

        private void Start()
        {
            // _button = GetComponent<Button>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            SelectedButton = eventData.selectedObject;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            StartCoroutine(EnsureSelection());
        }

        private IEnumerator EnsureSelection()
        {
            yield return new WaitForEndOfFrame();
            // Check if the new selected object is invalid or null
            if (EventSystem.current.currentSelectedGameObject == null || 
                EventSystem.current.currentSelectedGameObject.GetComponent<ButtonSelectionHandler>() == null)
            {
                // Reselect the button using the EventSystem
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
    }
}