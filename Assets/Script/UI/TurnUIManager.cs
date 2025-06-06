using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TurnUIManager : MonoBehaviour
{

    [Header("UI 텍스트 연결")]
    public TextMeshProUGUI trunCountText;  // UI에 표시될 텍스트
    public TextMeshProUGUI enemyCount; // 남은 적
    public TextMeshProUGUI waveCount; // 남은 적

    [Header("대응턴 확인")]
    public Image playerTrun;
    public Image enemyTrun;
    public Image playerReact;
    public Image enemyReact;

    [Header("대응턴 UI이미지")]
    public Sprite playerTrunAttack;
    public Sprite enemyTrunAttack;
    public Sprite NotplayerTrunAttack;
    public Sprite NotenemyTrunAttack;
    public Sprite playerReactGuard;
    public Sprite enemyReactGuard;
    public Sprite NotReactGuard;


    public void UpdateTrunCount(int turnCount)
    {
        trunCountText.text = turnCount.ToString() + "턴";
    }

    public void Updatenemytcount(int emeny)
    {
        enemyCount.text = "x" + emeny.ToString();
    }
    public void UpdateWaveCount(int wave)
    {
        waveCount.text = wave.ToString();
    }

    public void UpdateReactTurn(bool playerturn)
    {
        if(playerturn)
        {
            playerTrun.sprite = playerTrunAttack;
            enemyTrun.sprite = NotenemyTrunAttack;
            playerReact.sprite = NotReactGuard;
            enemyReact.sprite = enemyReactGuard;
        }
        else
        {
            playerTrun.sprite = NotplayerTrunAttack;
            enemyTrun.sprite = enemyTrunAttack;
            playerReact.sprite = playerReactGuard;
            enemyReact.sprite = NotReactGuard;
        }
    }

}
