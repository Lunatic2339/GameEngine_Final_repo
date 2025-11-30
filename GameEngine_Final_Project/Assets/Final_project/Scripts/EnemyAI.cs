using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("상태 설정")]
    public float moveSpeed = 2f;
    public float detectRange = 6f; 
    public float attackRange = 4f; 
    
    [Header("지형 체크")]
    public Transform groundCheckPos; // 앞쪽 바닥 확인용 위치

    [Header("공격 설정")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    private float nextFireTime = 0f;

    [Header("참조")]
    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;
    private bool isFacingRight = true; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange)
        {
            AttackState();
        }
        else 
        {
            PatrolState();
        }
    }

    // --- 행동 로직 ---

    void PatrolState()
    {
        // 1. 앞으로 이동
        rb.linearVelocity = new Vector2(transform.right.x * moveSpeed, rb.linearVelocity.y);

        // if (animator) animator.SetBool("IsMoving", true);

        // 2. 지형 체크 (Tag 방식)
        // 레이어 마스크 없이 일단 아래로 빔을 쏩니다.
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPos.position, Vector2.down, 1f);
        
        // [조건 1] 아무것도 안 맞았으면 (hit.collider == null) -> 낭떠러지 -> 턴!
        // [조건 2] 무언가 맞았는데, 그게 "Terrain"이 아니면 -> 땅 아님 -> 턴!
        if (hit.collider == null || !hit.collider.CompareTag("Terrain"))
        {
            Flip();
        }
    }

    void AttackState()
    {
        // 1. 멈춤
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        // if (animator) animator.SetBool("IsMoving", false);

        // 2. 플레이어 쪽 바라보기
        LookAtPlayer();

        // 3. 발사
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        if (animator) animator.SetTrigger("Attack");

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = transform.right * 5f; 
        }
    }

    void LookAtPlayer()
    {
        if (player.position.x > transform.position.x && !isFacingRight) Flip();
        else if (player.position.x < transform.position.x && isFacingRight) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (groundCheckPos != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(groundCheckPos.position, groundCheckPos.position + Vector3.down * 1f);
        }
    }
}