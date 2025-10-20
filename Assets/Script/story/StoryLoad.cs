using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class StoryLoad : StoryManager
{
    public static StoryLoad Instance;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // 기존 인스턴스가 있으면 자신을 파괴
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
