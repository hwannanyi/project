using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CharacterEffect : MonoBehaviour
{
    public CharacterMovement characterMovement;
    private Stats character;

/*    void Start()
    {
        var stats = CharacterStats.Instance;
        character = stats.GetStats(gameObject);

        // 리스트가 null이거나 생성되지 않았을 때 새로 생성
        if (character.buffEffects == null)
            character.buffEffects = new List<Stats.Buffa>();
        if (character.debuffEffects == null)
            character.debuffEffects = new List<Stats.Debuffa>();
        if (character.ccEffects == null)
            character.ccEffects = new List<Stats.CC>();
    }*/

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        // TurnEnd 이벤트 구독
        EventManager.Instance.TurnEnd += OnTurnEnd;

    }
    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (EventManager.Instance != null)
            EventManager.Instance.TurnEnd -= OnTurnEnd;
    }

    void Update()
    {
        var stats = CharacterStats.Instance;
        character = stats.GetStats(gameObject);

        Buff_solid(character);
        DeBuff_corrosion(character);
        CC_stun(character);
    }

    // 턴 종료 시 버프의 trun 감소
    private void OnTurnEnd(bool value)
    {
        foreach (var buff in character.buffEffects)
        {
            buff.trun -= 1;
        }
        // trun이 0 이하인 버프 제거
        character.buffEffects.RemoveAll(b => b.trun <= 0);

        foreach (var buff in character.debuffEffects)
        {
            buff.trun -= 1;
        }
        // trun이 0 이하인 버프 제거
        character.debuffEffects.RemoveAll(b => b.trun <= 0);

        foreach (var buff in character.ccEffects)
        {
            buff.trun -= 1;
        }
        // trun이 0 이하인 버프 제거
        character.ccEffects.RemoveAll(b => b.trun <= 0);
    }

    //버프
    //경화
    //받는 피해 감소
    public void Buff_solid(Stats stats)
    {
        bool hasSolidEffect = character.buffEffects.Any(b => b.effect == Buffs.solid);
        bool turn = character.buffEffects.Any(b => b.trun >= 0);
        bool applied = false;

        // Buffs.solid 효과의 Value 합산
        float solidBuffValue = character.buffEffects
            .Where(b => b.effect == Buffs.solid)
            .Sum(b => b.Value);

        if (hasSolidEffect && turn && !applied)
        {


            stats.damageReduction += solidBuffValue;
            applied = true; // 적용되었음을 표시
        }
        else if (hasSolidEffect && !turn && !applied)
        {
            stats.damageReduction -= solidBuffValue;
            return; // 지속시간이 끝나면 되돌리기
        }

    }

    //디버프
    //부식
    //받는 피해 증가
    public void DeBuff_corrosion(Stats stats)
    {
        bool hasSolidEffect = character.debuffEffects.Any(b => b.effect == Debuffs.corrosion);
        bool turn = character.debuffEffects.Any(b => b.trun >= 0);
        bool applied = false;

        // Buffs.solid 효과의 Value 합산
        float solidBuffValue = character.debuffEffects
            .Where(b => b.effect == Debuffs.corrosion)
            .Sum(b => b.Value);

        if (hasSolidEffect && turn && !applied)
        {


            stats.damageReduction -= solidBuffValue; // 10% 피해 감소
            applied = true; // 적용되었음을 표시
        }
        else if (hasSolidEffect && !turn && !applied)
        {
            stats.damageReduction += solidBuffValue;
            return; // 지속시간이 끝나면 되돌리기
        }
    }

    //CC기
    //기절
    //행동불가
    public void CC_stun(Stats stats)
    {
        bool hasSolidEffect = character.ccEffects.Any(b => b.effect == CCs.stun);
        bool turn = character.ccEffects.Any(b => b.trun >= 0);
        //bool applied = false;

        // CCs.stun 효과의 Value 합산
        float stunCCValue = character.ccEffects
            .Where(b => b.effect == CCs.stun)
            .Sum(b => b.Value);


        if (hasSolidEffect && turn)
        {
            stats.available = false; // 행동불가
            stats.movable = false; // 이동불가
            //applied = true; // 적용되었음을 표시
        }
        else if (hasSolidEffect && !turn)
        {
            stats.available = true; // 행동불가
            stats.movable = true; // 이동불가
            return; // 지속시간이 끝나면 되돌리기
        }
    }

}
