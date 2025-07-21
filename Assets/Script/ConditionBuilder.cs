using UnityEngine;
using System;
using UnityEngine.TextCore.Text;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

// 스탯 속성 종류
public enum AttributeType { hp, mp }
// 비교 연산 종류

public enum Condition_statement
{
    none,       //없음
    more,       //이상
    over,       //초과
    below,      //이하
    under,      //미만
    equal,      //같다
    not_equal  //같지않다
}

public enum Condition_Hit
{
    none,       //없음
    hit,       //적중
    not_hit,   //적중하지않음
    gurd,        //방어
    not_gurd,       //방어하지않음
    parrying      //패링
}




// 조건 판별용 델리게이트 (Stats 기준)
public delegate bool ConditionPredicate(Stats self, Stats enemy, SkillData skillData, Team team);
// 조건 빌더 패턴 클래스

public enum ValueSourceType { Constant, Self, Target, Team }
public class ValueSource
{
    public ValueSourceType SourceType { get; }
    public AttributeType Attribute { get; }
    public float ConstantValue { get; }

    private ValueSource(ValueSourceType type, AttributeType attr, float value)
    {
        SourceType = type;
        Attribute = attr;
        ConstantValue = value;
    }

    public static ValueSource Constant(float value) => new ValueSource(ValueSourceType.Constant, default, value);
    public static ValueSource Self(AttributeType attr) => new ValueSource(ValueSourceType.Self, attr, 0);
    public static ValueSource Target(AttributeType attr) => new ValueSource(ValueSourceType.Target, attr, 0);

    public float GetValue(Stats target1, Stats target2, Team team)
    {
        return SourceType switch
        {
            ValueSourceType.Constant => ConstantValue,
            ValueSourceType.Self => GetStatValue(target1, Attribute),
            ValueSourceType.Target => GetStatValue(target2, Attribute),
            // Team 관련은 필요시 구현
            _ => 0
        };
    }

    private float GetStatValue(Stats stats, AttributeType attr)
    {
        if (stats == null) return 0;
        return attr switch
        {
            AttributeType.hp => stats.hp,
            AttributeType.mp => stats.mp,
            _ => 0
        };
    }
}


public class ConditionBuilder
{
    private ConditionPredicate _predicate;

    // 내부 생성자
    private ConditionBuilder(ConditionPredicate predicate)
    {
        _predicate = predicate;
    }

    /// <summary>
    /// 스탯 속성 비교 조건 생성
    /// </summary>
    /// <param name="target">대상(자신/적)</param>
    /// <param name="attr">속성(체력/마나)</param>
    /// <param name="comp">비교 연산</param>
    /// <param name="value">비교 값</param>
    public static ConditionBuilder Attribute(Target target, AttributeType attr, Condition_statement comp, float value)
    {
        var manager = CharacterStats.Instance;

        return new ConditionBuilder((self, enemy, skillData, team) =>
        {
            // 대상 Stats 선택
            Stats stats = target switch
            {
                Target.self => self,
                Target.enemy => enemy,
                _ => null
            };
            if (stats == null) return false;
            // 속성 값 추출
            float attrValue = attr switch
            {
                AttributeType.hp => stats.hp,
                AttributeType.mp => stats.mp,
                _ => 0
            };
            // 비교 연산 수행
            return comp switch
            {
                Condition_statement.below => attrValue < value,
                Condition_statement.more => attrValue >= value,
                Condition_statement.equal => Mathf.Approximately(attrValue, value),
                Condition_statement.not_equal => !Mathf.Approximately(attrValue, value),
                _ => false
            };
        });
    }

    // ConditionHit 조건에 따라 유닛 충돌 여부를 판별하는 ConditionBuilder 생성 함수
    public static ConditionBuilder HitCondition(
        Target target, TargetUnit type, Condition_Hit comparison, string value)
    {
        return new ConditionBuilder((self, enemy, skillData, team) =>
        {

            // 1. target: 어느 팀원의 유닛인지 판별
            bool targetMatch = true;
            switch (target)
            {
                case Target.self:
                    targetMatch = self != null && enemy == self;
                    break;
                case Target.team:
                    targetMatch = self != null && enemy != null && self.team == enemy.team;
                    break;
                case Target.enemy:
                    targetMatch = self != null && enemy != null && self.team != enemy.team;
                    break;
                case Target.all:
                    targetMatch = enemy != null;
                    break;
                default:
                    targetMatch = false;
                    break;
            }
            if (!targetMatch) return false;

            // 2. 타입 구분: 캐릭터(Stats)인지, 스킬(SkillData)인지

            bool typeMatch = false;

            if (type == TargetUnit.character && enemy != null)
            {
                typeMatch = true;
            }
            else if (type == TargetUnit.skill && skillData != null)
            {
                typeMatch = true;   
            }
/*            else if (type == TargetUnit.all)
            {
                typeMatch = true;
            }*/
            if (!typeMatch) return false;

            // 3. comparison: 적중, 방어 등 상황 판별
            bool comparisonMatch = true;
            switch (comparison)
            {
/*                case Condition_Hit.hit:
                    comparisonMatch = enemy != null && enemy.lastHitType == Condition_Hit.hit;
                    break;
                case Condition_Hit.gurd:
                    comparisonMatch = enemy != null && enemy.lastHitType == Condition_Hit.gurd;
                    break;
                case Condition_Hit.parrying:
                    comparisonMatch = enemy != null && enemy.lastHitType == Condition_Hit.parrying;
                    break;
                case Condition_Hit.not_hit:
                    comparisonMatch = enemy != null && enemy.lastHitType == Condition_Hit.not_hit;
                    break;
                case Condition_Hit.not_gurd:
                    comparisonMatch = enemy != null && enemy.lastHitType == Condition_Hit.not_gurd;
                    break;*/
                default:
                    comparisonMatch = true;
                    break;
            }
            if (!comparisonMatch) return false;

            // 4. value: 비어있지 않으면 이름 비교 (skillName 또는 charactername)
            if (!string.IsNullOrEmpty(value))
            {
                bool nameMatch = false;
                // enemyStats.lastHitSkillData.name == value
                if (skillData != null && skillData.skillName == value)
                {
                    nameMatch = true;
                }
                // enemy의 캐릭터 이름 비교
                if (!nameMatch && enemy != null && enemy.name == value)
                {
                    nameMatch = true;
                }
                if (!nameMatch) return false;
            }
            
            return true;
        });
    }



    /// <summary>
    /// AND 조건 조합
    /// </summary>
    public ConditionBuilder And(ConditionBuilder other)
    {
        return new ConditionBuilder((self, enemy, skillData, team) =>
            _predicate(self, enemy, skillData, team) && other._predicate(self, enemy, skillData, team));
    }

    /// <summary>
    /// OR 조건 조합
    /// </summary>
    public ConditionBuilder Or(ConditionBuilder other)
    {
        return new ConditionBuilder((self, enemy, skillData, team) =>
            _predicate(self, enemy, skillData, team) || other._predicate(self, enemy, skillData, team));
    }

    /// <summary>
    /// NOT 조건
    /// </summary>
    public ConditionBuilder Not()
    {
        return new ConditionBuilder((self, enemy, skillData, team) =>
            !_predicate(self, enemy, skillData, team));
    }

    /// <summary>
    /// 최종 Predicate 반환
    /// </summary>
    public ConditionPredicate Build() => _predicate;
}
