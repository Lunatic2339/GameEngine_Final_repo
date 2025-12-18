using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

public class FallingPlatform : MonoBehaviour
{
    [Header("시간 설정")]
    [Tooltip("밟고 나서 떨어지기까지 걸리는 시간 (0이면 바로 떨어짐)")]
    public float fallDelay = 0.5f;
    
    [Tooltip("떨어진 후 다시 생성되기까지 걸리는 시간")]
    public float respawnDelay = 3.0f;

    [Header("물리 설정")]
    [Tooltip("떨어질 때의 중력 가중치 (클수록 빨리 떨어짐)")]
    public float fallGravity = 2f;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        
        // 시작할 때는 공중에 떠 있어야 하므로 Kinematic으로 설정
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어가 닿았고, 이미 떨어지는 중이 아니라면 작동
        if (collision.gameObject.CompareTag("Player") && !isFalling)
        {
            // 플레이어가 발판 위에서 밟았는지 확인 (선택사항, 보통 위에서 밟을 때만 작동)
            if (collision.contacts[0].normal.y < 0) 
            {
                StartCoroutine(FallRoutine());
            }
        }
    }

    IEnumerator FallRoutine()
    {
        isFalling = true;

        // 1. 떨어지기 전 대기 (덜덜 떨리는 연출 등을 넣을 수 있는 타이밍)
        yield return new WaitForSeconds(fallDelay);

        // 2. 물리 켜기 (떨어지기 시작)
        rb.bodyType = RigidbodyType2D.Dynamic; // 중력 영향 받게 변경
        rb.gravityScale = fallGravity;

        // 3. 재생성 대기 시간만큼 기다림
        yield return new WaitForSeconds(respawnDelay);

        // 4. 원상 복구 (재생성)
        ResetPlatform();
    }

    void ResetPlatform()
    {
        // 속도와 회전 초기화
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 다시 고정 상태로 변경
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // 위치를 원래대로
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        isFalling = false;
    }
}