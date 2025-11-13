using TMPro;
using UnityEngine;

public class MonsterDamageUI : MonoBehaviour
{
    [SerializeField, Tooltip("받은 데미지 텍스트")] private TextMeshProUGUI receivedDamageText;
    private float totalDamage;

    private void Update()
    {
        receivedDamageText.transform.LookAt(Camera.main.transform);
        receivedDamageText.transform.Rotate(0f, 180f, 0f);
    }

    public void EnableDamageText(float damage)
    {
        CancelInvoke();
        receivedDamageText.gameObject.SetActive(true);
        totalDamage += damage;
        receivedDamageText.text = $"{totalDamage.ToString("F0")}";
        Invoke("DisableDamageText", 5f);
    }

    public void DisableDamageText()
    {
        receivedDamageText.gameObject.SetActive(false);
        totalDamage = 0;
    }
}
