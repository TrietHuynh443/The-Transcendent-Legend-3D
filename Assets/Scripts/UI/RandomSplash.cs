using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomSplash : MonoBehaviour
{
    [SerializeField] private GameObject vfxPrefab; // The VFX prefab to instantiate
    [SerializeField] private GameObject randPosHolder; // Parent object containing random positions
    [SerializeField] private int poolSize = 10; // Initial size of the pool
    [SerializeField] private float vfxLifetime = 5.0f; // Lifetime of each VFX

    private Queue<GameObject> vfxPool; // The object pool
    private RectTransform[] randPos; // Array of random positions as RectTransforms
    
    void Start()
    {
        // Initialize random positions
        int childCount = randPosHolder.transform.childCount;
        randPos = new RectTransform[childCount];
        int index = 0;
        foreach (Transform child in randPosHolder.transform)
        {
            randPos[index] = child.GetComponent<RectTransform>();
            index++;
        }

        // Initialize the object pool
        vfxPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject vfx = Instantiate(vfxPrefab, randPosHolder.transform); // Instantiate within UI hierarchy
            vfx.SetActive(false); // Start inactive
            vfxPool.Enqueue(vfx);
        }

        StartCoroutine(LoadUIAndManagers());
    }

    private IEnumerator LoadUIAndManagers()
    {
        yield return new WaitForSeconds(vfxLifetime);
        GameManager gameManager = GameManager.Instance;
        var loadSceneOperation = SceneManager.LoadSceneAsync("UI");

        yield return loadSceneOperation;
    }

    void Update()
    {

        if (vfxPool.Count > 0)
        {
            GameObject vfx = vfxPool.Dequeue();

            // Random position
            int randomIndex = Random.Range(0, randPos.Length);
            RectTransform vfxRect = vfx.GetComponent<RectTransform>();
            vfxRect.SetParent(randPosHolder.transform, false); // Ensure it stays in the UI hierarchy
            vfxRect.anchoredPosition = randPos[randomIndex].anchoredPosition;

            // Random size
            float randomSize = Random.Range(0.5f, 2.0f);
            vfxRect.localScale = new Vector3(randomSize, randomSize, randomSize);

            // Activate and reset
            vfx.SetActive(true);

            // Return to the pool after a delay
            StartCoroutine(ReturnToPoolAfterDelay(vfx, vfxLifetime));
        }
        // else
        // {
        //     Debug.LogWarning("No available objects in the pool!");
        // }

    }

    IEnumerator ReturnToPoolAfterDelay(GameObject vfx, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Deactivate and return to the pool
        vfx.SetActive(false);
        vfxPool.Enqueue(vfx);
    }
}
