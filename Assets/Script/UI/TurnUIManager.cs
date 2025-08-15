using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnUIManager : MonoBehaviour
{
    public TurnManager turnManager; // TurnManager


    [Header("UI 텍스트 연결")]
    public TextMeshProUGUI trunCountText;  // 턴

    public GameObject Skill_Cancel_Button;
    public GameObject Skill_Start_Button;
    public GameObject Skill_TurnEnd_Button;


    public RectTransform AttackTurn;
    public RectTransform GurdTurn;

    public void Start()
    {
        UpdateReactTurn(true);
    }
    public void UpdateTrunCount(int turnCount)
    {
        trunCountText.text = turnCount.ToString() + "턴";
    }

    public void Updatenemytcount(int emeny)
    {

    }
    public void UpdateWaveCount(int wave)
    {

    }

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
        if (turnManager.isPlayerTurn)
        {
            StartCoroutine(MoveGurdTurnToZero());
            StartCoroutine(MoveAttackTurnTo150());
        }
        else
        {
            StartCoroutine(MoveAttackTurnToZero());
            StartCoroutine(MoveGurdTurnTo150());
        }
        UpdateButtonActive(playerturn);
    }

    public IEnumerator MoveAttackTurnToZero()
    {
        Vector3 startPos = AttackTurn.anchoredPosition;
        Vector3 targetPos = new Vector3(0, startPos.y, startPos.z);

        float duration = 0.8f; // 전체 이동 시간(초)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // t: 0~1, 빠르게 시작해서 느리게 끝나는 곡선
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            AttackTurn.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        AttackTurn.anchoredPosition = targetPos;
    }

    public IEnumerator MoveGurdTurnToZero()
    {
        Vector3 startPos = GurdTurn.anchoredPosition;
        Vector3 targetPos = new Vector3(0, startPos.y, startPos.z);

        float duration = 0.8f; // 전체 이동 시간(초)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // t: 0~1, 빠르게 시작해서 느리게 끝나는 곡선
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            GurdTurn.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        GurdTurn.anchoredPosition = targetPos;
    }

    public IEnumerator MoveAttackTurnTo150()
    {
        Vector3 startPos = AttackTurn.anchoredPosition;
        Vector3 targetPos = new Vector3(-150, startPos.y, startPos.z);

        float duration = 0.8f; // 전체 이동 시간(초)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // t: 0~1, 빠르게 시작해서 느리게 끝나는 곡선
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            AttackTurn.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        AttackTurn.anchoredPosition = targetPos;
    }

    public IEnumerator MoveGurdTurnTo150()
    {
        Vector3 startPos = GurdTurn.anchoredPosition;
        Vector3 targetPos = new Vector3(150, startPos.y, startPos.z);

        float duration = 0.8f; // 전체 이동 시간(초)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // t: 0~1, 빠르게 시작해서 느리게 끝나는 곡선
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            GurdTurn.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        GurdTurn.anchoredPosition = targetPos;
    }
}
