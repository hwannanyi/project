using System.Linq;
using UnityEngine;

public class StageClear : MonoBehaviour
{
    public CharacterStats characterStats; // 캐릭터 스탯 참조 필드 추가

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckStageClear()
    {
        // Team.enemy인 캐릭터만 enemies 리스트에 저장
        var enemies = characterStats.characterList
            .Where(c => c.isPlayerTeam == Team.enemy)
            .ToList();
        // 모든 적이 죽었는지 확인 (예시: enemies 리스트 사용)
        bool allEnemiesDead = enemies.All(e => e.isdie);

        // 스토리 종료 상태 확인
        if (allEnemiesDead && StoryManager.instance != null && StoryManager.instance.isStoryEnd)
        {
            Debug.Log("스테이지 클리어!");
            // 필요하다면 UI에 메시지 표시 등 추가 작업
        }
    }
}
