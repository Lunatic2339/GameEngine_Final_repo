using UnityEngine;
using UnityEngine.Tilemaps;

public class TileToObject : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("검사할 타일맵")]
    public Tilemap targetTilemap;

    [Tooltip("바꿀 대상이 되는 타일 (에디터에서 찍은 그 타일)")]
    public TileBase tileToReplace;

    [Tooltip("생성할 떨어지는 발판 프리팹")]
    public GameObject fallingPlatformPrefab;

    void Start()
    {
        ReplaceTiles();
    }

    void ReplaceTiles()
    {
        // 타일맵의 범위(Bounds)를 가져옵니다.
        BoundsInt bounds = targetTilemap.cellBounds;

        // 타일맵 전체를 순회합니다.
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                // 현재 좌표의 타일 위치
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                
                // 현재 위치에 있는 타일 정보를 가져옵니다.
                TileBase currentTile = targetTilemap.GetTile(cellPos);

                // 만약 그 타일이 우리가 '떨어지는 발판'으로 지정한 타일이라면?
                if (currentTile == tileToReplace)
                {
                    // 1. 해당 위치의 타일을 지웁니다. (안 지우면 뒤에 그림이 남음)
                    targetTilemap.SetTile(cellPos, null);

                    // 2. 타일의 중심 좌표를 월드 좌표로 변환합니다.
                    Vector3 spawnPos = targetTilemap.GetCellCenterWorld(cellPos);

                    // 3. 그 위치에 프리팹을 생성합니다.
                    Instantiate(fallingPlatformPrefab, spawnPos, Quaternion.identity);
                }
            }
        }
    }
}