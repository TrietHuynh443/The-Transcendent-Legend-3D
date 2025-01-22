using DG.Tweening;
using Network;
using UI.Event;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class GameManager : UnitySingleton<GameManager>
    {
        [SerializeField] GameObject _fadeCanvasPrefab;
        [SerializeField] GameObject _roomManagerPrefab;
        private CanvasGroup _fadeCanvasGroup;
        private RoomManager _roomManager;
        private PhotonRaiseEventHandler _photonRaiseEventHandle;
        private PhotonEventConsumer _photonEventConsumer;
        private EventAggregator _eventAggregator;
        public int PlayerCount = 0;
        public RoomManager RoomManager => _roomManager;
        public bool IsMaster { get; set; } = false;

        protected override void SingletonStarted()
        {
            base.SingletonStarted();
            var fadeCanvas = Instantiate(_fadeCanvasPrefab);
            _fadeCanvasGroup = fadeCanvas.GetComponent<CanvasGroup>();
            fadeCanvas.SetActive(false);
            _roomManager = Instantiate(_roomManagerPrefab).GetComponent<RoomManager>();
            _photonRaiseEventHandle = PhotonRaiseEventHandler.Instance;
            _photonEventConsumer = PhotonEventConsumer.Instance;
            _eventAggregator = EventAggregator.Instance;
            DontDestroyOnLoad(_fadeCanvasGroup.gameObject);
            DontDestroyOnLoad(_photonEventConsumer.gameObject);
            DontDestroyOnLoad(_photonRaiseEventHandle);
            DontDestroyOnLoad(_eventAggregator.gameObject);
            DontDestroyOnLoad(gameObject);
        }
        
        public void LoadWinningScene()
        {
            _fadeCanvasGroup.DOFade(1, 1.0f).OnComplete(() =>
            {
                // Load the scene asynchronously
                var asyncOperation = SceneManager.LoadSceneAsync("WinningScene");

                // Wait for the scene to load
                asyncOperation.completed += (operation) =>
                {
                    // Start fading in once the new scene is loaded
                    _fadeCanvasGroup.DOFade(0, 1.0f);
                };
            });
        }

        public void Play()
        {
            _fadeCanvasGroup.gameObject.SetActive(true);
            _fadeCanvasGroup.DOFade(1, 1.0f).OnComplete(() =>
            {
                // Load the scene asynchronously
                var asyncOperation = SceneManager.LoadSceneAsync("MapScene");

                // Wait for the scene to load
                asyncOperation.completed += (operation) =>
                {
                    _roomManager.ConnectRoom();
                    // Start fading in once the new scene is loaded
                    _fadeCanvasGroup.DOFade(0, 1.0f);
                    _fadeCanvasGroup.gameObject.SetActive(false);
                };
            });
        }

        public void IncreasePlayerNumber(int payloadCustomData)
        {
            // ++PlayerCount;
            // Debug.Log($"PlayerCount: {PlayerCount}");
            EventAggregator.Instance?.RaiseEvent(new PlayerJoinRoomEvent());
        }
    }
}