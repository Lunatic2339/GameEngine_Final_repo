using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 10.0f;
    private bool isGrounded = false;

    [Header("벽타기 설정")]
    public float wallSlideSpeed = 2f; // 벽탈 때 내려가는 속도
    private bool isOnWall = false;    // 벽에 붙어있는지 체크
    private bool isWallSliding = false; // 현재 벽타기 동작 중인지

    [Header("공격 설정")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public float attackDelay = 0.5f;
    public int attackDamage = 10;
    private bool isAttacking = false;

    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 공격 중이면 움직임 봉인
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // 2. 공격 입력
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        // --- 벽타기 상태 판단 로직 ---
        // 조건: 벽에 닿음(isOnWall) + 공중임(!isGrounded) + 떨어지는 중(속도 y < 0)
        // (올라가는 중에는 벽타기 모션이 나오면 안 되므로 y < 0 체크)
        if (isOnWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        // --- 이동 및 점프 로직 ---

        // 3. 벽타기 중일 때 (특수 조작)
        if (isWallSliding)
        {
            // A. 좌우 이동 불가 (X축 속도 0으로 고정)
            // B. 천천히 미끄러짐 (Y축 속도 wallSlideSpeed로 고정)
            rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);

            // C. 벽타기 애니메이션
            // animator.SetBool("IsWallSliding", true);

            // D. 벽 점프 (벽에서도 점프 가능하게)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 점프하는 순간 벽타기 해제 및 위로 튀어오름
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger("Jump");
            }
        }
        // 4. 평소 상태 (일반 조작)
        else
        {
            animator.SetBool("IsWallSliding", false);

            float moveX = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) moveX = 1f;

            // 일반 점프 (땅에 있을 때만)
            if (Input.GetKey(KeyCode.Space) && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger("Jump");
            }

            // 달리기
            float runCoeff = Input.GetKey(KeyCode.LeftShift) ? 2.0f : 1.0f;
            rb.linearVelocity = new Vector2(moveX * moveSpeed * runCoeff, rb.linearVelocity.y);

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
        // 땅 체크
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;

        // 벽 체크 (Tag로 확인)
        if (collision.gameObject.CompareTag("Wall")) isOnWall = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // 땅에서 발 뗌
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;

        // 벽에서 몸 뗌
        if (collision.gameObject.CompareTag("Wall")) isOnWall = false;
    }

    // ---------------------------------------------------------
    // 공격 코루틴 (기존 유지)
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");

        Vector2 point = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(point, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            // enemy.GetComponent<Enemy>()?.TakeDamage(attackDamage);
        }
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
    }
}