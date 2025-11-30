using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 15.0f;
    [HideInInspector] public float originalSpeed; // 인스펙터에선 숨김 (수정 불필요)

    [Header("상태 체크 (디버깅용)")]
    public bool isGrounded = false;
    public bool isCrouching = false;
    public bool isOnWall = false;    
    public bool isWallJumping = false;
    public bool isUpShooting = false;

    [Header("벽타기 설정")]
    public float wallSlideSpeed = 2f; // 기본 미끄러짐 속도
    public float wallFastSlideSpeed = 8f; // 아래키 눌렀을 때 속도 [추가됨]
    public float wallJumpDuration = 0.2f;

    [Header("웅크리기 설정")]
    // [SerializeField]를 쓰면 private이어도 인스펙터에서 수정 가능!
    [SerializeField] private Vector2 crouchSize = new Vector2(1.24f, 1.69f);
    [SerializeField] private Vector2 crouchOffset = new Vector2(0.06f, 1.75f);
    
    private CapsuleCollider2D bodyCollider; 
    private Vector2 originalSize;   
    private Vector2 originalOffset;

    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        
        // 초기값 저장
        originalSize = bodyCollider.size;
        originalOffset = bodyCollider.offset;
        originalSpeed = moveSpeed;
    }

    void Update()
    {
        // 1. 벽 점프 중이면 조작 불가 (관성 유지)
        if (isWallJumping) return;

        // 2. 벽타기 로직 (공중 + 벽 + 떨어지는 중)
        // (Update에서 isOnWall을 신뢰하되, 물리적 조건은 Collision에서 처리)
        if (isOnWall && !isGrounded)
        {
            HandleWallSlide();
        }
        // 3. 평소 상태 (일반 이동)
        else
        {
            HandleNormalMovement();
        }
    }

    // --- 기능별 함수 분리 (가독성 향상) ---

    void HandleWallSlide()
    {
        bool isDown = Input.GetKey(KeyCode.DownArrow);
        
        // 아래키 누르면 빠르게, 아니면 천천히 미끄러짐
        float targetSpeed = isDown ? wallFastSlideSpeed : wallSlideSpeed;
        
        // 벽타기 시 X축 고정, Y축은 미끄러짐 속도 적용
        rb.linearVelocity = new Vector2(0f, -targetSpeed);
        
        float facingDir = Mathf.Sign(transform.localScale.x);

        // 벽 점프
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(WallJumpRoutine(facingDir));
        }
    }

    void HandleNormalMovement()
    {
        float moveX = 0f;

        // 웅크리기 입력 처리
        if (isGrounded && !isOnWall && Input.GetKey(KeyCode.DownArrow))
        {
            if (!isCrouching)
            {
                isCrouching = true;
                animator.SetBool("isCrouching", true);
                bodyCollider.size = crouchSize;
                bodyCollider.offset = crouchOffset;
            }
        }
        // 웅크리기 해제 (키를 뗐거나 공중이거나)
        else if (isCrouching) 
        {
            isCrouching = false;
            animator.SetBool("isCrouching", false);
            bodyCollider.size = originalSize;
            bodyCollider.offset = originalOffset;
        }

        // 좌우 이동 (앉기, 위보기 중엔 이동 불가)
        if (Input.GetKey(KeyCode.LeftArrow) && !isCrouching && !isUpShooting) moveX = -1f;
        if (Input.GetKey(KeyCode.RightArrow) && !isCrouching && !isUpShooting) moveX = 1f;

        // 위 조준
        bool upInput = Input.GetKey(KeyCode.UpArrow);
        if (isGrounded && !isOnWall && upInput)
        {
            if (!isUpShooting)
            {
                isUpShooting = true;
                animator.SetBool("isUpShooting", true);
            }
        }
        else if (isUpShooting && (!upInput || !isGrounded))
        {
            isUpShooting = false;
            animator.SetBool("isUpShooting", false);
        }

        // 일반 점프
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }

        // 이동 적용
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // 방향 전환
        if (moveX != 0) transform.localScale = new Vector2(Mathf.Sign(moveX), 1f);

        // 애니메이션 Speed
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    // ---------------------------------------------------------
    // 충돌 감지 (중복 코드 제거 최적화)
    // ---------------------------------------------------------
    
void OnCollisionEnter2D(Collision2D collision) => EvaluateCollision(collision);
    void OnCollisionStay2D(Collision2D collision) => EvaluateCollision(collision);

    void EvaluateCollision(Collision2D collision)
    {
        if (isWallJumping) return;

        if (collision.gameObject.CompareTag("Terrain"))
        {
            // 임시 변수: 이번 충돌에서 땅이나 벽을 찾았는지 체크
            bool foundGround = false;
            bool foundWall = false;
            ContactPoint2D wallContact = new ContactPoint2D(); // 벽 방향 계산용

            // ★ 핵심 수정: 모든 접촉점을 다 뒤져봅니다!
            // GetContact(0)만 쓰면 재수 없게 벽이 먼저 걸릴 수 있음.
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);

                // 1. 땅을 발견했는가?
                if (contact.normal.y > 0.7f)
                {
                    foundGround = true;
                    break; // 땅을 찾았으면 더 볼 것도 없음 (땅이 최우선)
                }
                
                // 2. 벽을 발견했는가? (아직 땅을 못 찾았을 때만 의미 있음)
                if (Mathf.Abs(contact.normal.x) > 0.7f)
                {
                    foundWall = true;
                    wallContact = contact; // 나중에 방향 계산을 위해 저장
                }
            }

            // --- 판정 결과 적용 ---

            // A. 땅이 하나라도 있었다면 -> 무조건 Ground 상태
            if (foundGround)
            {
                isGrounded = true;
                animator.SetBool("isGrounded", true);
                
                isOnWall = false;
                animator.SetBool("isOnWall", false);
                return; // 벽 판정 무시하고 종료
            }

            // B. 땅은 없고 벽만 있으며, 공중이고, 떨어지는 중이라면 -> 벽타기
            if (foundWall && !isGrounded && rb.linearVelocity.y < 0.1f)
            {
                isOnWall = true;
                animator.SetBool("isOnWall", true);

                // 아까 저장해둔 벽의 정보로 방향 전환
                if (wallContact.normal.x > 0.1f) 
                    transform.localScale = new Vector2(1, 1); 
                else if (wallContact.normal.x < -0.1f)
                    transform.localScale = new Vector2(-1, 1);  
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            isGrounded = false;
            isOnWall = false;
            
            animator.SetBool("isGrounded", false);
            animator.SetBool("isOnWall", false);
        }
    }

    IEnumerator WallJumpRoutine(float facingDir)
    {
        isWallJumping = true;  
        isOnWall = false;      
        animator.SetBool("isOnWall", false);

        // 점프 방향 (벽 반대편으로)
        rb.linearVelocity = new Vector2(facingDir * moveSpeed * 1.5f, jumpForce * 1.2f);
        
        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(wallJumpDuration);

        isWallJumping = false;
    }
}