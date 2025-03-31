using System.Collections.Generic;
using UnityEngine;

public class DeBuff
{
    public string name;// 캐릭터 이름
    public Dictionary<string, int> CC = new Dictionary<string, int>();//현제 걸린 군중제어효과
    public Dictionary<string, int> Debuff = new Dictionary<string, int>();//현제 걸린 디버프
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public DeBuff(Character data, Dictionary<string, int> cc, Dictionary<string, int> debuff)
    {
        name = data.charactername;
        CC = cc;
        debuff = Debuff;
    }
}
