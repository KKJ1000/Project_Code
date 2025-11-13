using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuFadeIn : MonoBehaviour
{
    [SerializeField] private Image _fadeImage;
    [SerializeField] private float _fadeTime = 3.5f;

    private void Awake()
    {
        _fadeImage = GetComponent<Image>();
        _fadeImage.gameObject.SetActive(true);
    }

    private void Start()
    {
        StartCoroutine(FadeIn(_fadeTime));
    }

    private IEnumerator FadeIn(float duration)
    {
        Color color = _fadeImage.color;
        float elapsedTime = 0f;

        while (_fadeImage.color.a > 0f)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - (elapsedTime / duration);
            _fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        _fadeImage.color = color;
        gameObject.SetActive(false);
    }
}
