using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject tilePrefab; // 타일 프리팹
    public int gridWidth = 5;     // 그리드 너비
    public int gridHeight = 5;    // 그리드 높이
    public float tileSize = 1f;   // 타일 크기

    public GameObject[,] grid;  // 타일 배열

    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];
        CreateTileGrid();
    }

    // 타일 그리드 생성
    void CreateTileGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = new Vector3(x * tileSize,y * tileSize,0);
                grid[x, y] = Instantiate(tilePrefab, position, Quaternion.identity);
                grid[x, y].name = "Tile_" + x + "_" + y;
            }
        }
    }
}
