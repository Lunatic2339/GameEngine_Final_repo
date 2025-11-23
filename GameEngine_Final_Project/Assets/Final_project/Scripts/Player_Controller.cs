using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 10.0f;
    private Animator animator;
    private Rigidbody2D rb;  // 새로 추가!
    
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();  // Rigidbody2D 가져오기
        
        // 디버그: 제대로 찾았는지 확인
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D가 없습니다! Player에 추가하세요.");
        }
    }
    
    void Update()
    {
        // 좌우 이동 변수
        float moveX = 0f;
        // Left Shift 키로 달리기 - 기존 속도에 2배 적용
        float runCoeff = Input.GetKey(KeyCode.LeftShift) ? 2.0f : 1.0f;

        // 좌우 방향키 입력 처리
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;  // 왼쪽
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;   // 오른쪽
        }
        // 플레이어 방향 전환
        if (moveX != 0)
        {
            transform.localScale = new Vector2(Mathf.Sign(moveX), 1);
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 점프 처리
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // rigidbody2D를 사용한 이동
        rb.linearVelocity = new Vector2(moveX * moveSpeed * runCoeff, rb.linearVelocity.y);
        
        
        // 애니메이션 제어
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("speed", currentSpeed);

        if (currentSpeed > 0f) Debug.Log("현재 속도: " + currentSpeed);


    }
}