using System;
using Unity.VisualScripting;
using UnityEngine;

public class MapMaker : MonoBehaviour
{

    public GameObject tilePrefab; // 타일 프리팹
    public GameObject wallPrefab; // 벽 프리팹
    public GameObject mapBorder; // 맵 테두리 프리팹
    public GameObject Map; // 맵 오브젝트
    public Transform cameraTra; // 맵 오브젝트

    public int width = 5; // 맵의 가로 크기
    public int height = 5; // 맵의 세로 크기

    public StageDataManager stageDataManager;


    public void Start()
    {
        Sprite sprite = stageDataManager.CurrentStage.mapSprite;
        Sprite background = stageDataManager.CurrentStage.mapBackGround;
        int w = stageDataManager.CurrentStage.mapWidth;
        int y = stageDataManager.CurrentStage.mapHeight;
        Mapcreate(sprite, background, w, y);
    }



    public void Mapcreate(Sprite tile, Sprite background, int w, int h)
    {
        // 벽 프리팹에서 SpriteRenderer를 미리 가져옴
        Sprite wallSprite = null;
        if (wallPrefab.TryGetComponent<SpriteRenderer>(out var wallSr))
        {
            wallSprite = wallSr.sprite;
        }

        for (int x = -1; x <= w; x++)
        {
            for (int z = -1; z <= h; z++)
            {
                Vector3 position = new Vector3(x * 1.2f, 0, z);
                // 벽 생성
                if (x == -1 || x == w || z == -1 || z == h)
                {
                    GameObject tileWall = Instantiate(wallPrefab, position, Quaternion.Euler(90f, 0f, 0f), Map.transform);
                    SpriteRenderer sr = tileWall.GetComponent<SpriteRenderer>();
                    sr.sprite = wallSprite;
                }
                else
                {
                    GameObject tileMap = Instantiate(tilePrefab, position, Quaternion.Euler(90f, 0f, 0f), Map.transform);
                    SpriteRenderer sr = tileMap.GetComponent<SpriteRenderer>();
                    sr.sprite = tile;
                }

            }
        }

        cameraTra.position = new Vector3(w * 1.2f / 2f - 0.5f, 6,0);
    }
}
