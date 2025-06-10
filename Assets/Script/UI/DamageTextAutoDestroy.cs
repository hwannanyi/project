using UnityEngine;

public class DamageTextAutoDestroy : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 2f); // 2초 뒤에 오브젝트 삭제
    }
}