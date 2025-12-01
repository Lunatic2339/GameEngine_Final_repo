using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 25.0f;
    [HideInInspector] public float originalSpeed;

    [Header("상태 체크")]
    public bool isGrounded = false;
    public bool isCrouching = false;
    public bool isOnWall = false;    
    public bool isWallJumping = false;
    public bool isUpShooting = false;
    public bool isCharging = false;
    public bool isDashing = false; // [대쉬] 현재 대쉬 중인가?
    
    private bool isJumping = false;

    [Header("능력 해금")]
    public bool unlockDoubleJump = false;
    private bool canDoubleJump = false; 

    // ★ [대쉬] 설정 변수들
    [Header("대쉬 설정")]
    public float dashSpeed = 20f;      // 대쉬 속도 (이동 속도의 3~4배 추천)
    public float dashDuration = 0.8f;  // 대쉬 지속 시간 (짧게!)
    public float dashCooldown = 3.0f;  // 지상 대쉬 쿨타임
    private bool canDash = true;       // 공중 대쉬 가능 여부 (땅 밟으면 리셋)
    private float lastDashTime = -100f;// 마지막 대쉬 시간 (쿨타임용)
    private float defaultGravity;      // 원래 중력 저장용

    [Header("벽타기 설정")]
    public float wallSlideSpeed = 2f; 
    public float wallFastSlideSpeed = 8f; 
    public float wallJumpDuration = 0.2f;

    [Header("웅크리기 설정")]
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
        
        originalSize = bodyCollider.size;
        originalOffset = bodyCollider.offset;
        originalSpeed = moveSpeed;
        
        defaultGravity = rb.gravityScale; // [대쉬] 원래 중력값 기억

        // ★ [추가됨] GameManager와 연동 (능력 & 위치)
        if (GameManager.instance != null)
        {
            // 1. 능력 불러오기
            unlockDoubleJump = GameManager.instance.hasDoubleJump;

            // 2. 체크포인트 위치로 이동 (저장된 적이 있다면)
            if (GameManager.instance.isCheckpointActive)
            {
                transform.position = GameManager.instance.lastCheckPointPos;
            }
        }
    }

    void Update()
    {
        // 1. 조작 완전 불가 상태 (벽 점프, 대쉬 중)
        // 대쉬 중일 때도 return을 해서 이동/점프/벽타기 로직을 다 무시해야 함
        if (isWallJumping || isDashing) return;

        // 2. 대쉬 입력 체크 (Z, X는 공격이니 C나 Shift 추천)
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            AttemptDash();
            if (isDashing) return;
        }

        // 3. 상태별 로직 실행
        if (isOnWall && !isGrounded)
        {
            HandleWallSlide();
        }
        else
        {
            HandleNormalMovement();
        }
    }

    // ★ [대쉬] 대쉬 시도 함수
    void AttemptDash()
    {
        // 웅크리기, 차징 중에는 대쉬 불가 (기획에 따라 변경 가능)
        if (isCrouching || isCharging) return;

        // A. 땅에 있을 때 (쿨타임 체크)
        if (isGrounded)
        {
            if (Time.time >= lastDashTime + dashCooldown)
            {
                StartCoroutine(DashRoutine());
            }
        }
        // B. 공중이거나 벽에 있을 때 (횟수 체크)
        else
        {
            if (canDash)
            {
                StartCoroutine(DashRoutine());
            }
        }
    }

    // ★ [대쉬] 실행 코루틴
    IEnumerator DashRoutine()
    {
        Debug.Log("대쉬!");
        isDashing = true;          // 다른 조작 잠금
        canDash = false;           // 공중 대쉬 기회 소모 (땅 밟아야 리필)
        lastDashTime = Time.time;  // 쿨타임 갱신
        
        // 1. 중력 0으로 만들기 (직선으로 날아가기 위해)
        rb.gravityScale = 0f;
        
        // 2. 속도 적용 (보는 방향으로 발사!)
        float facingDir = Mathf.Sign(transform.localScale.x);
        rb.linearVelocity = new Vector2(facingDir * dashSpeed, 0f);

        // 3. 애니메이션 (나중에 추가)
        // animator.SetTrigger("Dash"); 
        
        // 잔상 효과(Ghost Effect) 같은 게 있다면 여기서 Start

        // 4. 지속 시간 대기
        yield return new WaitForSeconds(dashDuration);

        // 5. 복구
        rb.gravityScale = defaultGravity; // 중력 복구
        rb.linearVelocity = Vector2.zero; // 속도 정지 (관성 없애기)
        
        isDashing = false; // 조작 잠금 해제
    }

    void HandleWallSlide()
    {
        bool isDown = Input.GetKey(KeyCode.DownArrow);
        float targetSpeed = isDown ? wallFastSlideSpeed : wallSlideSpeed;
        
        rb.linearVelocity = new Vector2(0f, -targetSpeed);
        
        float facingDir = Mathf.Sign(transform.localScale.x);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(WallJumpRoutine(facingDir));
        }
    }

    void HandleNormalMovement()
    {
        float moveX = 0f;

        if (!isCharging)
        {
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
            else if (isCrouching) 
            {
                if (!Input.GetKey(KeyCode.DownArrow) || !isGrounded)
                {
                    isCrouching = false;
                    animator.SetBool("isCrouching", false);
                    bodyCollider.size = originalSize;
                    bodyCollider.offset = originalOffset;
                }
            }

            if (Input.GetKey(KeyCode.LeftArrow) && !isCrouching && !isUpShooting) moveX = -1f;
            if (Input.GetKey(KeyCode.RightArrow) && !isCrouching && !isUpShooting) moveX = 1f;

            bool upInput = Input.GetKey(KeyCode.UpArrow);
            if (isGrounded && !isOnWall && upInput)
            {
                if (!isUpShooting) { isUpShooting = true; animator.SetBool("isUpShooting", true); }
            }
            else if (isUpShooting)
            {
                isUpShooting = false; animator.SetBool("isUpShooting", false);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isGrounded)
                {
                    PerformJump();
                    canDoubleJump = true;
                }
                else if (unlockDoubleJump && canDoubleJump && !isOnWall)
                {
                    PerformJump();
                    canDoubleJump = false;
                    Debug.Log("더블 점프!");
                }
            }
        }
        else 
        {
            moveX = 0f;
        }

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (moveX != 0) transform.localScale = new Vector2(Mathf.Sign(moveX), 1f);

        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    void PerformJump()
    {
        StartCoroutine(JumpCooldownRoutine());
    }

    IEnumerator JumpCooldownRoutine()
    {
        isJumping = true; 
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        animator.SetTrigger("Jump");
        
        isGrounded = false;
        isOnWall = false;
        animator.SetBool("isGrounded", false);
        animator.SetBool("isOnWall", false);

        yield return new WaitForSeconds(0.1f);
        isJumping = false;
    }

    IEnumerator WallJumpRoutine(float facingDir)
    {
        isWallJumping = true;  
        isOnWall = false;      
        animator.SetBool("isOnWall", false);

        // ★ [대쉬 연계] 벽 점프 하면 공중 대쉬 횟수도 리필해줄지? (보통 해줌)
        canDash = true; 
        canDoubleJump = true;

        rb.linearVelocity = new Vector2(facingDir * moveSpeed * 1.5f, jumpForce * 1.2f);
        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }

    void OnCollisionEnter2D(Collision2D collision) => EvaluateCollision(collision);
    void OnCollisionStay2D(Collision2D collision) => EvaluateCollision(collision);

    void EvaluateCollision(Collision2D collision)
    {
        if (isWallJumping || isJumping) return;

        if (collision.gameObject.CompareTag("Terrain"))
        {
            bool foundGround = false;
            bool foundWall = false;
            ContactPoint2D wallContact = new ContactPoint2D();

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                if (contact.normal.y > 0.7f) { foundGround = true; break; } 
                if (Mathf.Abs(contact.normal.x) > 0.7f) { foundWall = true; wallContact = contact; }
            }

            if (foundGround)
            {
                isGrounded = true;
                animator.SetBool("isGrounded", true);
                isOnWall = false;
                animator.SetBool("isOnWall", false);
                
                canDoubleJump = true; 
                canDash = true; // ★ [대쉬] 땅 밟으면 대쉬 리필!
                return;
            }

            if (foundWall && !isGrounded && rb.linearVelocity.y < 0.1f)
            {
                isOnWall = true;
                canDash = true; // ★ [대쉬] 벽에 닿아도 대쉬 리필!
                animator.SetBool("isOnWall", true);
                if (wallContact.normal.x > 0.1f) transform.localScale = new Vector2(1, 1); 
                else if (wallContact.normal.x < -0.1f) transform.localScale = new Vector2(-1, 1);  
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
}