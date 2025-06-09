using UnityEngine;

public class Characterdeath : MonoBehaviour
{
    // 체력이 0 이하일 때 호출
    public void CheckDeath(Stats stats)
    {
        if (!stats.isdie && stats.hp <= 0)
        {
            stats.isdie = true;
            Die();
        }
    }

    private void Die()
    {
        // 사망 처리 (예: 오브젝트 비활성화, 애니메이션 등)
        gameObject.SetActive(false);
        Debug.Log($"{gameObject.name} 사망!");
    }
}