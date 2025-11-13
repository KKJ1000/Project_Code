using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] private Image m_fadeImage;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            m_fadeImage.gameObject.SetActive(false);
            DontDestroyOnLoad(gameObject);
        }

        // SceneManager.sceneLoaded += OnSceneLoaded;
    }


    public void StartFadeAndLoad(string sceneName, float fadeDuration, Func<IEnumerator> onLoaded = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[FadeManager] 씬 이름이 null 또는 빈 문자열입니다.");
            return;
        }

        StartCoroutine(FadeOutAndLoad(sceneName, fadeDuration, onLoaded));
    }


    private IEnumerator FadeOutAndLoad(string sceneName, float fadeDuration, Func<IEnumerator> onLoaded = null)
    {
        m_fadeImage.gameObject.SetActive(true);

        Color color = m_fadeImage.color;
        color.a = 0;
        m_fadeImage.color = color;

        float time = 0;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = time / fadeDuration;
            m_fadeImage.color = color;
            yield return null;
        }

        color.a = 1;
        m_fadeImage.color = color;

        var op = SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitUntil(() => op.isDone);

        if (onLoaded != null)
        {
            yield return onLoaded();
        }

        yield return FadeIn(fadeDuration);
    }

    private IEnumerator FadeOut(float duration, string name = null)
    {
        m_fadeImage.gameObject.SetActive(true);
        Color color = m_fadeImage.color;
        color.a = 0;
        m_fadeImage.color = color;

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = time / duration;
            m_fadeImage.color = color;
            yield return null;
        }

        color.a = 1;
        m_fadeImage.color = color;

        if (name != null)
        {
            SceneManager.LoadScene(name);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn(5f));
    }

    private IEnumerator FadeIn(float duration)
    {
        Color color = m_fadeImage.color;
        color.a = 1;
        m_fadeImage.color = color;

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = 1 - time / duration;
            m_fadeImage.color = color;
            yield return null;
        }

        color.a = 0;
        m_fadeImage.color = color;
        m_fadeImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// FadeOut Coroutine Start
    /// </summary>
    /// <param name="duration">페이드 아웃에 걸리는 시간</param>
    /// <param name="name">페이드 아웃 이후 로드할 씬 이름</param>
    public void StartFadeOut(float duration, string name = null)
    {
        StartCoroutine(FadeOut(duration, name));
    }
}
