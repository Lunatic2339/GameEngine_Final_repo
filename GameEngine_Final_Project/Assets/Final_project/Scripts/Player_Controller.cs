using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 10.0f;
    private bool isGrounded = false;
    private Animator animator;
    private Rigidbody2D rb;  // 새로 추가!



    void OnCollisionEnter2D(Collision2D collision)
    {
        if  (collision.gameObject.CompareTag("Ground"))
        {  
            Debug.Log("충돌 시작: " + collision.gameObject.name);
            isGrounded = true;
        }
        

    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if  (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("충돌 종료: " + collision.gameObject.name);
            isGrounded = false;
        }

    }
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
        // 입력 감지
        float moveX = 0f;
        
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1f;  // 왼쪽
        if (Input.GetKey(KeyCode.RightArrow)) moveX = 1f;   // 오른쪽
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // 점프
            animator.SetTrigger("Jump");
        }

        // 달리기 보정
        float runCoeff = Input.GetKey(KeyCode.LeftShift) ? 2.0f : 1.0f;
        // 물리 기반 이동 (새로운 방식!)
        rb.linearVelocity = new Vector2(moveX * moveSpeed * runCoeff, rb.linearVelocity.y);
        // 캐릭터 방향 전환
        if (moveX != 0) transform.localScale = new Vector2(Mathf.Sign(moveX), 1f);
        // 애니메이션 제어
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", currentSpeed);



    }
}