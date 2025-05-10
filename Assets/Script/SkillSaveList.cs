using System.Collections.Generic;
using UnityEngine;

public class SkillSaveList : MonoBehaviour
{
    public static SkillSaveList Instance;

    public SelectedSkillList selectedSkillList;
    public List<PendingSkillList> pendingSkillList;
    public List<ReactPendingSkillList> reactPendingSkillList;
}

[System.Serializable]
public class SelectedSkillList
{
    public SkillData selectedSkill = null;
    public GameObject selectedCaster = null;
    public Stats selectedCharacter = null;
    public Vector3 selectedAoeCenterPosition = Vector3.zero;
    public Vector3 selectedTargetPosition = Vector3.zero;
    public GameObject selectedTargetUnit = null;
}

[System.Serializable]
public class PendingSkillList
{
    public SkillData selectedSkill = null;
    public GameObject selectedCaster = null;
    public Stats selectedCharacter = null;
    public Vector3 selectedAoeCenterPosition = Vector3.zero;
    public Vector3 selectedTargetPosition = Vector3.zero;
    public GameObject selectedTargetUnit = null;
}

[System.Serializable]
public class ReactPendingSkillList
{
    public SkillData selectedSkill = null;
    public GameObject selectedCaster = null;
    public Stats selectedCharacter = null;
    public Vector3 selectedAoeCenterPosition = Vector3.zero;
    public Vector3 selectedTargetPosition = Vector3.zero;
    public GameObject selectedTargetUnit = null;
}