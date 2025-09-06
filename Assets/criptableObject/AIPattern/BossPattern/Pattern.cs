using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pattern", menuName = "Scriptable Objects/Pattern")]
public class Pattern : ScriptableObject
{
    public enum PatternType
    {
        circle,
        straight,
        repeat,
    }

    public Skill skill;

    [Header("circle")]
    public int ammo_circle;
    public float radius_circle;
    public bool isRadius_tracking_circle;
    public float delayTime_circle;
    public Vector3 position_circle;
    public bool isPosition_tracking_circle;
    public int count_circle;
    public bool isRandom_circle;

    [Header("straight")]
    public int ammo_straight;
    public int interval_straight;
    public Vector2 direction_straight;
    public float radius_straight;
    public bool isRadius_tracking_straight;
    public float delayTime_straight;
    public Vector3 position_straight;
    public bool isPosition_tracking_straight;
    public int count_straight;
    public bool isRandom_straight;


    [Header("repeat")]
    public List<SkillQueue> skill_repeat;
    public int count_repeat;
    public bool isRandom_repeat;
}
