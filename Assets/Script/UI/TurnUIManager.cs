using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TurnUIManager : MonoBehaviour
{

    [Header("UI 텍스트 연결")]
    public TextMeshProUGUI trunCountText;  // UI에 표시될 텍스트
    public TextMeshProUGUI enemyCount; // 남은 적
    public TextMeshProUGUI waveCount; // 남은 적

/*    [Header("대응턴 확인")]
    public Image playerTrun;
    public Image enemyTrun;
    public Image playerReact;
    public Image enemyReact;*/

    [Header("공격턴 수비턴")]
    public TextMeshProUGUI Trun;
    public Image TrunBackgroundColor;

    public GameObject Skill_Cancel_Button;
    public GameObject Skill_Start_Button;
    public GameObject Skill_TurnEnd_Button;

    private Coroutine colorFadeCoroutine;
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


    //공격턴 수비턴에 따라 버튼 활성화 비활성화
    public void UpdateButtonActive(bool playerturn)
    {
        if (playerturn)
        {
            Skill_Cancel_Button.SetActive(true);
            Skill_Start_Button.SetActive(true);
            Skill_TurnEnd_Button.SetActive(true);
        }
        else
        {
            Skill_Cancel_Button.SetActive(false);
            Skill_Start_Button.SetActive(false);
            Skill_TurnEnd_Button.SetActive(false);
        }
    }

    public void UpdateReactTurn(bool playerturn)
    {
        if(playerturn)
        {
            // "현제 턴:"은 검은색, "공격"은 하늘색(#00D5FF)
            Trun.text = "<color=#00D5FF>현제 턴: 공격</color>";
            StartColorFade(new Color(0f, 0.835f, 1f)); // 파란색 (#00D5FF)
        }
        else
        {
            // "현제 턴:"은 검은색, "공격"은 하늘색(#F6375D)
            Trun.text = "<color=#F6375D>현제 턴: 방어</color>";
            StartColorFade(new Color(0.964f, 0.216f, 0.364f)); // 빨간색 (#F6375D)
        }
        UpdateButtonActive(playerturn);
    }
    private void StartColorFade(Color startColor)
    {
        if (colorFadeCoroutine != null)
            StopCoroutine(colorFadeCoroutine);
        colorFadeCoroutine = StartCoroutine(ColorFadeRoutineRealtime(startColor, Color.white, 0.3f));
    }

    // 시간 정지에도 동작하는 코루틴
    private IEnumerator ColorFadeRoutineRealtime(Color from, Color to, float duration)
    {
        float time = 0f;
        TrunBackgroundColor.color = from;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            TrunBackgroundColor.color = Color.Lerp(from, to, time / duration);
            yield return null;
        }
        TrunBackgroundColor.color = to;
    }
}
