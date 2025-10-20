using UnityEngine;

public class BattelManager : MonoBehaviour
{
    public static BattelManager instance;

    public int boss_hp = 50;
    public int boss_maxhp = 50;
    public int player_hp = 50;
    public int player_maxhp = 50;

    public int serveTurn_actionCount = 1;
    public int serveTurn_Count = 1;

    public delegate void HitDamage(int dama, bool playerTeam);
    public static event HitDamage hitDamage;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

    }

    public void Hit(int damage, bool playerTeam)
    {
        if(playerTeam)
        {
            player_hp -= damage;
            if (player_hp < 0) player_hp = 0;
        }
        else
        {
            boss_hp -= damage;
            if (boss_hp < 0) boss_hp = 0;
        }
    }

    public bool StageClear()
    {
        return boss_hp <= 0;
    }

    public bool StageFail()
    {
        return player_hp <= 0;
    }

    public void NextSerTurn()
    {
        serveTurn_Count -= 1;
    }

    public bool IsSerTurnEnd()
    {
        return serveTurn_Count <= 0;
    }
}
