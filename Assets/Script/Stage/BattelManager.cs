using UnityEngine;

public class BattelManager : MonoBehaviour
{
    public int boss_hp = 100;
    public int player_hp = 100;

    public delegate void HitDamage(int dama);
    public static event HitDamage hitDamage;

}
