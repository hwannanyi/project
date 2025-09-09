using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

using static UnityEngine.Rendering.DebugUI;

public class AITurn : MonoBehaviour
{
    public float turntime = 3.0f; // AI 턴 지속 시간
    public float turnTimer = 0f; // 타이머 초기화
    public StoryManager storyManager; // 스토리 매니저
    public CharacterStats characterStats; // 캐릭터 스탯 매니저
    public TurnManager turnManager; // 턴 매니저 
    public bool AIturnEnd = false; // AI 턴 종료 여부

    public void Awake()
    {

        storyManager = GetComponent<StoryManager>();
        characterStats = GetComponent<CharacterStats>();
        turnManager = GetComponent<TurnManager>();
    }

    public void Update()
    {
        // Team.enemy인 캐릭터만 enemies 리스트에 저장
        var enemies = characterStats.characterList
            .Where(c => c.team == Team.enemy)
            .ToList();
        // 모든 적이 죽었는지 확인 (예시: enemies 리스트 사용)
        bool allEnemiesDead = enemies.All(e => e.isdie);

        // 스토리 종료 상태 확인
        if (allEnemiesDead && storyManager.ispopUpStoryEnd && storyManager.isStoryEnd && characterStats.characterCreat)
        {
            Debug.Log("스테이지 클리어!");
            StartCoroutine(CheckStageClearRoutine()); // 게임 씬으로 전환
            return;
            // 필요하다면 UI에 메시지 표시 등 추가 작업
        }

        if (CharacterStats.Instance == null)
            return;
        if (CharacterStats.Instance.characterList.Count == 0)
            return;
        if (TurnManager.Instance.isPlayerTurn)
            return;

        if (CharacterStats.Instance.characterList
            .Where(stats => stats.team == Team.enemy)
            .All(stats => stats.isPatternEnd) &&
            !(storyManager.isStoryActive || storyManager.popUpisStoryActive))
        {
            
            turnTimer += Time.deltaTime; // 타이머 업데이트
            AIturnEnd = false; // AI 턴 종료 상태 초기화
            if (turnTimer >= turntime)
            {
                EventManager.Instance.FinishTurn(); // AI 턴 종료
                turnTimer = 0f;
                AIturnEnd = true; // AI 턴 종료 상태 업데이트
            }
        }
        else
        {
            turnTimer = 0f;
        }
    }

    private IEnumerator CheckStageClearRoutine()
    {
        // popUpisStoryActive와 isStoryActive가 모두 true면 대기
        while (StoryManager.instance != null &&
               StoryManager.instance.popUpisStoryActive &&
               StoryManager.instance.isStoryActive)
        {
            yield return null; // 한 프레임 대기
        }

        Debug.Log("스테이지 클리어!");
        SceneManager.LoadScene("Stage_Selection"); // 게임 씬으로 전환
        yield break;
    }
}
