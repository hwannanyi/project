using System;
using UnityEngine;

public class SkillUpgrade : MonoBehaviour
{
    public int cost;
    public int[] chrtskill = new int[8]; //스킬배열칸
    // q ,w, e, r , q, w, e, r
    // 0 = 기본, 1 = 강화a, 2 = 강화b, 3 = 강화c
    public int nowChrt = -1; // 선택된 캐릭 -1 = 없음, 0 = 1번캐릭, 1 = 2번캐릭
    public int nowSkill = -1; // 선택된 스킬 -1 = 없음, 0 = q, 1 = w, 2 = e, 3 = r


    public void ChrtChoice(int i)
    {
        nowChrt = i;
    }

    public void SkillChoice(int i)
    {
        nowSkill = i;
    }
    public void UpgradeSkill(int skill_idx)
    {
        if (nowChrt == -1 || nowSkill == -1) return;
        chrtskill[nowSkill + nowChrt * 4] =
            chrtskill[nowSkill + nowChrt * 4] == skill_idx ?
            0  // 현재 스킬이 선택된 스킬과 같으면 초기화
            : 
            skill_idx; // 캐릭터의 스킬 업그레이드
    }
}
