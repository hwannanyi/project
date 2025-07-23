using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;
    public Stage[] ALLStageList; // 전체 스테이지 리스트
    public Stage CurrentStage; // 현재 선택된 스테이지
    public List<string> character;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Addressables.LoadAssetsAsync<Stage>("Stage", null).Completed += OnStageLoaded;
    }

    private void OnStageLoaded(AsyncOperationHandle<IList<Stage>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            ALLStageList = handle.Result.OrderBy(stage => stage.stagenumber).ToArray();
        }
        else
        {
            Debug.LogError("Addressable Assets에서 Stage Scriptable Object를 로드하는 데 실패했습니다.");
        }
        
    }
    // Update is called once per frame
    public void StageSelection(int i)
    {
        if (ALLStageList == null)
        {
            return;
        }

        // stagenumber가 i와 같은 Stage를 찾음
        Stage foundStage = ALLStageList.FirstOrDefault(stage => stage.stagenumber == i);
        if (foundStage != null)
        {
            CurrentStage = foundStage;
        }
        else
        {
            Debug.LogWarning($"stagenumber가 {i}인 스테이지를 찾을 수 없습니다.");
        }
    }
}
