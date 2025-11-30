using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 15.0f;
    public float originalSpeed;

    [Header("상태 체크")]
    public bool isGrounded = false;
    public bool isCrouching = false;
    public bool isOnWall = false;    
    public bool isWallJumping = false;
    public bool isUpShooting = false;

    [Header("벽타기 설정")]
    public float wallSlideSpeed = 5f; // 벽탈 때 내려가는 속도
    public float wallJumpDuration = 0.2f; // [추가됨] 벽 점프 후 이동 불가 시간

    [Header("웅크리기 설정 (Capsule & Bottom Pivot)")]
    private CapsuleCollider2D bodyCollider; 
    private Vector2 crouchSize = new Vector2(1.24f, 1.69f);
    private Vector2 crouchOffset = new Vector2(0.06f, 1.75f);
    private Vector2 originalSize;   
    private Vector2 originalOffset;

    [Header("애니메이션 설정")]
    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        originalSize = bodyCollider.size;
        originalOffset = bodyCollider.offset;
        originalSpeed = moveSpeed;
    }

    void Update()
    {

        // --- 이동 및 점프 로직 ---

        // 3. 벽타기 중일 때 (특수 조작)
        if (isOnWall && !isGrounded)
        {
            // A. 좌우 이동 불가 (X축 속도 0으로 고정)
            // B. 천천히 미끄러짐 (Y축 속도 wallSlideSpeed로 고정)
            rb.linearVelocity = new Vector2(0f, 0f);
            float facingDir = Mathf.Sign(transform.localScale.x);

            // D. 벽 점프 (벽에서도 점프 가능하게)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(WallJumpRoutine(facingDir));
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);
            }
        }
        // 4. 평소 상태 (일반 조작)
        else
        {
            if (isWallJumping) return; // 벽점프 중일 때는 조작 불가


            float moveX = 0f;
            if (Input.GetKey(KeyCode.DownArrow) && isGrounded &&!isOnWall)
            {
                isCrouching = true;
                animator.SetBool("isCrouching", true);
                // 콜라이더 크기 변경
                bodyCollider.size = crouchSize;
                bodyCollider.offset = crouchOffset;
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                isCrouching = false;
                animator.SetBool("isCrouching", false);
                // 콜라이더 크기 변경
                bodyCollider.size = originalSize;
                bodyCollider.offset = originalOffset;
            }
            if (Input.GetKey(KeyCode.LeftArrow) && !isCrouching && !isUpShooting) moveX = -1f;
            if (Input.GetKey(KeyCode.RightArrow) && !isCrouching && !isUpShooting) moveX = 1f;

            if(isGrounded && !isOnWall && Input.GetKey(KeyCode.UpArrow))
            {
                animator.SetBool("isUpShooting", true);
                isUpShooting = true;
            }
            else
            {
                animator.SetBool("isUpShooting", false);
                isUpShooting = false;
            }

            // 일반 점프 (땅에 있을 때만)
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger("Jump");
            }


            // 달리기
            rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

            // 방향 전환
            if (moveX != 0) transform.localScale = new Vector2(Mathf.Sign(moveX), 1f);

            // 달리기 애니메이션
            float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
            animator.SetFloat("Speed", currentSpeed);
        }
    }

    // ---------------------------------------------------------
    // 충돌 감지 (Collision) - 여기가 핵심입니다!
    // ---------------------------------------------------------
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // ★ 핵심 수정: 무조건 true가 아니라, "위쪽 방향" 충돌일 때만 인정!
            // GetContact(0)는 첫 번째 충돌 지점의 정보를 가져옵니다.
            ContactPoint2D contact = collision.GetContact(0);

            // normal.y가 0.7보다 크면 "확실히 내 발 밑에 있다"는 뜻입니다.
            // (완전 평지는 1.0, 약간의 경사면도 포함하기 위해 0.7 사용)
            if (contact.normal.y > 0.7f) 
            {
                isGrounded = true;
                animator.SetBool("isGrounded", true);
            }
        }


        // 벽 체크 (Tag로 확인)
        if (collision.gameObject.CompareTag("Wall"))
        {
            isOnWall = true;
            animator.SetBool("isOnWall", true);

            ContactPoint2D contact = collision.GetContact(0);
            
            // contact.normal.x > 0 이면 벽이 왼쪽
            // contact.normal.x < 0 이면 벽이 오른쪽
            
            if (contact.normal.x > 0.1f) 
            {
                // 벽이 왼쪽에 있음 -> 왼쪽 보기 (-1)
                transform.localScale = new Vector2(1, 1);
            }
            else if (contact.normal.x < -0.1f)
            {
                // 벽이 오른쪽에 있음 -> 오른쪽 보기 (1)
                transform.localScale = new Vector2(-1, 1);
            }
        }
    }

    // ★ OnCollisionStay2D 추가 추천!
    // 가끔 Enter가 씹히거나, 경사면을 타고 내려올 때 Ground 상태가 풀리는 걸 방지합니다.
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.normal.y > 0.7f) 
            {
                isGrounded = true;
                animator.SetBool("isGrounded", true);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // 땅에서 발 뗌
        if (collision.gameObject.CompareTag("Ground")) 
        {
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }

        // 벽에서 몸 뗌
        if (collision.gameObject.CompareTag("Wall")) 
        {
            isOnWall = false;
            animator.SetBool("isOnWall", false);
        }
    }


// ★ [추가됨] 벽 점프 코루틴
    IEnumerator WallJumpRoutine(float facingDir)
    {
        isWallJumping = true;  // 1. 일반 이동 차단
        isOnWall = false;      // 2. 벽에서 떨어짐 처리
        animator.SetBool("isOnWall", false);

        // 3. 반대 방향으로 힘을 빡! 줌
        // facingDir은 벽을 보고 있으므로, -facingDir이 반대 방향
        // X축: 이동속도보다 조금 더 멀리(1.5배) 튕겨나가게 설정
        rb.linearVelocity = new Vector2(facingDir * moveSpeed * 1.5f, jumpForce* 1.2f);
        
        
        animator.SetTrigger("Jump");

        // 5. 0.2초 동안 기다림 (이 동안은 Update의 이동 코드가 실행 안 됨 -> 관성 유지)
        yield return new WaitForSeconds(wallJumpDuration);

        isWallJumping = false; // 6. 다시 조작 가능
    }
}
