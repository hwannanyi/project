using UnityEngine;

/// <summary>
/// 스킬 사용 전, 대응 조건을 체크하고 대응단계를 시작하는 관리 클래스
/// </summary>
public class ReactManager : MonoBehaviour
{
    public static ReactManager Instance;

    private void Awake()
    {

            Instance = this;

    }

    /// <summary>
    /// 타겟이 해당 스킬에 대응할 수 있는지 여부를 판정
    /// </summary>
    public bool CanRespond(SkillData skill)
    {
        if (skill == null) return false;

        // 예시: 상태이상, 대응 스킬 유무, 반응속도 등 추후 확장
/*        if (stats.IsStunned()) return false;
        if (!stats.HasAvailableResponseSkill()) return false;
        if (stats.GetResponseSpeed() < skill.speed) return false;*/

        return true;
    }

    /// <summary>
    /// 대응단계를 시작함. UI/입력 대기/아군 지원 등은 여기서 구현
    /// </summary>
/*    public void EnterResponsePhase(SkillData skill, GameObject caster)
    {

        CharacterSelection.selectedCharacterIndex = -1;
        Debug.Log($"[ResponseManager] 대응단계 진입: 스킬: {skill.skillName}");

        // 대응 선택 UI, 타이머, 대응 가능한 스킬 목록 표시 등 처리 예정
        // 현재는 로그만 출력하고 바로 대응 없이 종료

        // 추후: 대응 입력을 받아 스킬 차단/회피/반격 등 처리
    }*/

    public void StartReact()
    {
        
    }
}