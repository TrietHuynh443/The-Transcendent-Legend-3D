using UnityEngine;

namespace Network.SO
{
    [CreateAssetMenu(fileName = "CheckPointSO", menuName = "Data/CheckPointSO")]
    public class CheckPointSO : ScriptableObject
    {
        [SerializeField] private GameObject _checkPointOne;
        [SerializeField] private GameObject _checkPointTwo;
        [SerializeField] private GameObject _checkPointThree;
        [SerializeField] private GameObject _checkPointFour;
        [SerializeField] private GameObject _winningCheckPoint;
        public static GameObject LastCheckPoint = null;
    }
}