using UnityEngine;

public class JumpTile : MonoBehaviour
{
    [Header("점프 설정")]
    [Tooltip("점프하는 힘의 크기입니다.")]
    public float jumpForce = 20f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // 중요: 기존의 떨어지는 속도를 0으로 초기화해야 
                // 떨어지는 속도에 상관없이 항상 일정한 높이로 튕겨 올라갑니다.
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

                // 위쪽 방향으로 순간적인 힘(Impulse)을 가함
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                
                // (선택 사항) 점프 시 로그 출력
                // Debug.Log("점프 타일 작동!");
            }
        }
    }
}