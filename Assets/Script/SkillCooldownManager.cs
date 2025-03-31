using UnityEngine;

public class SkillCooldownManager : MonoBehaviour
{
    public UDictionary<string, int> cooldowns;


    void Update() 
    {
        string skillname;
        int skillcooldown;

        /*if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("9¹ø ÀÔ·Â");
            skillname = SkillUseManager.useSkillList[0].skillName;
            skillcooldown = SkillUseManager.useSkillList[0].cooldown;
            cooldowns.Add(skillname, skillcooldown);
            Debug.Log(cooldowns[skillname]);
        }*/
    }
}
