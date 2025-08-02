using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Characterdeath : MonoBehaviour
{
    public CharacterMovement characterMovement;

    public void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
    }

    public void Start()
    {

    }
    // 체력이 0 이하일 때 호출
    public void CheckDeath(Stats stats)
    {
        if (!stats.isdie && stats.hp <= 0)
        {
            stats.isdie = true;
            stats.isPatternEnd = true; // 패턴 종료 상태 업데이트
            if (stats.team == Team.enemy)
            {
                StageDataManager.Instance.CheckKill(); // 스토리 체크
            }
            Die();
        }
    }

/*    public void OnDestroy()
    {
        var manager = CharacterStats.Instance;
        var stats = manager.GetStats(gameObject);
        stats.isdie = true;
        stats.isPatternEnd = true; // 패턴 종료 상태 업데이트
    }*/

    private void Die()
    {
        // 사망 처리 (예: 오브젝트 비활성화, 애니메이션 등)
        characterMovement.ClearHighlights();
        Destroy(gameObject);
        Debug.Log($"{gameObject.name} 사망!");
    }
}