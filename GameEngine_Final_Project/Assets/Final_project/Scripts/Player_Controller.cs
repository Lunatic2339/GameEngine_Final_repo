using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 10.0f;
    private bool isGrounded = false;

    [Header("공격 설정")]
    public Transform attackPoint;    // 플레이어 자식으로 만든 빈 오브젝트 연결
    public float attackRange = 0.5f; // 공격 범위
    public LayerMask enemyLayers;    // 적 레이어
    public float attackDelay = 0.5f; // 공격 동작 시간 (이 시간 동안 이동 불가)
    public int attackDamage = 10;
    private bool isAttacking = false; // 현재 공격 중인지 확인하는 플래그

    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null) Debug.LogError("Rigidbody2D가 없습니다!");
        if (attackPoint == null) Debug.LogWarning("AttackPoint가 연결되지 않았습니다! 인스펙터에서 할당해주세요.");
    }

    void Update()
    {
        // 1. 공격 중이라면 이동/점프 입력을 받지 않고, 제자리에서 멈춤
        if (isAttacking)
        {
            // 공격 중 미끄러짐 방지 (X축 속도 0으로, Y축은 중력 유지)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return; 
        }

        // 2. 공격 입력 감지 (Z키)
        // 땅에 있을 때만 공격 가능하게 하려면 && isGrounded 추가
        if (Input.GetKeyDown(KeyCode.A)) 
        {
            StartCoroutine(AttackRoutine());
            return; // 공격 시작했으면 아래 이동 로직 실행 안 함
        }

        // --- 기존 이동 로직 ---
        float moveX = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveX = 1f;

        // 점프
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }

        // 달리기 보정
        float runCoeff = Input.GetKey(KeyCode.LeftShift) ? 2.0f : 1.0f;

        // 물리 기반 이동
        rb.linearVelocity = new Vector2(moveX * moveSpeed * runCoeff, rb.linearVelocity.y);

        // 캐릭터 방향 전환 (좌우 반전)
        if (moveX != 0) transform.localScale = new Vector2(Mathf.Sign(moveX), 1f);

        // 애니메이션 제어
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", currentSpeed);
    }

    // 공격 로직을 처리하는 코루틴
    IEnumerator AttackRoutine()
    {
        isAttacking = true; // 이동 잠금

        // 1. 애니메이션 실행
        // animator.SetTrigger("Attack");

        // 2. 물리 판정 (범위 내 적 감지)
        // AttackPoint가 없으면 현재 위치 기준으로 함 (에러 방지)
        Vector2 point = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(point, attackRange, enemyLayers);

        // 3. 데미지 처리
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log(enemy.name + " 때림!");
            // 적 스크립트가 있다면 여기서 함수 호출
            // enemy.GetComponent<Enemy>()?.TakeDamage(attackDamage);
        }

        // 4. 딜레이 (공격 모션이 끝날 때까지 대기)
        // 애니메이션 길이와 비슷하게 맞춰주세요 (예: 0.5초)
        yield return new WaitForSeconds(attackDelay);

        isAttacking = false; // 이동 잠금 해제
    }

    // 에디터에서 공격 범위 확인용
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    // --- 바닥 충돌 체크 (기존 코드 유지) ---
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // 총알과 충돌 시 처리 (예: 데미지 받기)
            Debug.Log("플레이어가 총알에 맞았습니다!");
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}