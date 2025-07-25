using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AITurn : MonoBehaviour
{
    public float turntime = 3.0f; // AI 턴 지속 시간
    public float turnTimer = 0f; // 타이머 초기화

    public void Update()
    {
        if(CharacterStats.Instance == null)
            return;
        if (CharacterStats.Instance.characterList.Count == 0)
            return;
        if (TurnManager.Instance.isPlayerTurn)
            return;

        if (CharacterStats.Instance.characterList
            .Where(stats => stats.team == Team.enemy)
            .All(stats => stats.isPatternEnd))
        {
            turnTimer += Time.deltaTime; // 타이머 업데이트

            if (turnTimer >= turntime)
            {
                EventManager.Instance.FinishTurn(true); // AI 턴 종료
                turnTimer = 0f;
            }
        }
        else
        {
            turnTimer = 0f;
        }
    }
}
