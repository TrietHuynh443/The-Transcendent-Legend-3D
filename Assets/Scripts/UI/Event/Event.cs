using UnityEngine;

namespace UI.Event
{
    public class IEvent
    {
        
    }

    public class SubmitCharacterEvent : IEvent
    {
        public string CharacterName { get; set; } = string.Empty;
    }

    public enum EPopupType
    {
        Notice = 0,
        Error = 1,
        Warning = 2,
    }
    
    public class OnPopupEvent : IEvent
    {
        public EPopupType PopupType { get; set; }
        public string PopupMessage { get; set; } = string.Empty;
    }

    public enum EAnimationType
    {
        None = 0,
        Fade = 1,
        Swipe = 2,
    }
    public class OnUISubmitEvent : IEvent
    {
        public GameObject SourceObject { get; set; }
        public GameObject TargetObject { get; set; }
        
        public EAnimationType AnimationTransitionType { get; set; } = EAnimationType.None;
    }
}