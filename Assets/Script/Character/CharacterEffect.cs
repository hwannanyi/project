using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;


public class CharacterEffect : MonoBehaviour
{
    public CharacterMovement characterMovement;
    private Stats character;
    public Characterdeath characterdeath;
    public delegate void EffectAction(Stats stats);
    public EffectAction effectAction = null;

    public Dictionary<(EffectWrapper.EffectType, int), Action> EffectActions { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        characterdeath = GetComponent<Characterdeath>();
        characterMovement = GetComponent<CharacterMovement>();
        // TurnEnd 이벤트 구독
        EventManager.Instance.TurnEnd += OnTurnEnd;

    }
    void Start()
    {
        var stats = CharacterStats.Instance;
        character = stats.GetStats(gameObject);

        /*        // EffectActions 딕셔너리 초기화
                EffectActions = new Dictionary<(EffectWrapper.EffectType, int), Action>
                {
                    {(EffectWrapper.EffectType.Buff, (int)Buffs.solid), () => Buff_solid(character)},
                    {(EffectWrapper.EffectType.Debuff, (int)Debuffs.corrosion), () => DeBuff_corrosion(character)},
                    {(EffectWrapper.EffectType.CC, (int)CCs.stun), () => CC_stun(character)}
                };*/

        effectAction = Unable;
    }

    void OnDestroy()
    {
        effectAction = null;
        // 이벤트 구독 해제
        if (EventManager.Instance != null)
            EventManager.Instance.TurnEnd -= OnTurnEnd;
    }

    void Update()
    {
        if(character.gurd > 0)
        {
            character.gurd -= Time.deltaTime;
        }
        TimeFlow();
        //HitDamage();
/*
        Buff_solid(character);
        DeBuff_corrosion(character);
        CC_stun(character);*/
    }



    // 턴 종료 시 버프의 trun 감소
    private void OnTurnEnd()
    {
        /*        foreach (var buff in character.buffEffects)
                {
                    buff.trun -= 1;
                }

                foreach (var buff in character.debuffEffects)
                {
                    buff.trun -= 1;
                }

                foreach (var buff in character.ccEffects)
                {
                    buff.trun -= 1;
                }*/
        foreach (StatusEffect sta in character.statusEffects)
        {
            sta.trun -= 1;
        }
        


    }

    public void TimeFlow()
    {
        /*        foreach (Stats.Buffa eft in character.buffEffects)
                {
                    eft.time -= Time.deltaTime;
                }
                // trun이 0 이하인 버프 제거
                character.buffEffects.RemoveAll(b => b.trun <= 0 && b.time <= 0);

                foreach (Stats.Debuffa eft in character.debuffEffects)
                {
                    eft.time -= Time.deltaTime;
                }
                // trun이 0 이하인 버프 제거
                character.debuffEffects.RemoveAll(b => b.trun <= 0 && b.time <= 0);

                foreach (Stats.CC eft in character.ccEffects)
                {
                    eft.time -= Time.deltaTime;
                }
                character.ccEffects.RemoveAll(b => b.trun <= 0 && b.time <= 0);*/
        effectAction.Invoke(character);

        // 역순 순회
        for (int i = character.statusEffects.Count - 1; i >= 0; i--)
        {
            var sta = character.statusEffects[i];
            sta.time -= Time.deltaTime;
            if (sta.trun <= 0 && sta.time <= 0)
            {
                RemoveStatus(sta); //리스트에서 제거
            }
        }

    }

    public void RemoveStatus(StatusEffect status)
    {
        character.RemoveStatus(status.status);
        character.statusEffects.Remove(status);
    }

    public void Unable(Stats stats)
    {
            if (stats.HasStatus(StatusType.sturn))
            {
                stats.available = false;
                stats.movable = false;
            }
            else
            {
                stats.available = true;
                stats.movable = true;
            }
    }


/*    //홀드 도중 들어오는 데미지
    public void HitDamage()
    {
        try
        {
            foreach (HoldEffect buff in character.holdGauge)
            {
                // 기존 코드
                // if(buff.effect == (EffectWrapper.EffectType.Hit, (int)SkillhitEffect.damage))

                // 수정된 코드
                if (!(buff.effect.effectType == EffectWrapper.EffectType.Hit && buff.effect.Hit == SkillhitEffect.damage))
                    return;
                buff.curtic += Time.deltaTime;
                if (buff.curtic >= buff.tic)
                {
                    int finalDamage = (int)(buff.value);
                    int targetShlields = character.shields; // 현재 대상의 보호막 값
                    GameObject targetObj = character.characterPrefab;
                    if (targetShlields > 0)
                    {

                        targetShlields = targetShlields >= finalDamage ? targetShlields - finalDamage : 0;

                        int damageExceeded = targetShlields < finalDamage ? -(targetShlields - finalDamage) : 0;
                        character.hp -= -damageExceeded;
                        DamageText.Instance.ShowDamage(targetObj.transform.position + Vector3.up * 1.5f, finalDamage - damageExceeded, false);
                        if (damageExceeded > 0) { DamageText.Instance.ShowDamage(targetObj.transform.position + Vector3.up * 2f, damageExceeded, false); }
                    }
                    else
                    {
                        character.hp -= finalDamage;
                        DamageText.Instance.ShowDamage(character.characterPrefab.transform.position + Vector3.up * 1.5f, finalDamage, false);
                    }
                    buff.curtic = 0f; // 틱 초기화

                    if (character != null && !character.isdie && character.hp <= 0)
                        characterdeath.CheckDeath(character);
                }
            }
        }
        catch
        {
            return;
        }
    }*/

    //버프
    //경화
    /*    public void Buff_solid(Stats stats)
        {
            foreach (var buff in character.buffEffects)
            {
                if (buff.effect == Buffs.solid && buff.trun > 0 && !buff.isApplied)
                {
                    stats.damageReduction += buff.Value;
                    buff.isApplied = true;
                }
                // 만약 trun이 0 이하인데 isApplied가 true라면 효과 해제
                else if (buff.effect == Buffs.solid && buff.trun <= 0 && buff.isApplied)
                {
                    stats.damageReduction -= buff.Value;
                    buff.isApplied = false;
                }
            }
        }
    */
    /*    //디버프
        //부식
        //받는 피해 증가
        public void DeBuff_corrosion(Stats stats)
        {
            foreach (var debuff in character.debuffEffects)
            {
                if (debuff.effect == Debuffs.corrosion && debuff.trun > 0 && !debuff.isApplied)
                {
                    stats.damageReduction -= debuff.Value;
                    debuff.isApplied = true;
                }
                else if (debuff.effect == Debuffs.corrosion && debuff.trun <= 0 && debuff.isApplied)
                {
                    stats.damageReduction += debuff.Value;
                    debuff.isApplied = false;
                }
            }
        }*/

    /*    //CC기
        //기절
        //행동불가
        public void CC_stun(Stats stats)
        {
            foreach (var cc in character.ccEffects)
            {
                if (cc.effect == CCs.stun && cc.trun > 0 && !cc.isApplied)
                {
                    stats.available = false;
                    stats.movable = false;
                    cc.isApplied = true;
                }
                else if (cc.effect == CCs.stun && cc.trun <= 0 && cc.isApplied)
                {
                    stats.available = true;
                    stats.movable = true;
                    cc.isApplied = false;
                }
            }
        }*/

}
