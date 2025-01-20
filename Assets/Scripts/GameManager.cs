using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class GameManager : UnitySingleton<GameManager>
    {
        [SerializeField] GameObject _fadeCanvasPrefab;
        private CanvasGroup _fadeCanvasGroup;

        private void Start()
        {
            _fadeCanvasGroup = Instantiate(_fadeCanvasPrefab).GetComponent<CanvasGroup>();
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
    }
}