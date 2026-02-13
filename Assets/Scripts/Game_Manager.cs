using UnityEngine;
using System.Collections;

public class Game_Manager : MonoBehaviour
{
    public static Game_Manager Instance;

    [Header("Game Data")]
    public int coins = 0;

    [Header("Camera Shake")]
    public Transform cam;
    private Vector3 originalCamPos;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        originalCamPos = cam.localPosition;
    }

    // ========================
    // Coins
    // ========================
    public void AddCoin(int amount)
    {
        coins += amount;
        Debug.Log("Coins: " + coins);

        // TODO:
        // update UI
        // play sound
        // save data
    }

    // ========================
    // Screen Shake
    // ========================
    public void Shake(float duration, float magnitude)
    {
        // TODO: Implement it 
        // stop previous shake
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cam.localPosition = originalCamPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.localPosition = originalCamPos;
    }
}
