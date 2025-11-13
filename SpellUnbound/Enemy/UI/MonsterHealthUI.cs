using UnityEngine;
using UnityEngine.UI;

public class MonsterHealthUI : MonoBehaviour
{
    [SerializeField] private Slider hpBarSlider;
    [SerializeField] private Text healthPointText; // 현재는 보스 몬스터에만 존재하는 텍스트

    private BaseMonster monster;
    private float currentHealth;
    public float maxHealth;

    void Start()
    {
        monster = GetComponent<BaseMonster>();
        InitUI();
        DisableHpUI();
    }

    void Update()
    {
        if (!monster.IsBoss)
        {
            hpBarSlider.transform.LookAt(Camera.main.transform);
            hpBarSlider.transform.Rotate(0f, 180f, 0f);
        }
    }

    public void InitUI()
    {
        maxHealth = monster.healthPoint;
        currentHealth = maxHealth;
        hpBarSlider.value = currentHealth / maxHealth;

        if (healthPointText != null)
        {
            healthPointText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    public void UpdateHpUI()
    {
        currentHealth = monster.healthPoint;
     
        if (hpBarSlider != null)
        {
            hpBarSlider.value = currentHealth / maxHealth;
        }

        if (healthPointText != null)
        {
            healthPointText.text = $"{(int)currentHealth} / {maxHealth}";
        }
    }

    public void DisableHpUI()
    {
        hpBarSlider.gameObject.SetActive(false);

        if (healthPointText != null)
        {
            healthPointText.gameObject.SetActive(false);
        }
    }

    public void EnableHpUI()
    {
        hpBarSlider.gameObject.SetActive(true);

        if (healthPointText != null)
        {
            healthPointText.gameObject.SetActive(true);
        }
    }
}
