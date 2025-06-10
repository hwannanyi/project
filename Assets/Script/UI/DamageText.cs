using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public static DamageText Instance;
    public GameObject damageTextPrefab; // 인스펙터에서 할당

    void Awake()
    {
        // 싱글턴 패턴 적용 (중복 방지)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowDamage(Vector3 worldPosition, int amount, bool isHeal = false)
    {
        GameObject obj = Instantiate(damageTextPrefab, worldPosition, Quaternion.identity);
        TextMeshPro text = obj.GetComponent<TextMeshPro>();
        if (text != null)
        {
            text.text = Mathf.Abs(amount).ToString();
            text.color = isHeal ? Color.green : Color.red;
        }
    }
}
