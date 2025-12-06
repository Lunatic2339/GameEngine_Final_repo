using UnityEngine;

public class TeleportTile : MonoBehaviour
{
    [Header("도착 지점 설정")]
    public Transform destination; // 이동할 위치 (빈 오브젝트)

    // 1. [Trigger 모드] 타일맵이 'Is Trigger'가 켜져 있을 때 (통과 가능)
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TeleportPlayer(collision.transform);
        }
    }

    // 2. [Collider 모드] 타일맵이 'Is Trigger'가 꺼져 있을 때 (벽/바닥처럼 밟힘)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TeleportPlayer(collision.transform);
        }
    }

    // 실제 이동 로직 (중복 제거)
    void TeleportPlayer(Transform playerTransform)
    {
        if (destination != null)
        {
            // 위치 이동
            playerTransform.position = destination.position;
            
            // (선택사항) 텔레포트 후 관성 제거 (낙하 속도 등 초기화)
            Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; 
            }

            Debug.Log("타일맵 텔레포트 성공!");
        }
        else
        {
            Debug.LogWarning("도착 지점(Destination)이 연결되지 않았습니다!");
        }
    }
}