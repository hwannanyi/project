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

// ValueSource (기존 그대로)
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
            //AttributeType.mp => stats.mp,
            _ => 0
        };
    }
}


/// <summary>
/// 상태패턴용 인터페이스: 하나의 조건(또는 조건 조합)을 표현
/// </summary>
public interface IConditionState
{
    bool Evaluate(Stats self, Stats enemy, SkillData skillData, Team team);
}

/// <summary>
/// 속성 비교 조건 상태 (AttributeCondition)
/// - ValueSource를 사용해 비교 값을 동적으로 지원
/// </summary>
public class AttributeConditionState : IConditionState
{
    public Target Target { get; }
    public AttributeType Attribute { get; }
    public Condition_statement Comparison { get; }
    public ValueSource CompareValue { get; }

    public AttributeConditionState(Target target, AttributeType attribute, Condition_statement comparison, ValueSource value)
    {
        Target = target;
        Attribute = attribute;
        Comparison = comparison;
        CompareValue = value;
    }

    public bool Evaluate(Stats self, Stats enemy, SkillData skillData, Team team)
    {
        Stats stats = Target switch
        {
            Target.self => self,
            Target.enemy => enemy,
            _ => null
        };
        if (stats == null) return false;

        float attrValue = Attribute switch
        {
            AttributeType.hp => stats.hp,
            //AttributeType.mp => stats.mp,
            _ => 0
        };

        float cmpValue = CompareValue?.GetValue(self, enemy, team) ?? 0;

        return Comparison switch
        {
            Condition_statement.below => attrValue < cmpValue,
            Condition_statement.more => attrValue >= cmpValue,
            Condition_statement.equal => Mathf.Approximately(attrValue, cmpValue),
            Condition_statement.not_equal => !Mathf.Approximately(attrValue, cmpValue),
            _ => false
        };
    }
}

/// <summary>
/// Hit 관련 조건 상태 (원래 HitCondition 로직 캡슐화)
/// </summary>
public class HitConditionState : IConditionState
{
    public Target Target { get; }
    public TargetUnit UnitType { get; }
    public Condition_Hit Comparison { get; }
    public string Value { get; }

    public HitConditionState(Target target, TargetUnit type, Condition_Hit comparison, string value)
    {
        Target = target;
        UnitType = type;
        Comparison = comparison;
        Value = value;
    }

    public bool Evaluate(Stats self, Stats enemy, SkillData skillData, Team team)
    {
        // 1. target 매칭
        bool targetMatch = true;
        switch (Target)
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

        // 2. 타입 매칭
        bool typeMatch = false;
        if (UnitType == TargetUnit.character && enemy != null) typeMatch = true;
        else if (UnitType == TargetUnit.skill && skillData != null) typeMatch = true;
        if (!typeMatch) return false;

        // 3. comparison (기존 주석 처리된 부분은 프로젝트 타입에 맞춰 확장)
        bool comparisonMatch = true;
        switch (Comparison)
        {
            // 실제 프로퍼티가 존재하면 여기서 체크
            default:
                comparisonMatch = true;
                break;
        }
        if (!comparisonMatch) return false;

        // 4. 값(이름) 비교
        if (!string.IsNullOrEmpty(Value))
        {
            bool nameMatch = false;
            if (skillData != null && skillData.skillName == Value) nameMatch = true;
            if (!nameMatch && enemy != null && enemy.name == Value) nameMatch = true;
            if (!nameMatch) return false;
        }

        return true;
    }
}


/// <summary>
/// Status(예: 기절) 조건 상태
/// Stats.HasStatus(StatusType) 를 사용하여 검사
/// </summary>
public class StatusConditionState : IConditionState
{
    public Target Target { get; }
    public StatusType Status { get; }
    public bool Required { get; }

    // Required: true => 해당 상태가 있어야 true, false => 없어야 true
    public StatusConditionState(Target target, StatusType status, bool required = true)
    {
        Target = target;
        Status = status;
        Required = required;
    }

    public bool Evaluate(Stats self, Stats enemy, SkillData skillData, Team team)
    {
        Stats stats = Target switch
        {
            Target.self => self,
            Target.enemy => enemy,
            _ => null
        };
        if (stats == null) return false;

        bool has = stats.HasStatus(Status);
        return Required ? has : !has;
    }
}


/// <summary>
/// Composite 상태들: And, Or, Not
/// </summary>
public class AndConditionState : IConditionState
{
    private readonly IConditionState[] _children;
    public AndConditionState(params IConditionState[] children) => _children = children;
    public bool Evaluate(Stats self, Stats enemy, SkillData skillData, Team team)
    {
        foreach (var c in _children) if (!c.Evaluate(self, enemy, skillData, team)) return false;
        return true;
    }
}

public class OrConditionState : IConditionState
{
    private readonly IConditionState[] _children;
    public OrConditionState(params IConditionState[] children) => _children = children;
    public bool Evaluate(Stats self, Stats enemy, SkillData skillData, Team team)
    {
        foreach (var c in _children) if (c.Evaluate(self, enemy, skillData, team)) return true;
        return false;
    }
}

public class NotConditionState : IConditionState
{
    private readonly IConditionState _child;
    public NotConditionState(IConditionState child) => _child = child;
    public bool Evaluate(Stats self, Stats enemy, SkillData skillData, Team team) => !_child.Evaluate(self, enemy, skillData, team);
}


// 기존 ConditionBuilder 유지 + 상태 기반 생성기 추가
public class ConditionBuilder
{
    private ConditionPredicate _predicate;

    // 내부 생성자
    private ConditionBuilder(ConditionPredicate predicate)
    {
        _predicate = predicate;
    }

    /// <summary>
    /// 상태(IConditionState)로부터 ConditionBuilder 생성 (상태패턴 통합 포인트)
    /// </summary>
    public static ConditionBuilder FromState(IConditionState state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        return new ConditionBuilder((self, enemy, skillData, team) => state.Evaluate(self, enemy, skillData, team));
    }

    /// <summary>
    /// 스탯 속성 비교 조건 생성 (기존 API 유지)
    /// </summary>
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
                //AttributeType.mp => stats.mp,
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

    // ConditionHit 조건에 따라 유닛 충돌 여부를 판별하는 ConditionBuilder 생성 함수 (기존 API 유지)
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
            if (!typeMatch) return false;

            // 3. comparison: 적중, 방어 등 상황 판별
            bool comparisonMatch = true;
            switch (comparison)
            {
                default:
                    comparisonMatch = true;
                    break;
            }
            if (!comparisonMatch) return false;

            // 4. value: 비어있지 않으면 이름 비교 (skillName 또는 charactername)
            if (!string.IsNullOrEmpty(value))
            {
                bool nameMatch = false;
                if (skillData != null && skillData.skillName == value)
                {
                    nameMatch = true;
                }
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
