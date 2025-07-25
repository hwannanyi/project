
using UnityEngine;
using UnityEngine.SceneManagement;
public class StageSelection : MonoBehaviour
{
public void StageSelectionIndex(int i)
    {
        if (StageManager.Instance == null || StageManager.Instance.ALLStageList == null)
        {
            Debug.LogWarning("StageManager 또는 ALLStageList가 초기화되지 않았습니다.");
            return;
        }

        // stagenumber가 i와 같은 Stage를 찾음
        StageManager.Instance.StageSelection(i);
        
        if (StageManager.Instance.CurrentStage != null)
        {
            Debug.Log($"선택된 스테이지: {StageManager.Instance.CurrentStage.stagenumber}");
        }
        else
        {
            Debug.LogWarning($"stagenumber가 {i}인 스테이지를 찾을 수 없습니다.");
        }
    }
}
