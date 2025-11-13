using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashSceneFadeOut : MonoBehaviour
{
    [SerializeField] private Image _fadeImage;
    [SerializeField] private float _fadeTime = 3.5f;

    private void Start()
    {
        StartCoroutine(FadeOut(_fadeTime));
    }

    private IEnumerator FadeOut(float duration)
    {
        _fadeImage.enabled = true;
        Color color = _fadeImage.color;

        float elapsedTime = 0f;

        while (_fadeImage.color.a < 1f)
        {
            elapsedTime += Time.deltaTime;
            color.a = elapsedTime / duration;
            _fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        _fadeImage.color = color;
        SceneManager.LoadScene("02_MainMenu");
    }
}
