using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupUIHandler : MonoBehaviour
{
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    private UnityAction _yesActionCb;
    private UnityAction _noActionCb;

    public void Popup(string title = "", string desc = "", UnityAction yesActionCb = null, UnityAction noActionCb = null)
    {
        _titleText.text = title;
        _descriptionText.text = desc;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        _yesActionCb += UIManager.Instance.YesActionCb;
        _noActionCb += UIManager.Instance.NoActionCb;
        _yesActionCb += Close;
        _noActionCb += Close;
        _yesButton.onClick.AddListener(_yesActionCb);
        _noButton.onClick.AddListener(_noActionCb);
    }
    
    private void OnDisable()
    {
        _yesButton.onClick.RemoveListener(_yesActionCb);
        _noButton.onClick.RemoveListener(_noActionCb);
    }
}
