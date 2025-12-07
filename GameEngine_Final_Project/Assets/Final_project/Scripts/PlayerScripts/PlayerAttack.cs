using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("참조")]
    public PlayerMovement playerMovement;
    public Animator animator;

    // ==========================================
    // [1] 근접 공격 설정 (New!)
    // ==========================================
    [Header("근접 공격 (Z키)")]
    public Transform attackPoint;     // 칼 휘두르는 중심점 (빈 오브젝트)
    public float attackRange = 0.8f;  // 공격 범위 반지름
    public LayerMask targetLayers;     // 적 레이어 (Enemy만 때려야 함)
    public int meleeDamage = 1;       // 칼 데미지
    public float meleeRate = 0.3f;    // 칼질 속도 (연타 방지)
    private float nextMeleeTime = 0f;

    // ==========================================
    // [2] 원거리 공격 설정 (기존)
    // ==========================================
    [Header("원거리 발사 (X키)")]
    public Transform standFirePoint;
    public Transform crouchFirePoint;
    public Transform wallFirePoint;
    public Transform upFirePoint; 

    [Header("총알 설정")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 15f;
    public float fireRate = 0.2f;    
    private float nextFireTime = 0f;

    [Header("애니메이션 설정")]
    public float shootHolsterTime = 0.5f; 
    private Coroutine shootRoutine;    

    void Start()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // -----------------------------------------------------------
        // 1. 근접 공격 (Z키)
        // -----------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Z) && Time.time >= nextMeleeTime)
        {
            // 대쉬 중이 아닐 때만 공격 가능
            if (!playerMovement.isDashing)
            {
                PerformMeleeAttack();
                nextMeleeTime = Time.time + meleeRate;
            }
        }

        // -----------------------------------------------------------
        // 2. 원거리 발사 (X키) - 키를 X로 변경했습니다
        // -----------------------------------------------------------
        if (Input.GetKey(KeyCode.X) && Time.time >= nextFireTime)
        {
            // 땅에 있거나 벽에 붙어있을 때만 발사
            if (!playerMovement.isDashing)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    // ★ [신규] 근접 공격 함수
    void PerformMeleeAttack()
    {
        // 1. 애니메이션 실행 (Trigger 사용)
        if(animator != null) animator.SetTrigger("Attack");

        // 2. 범위 감지: attackPoint를 중심으로 원을 그려서 적을 찾음
        if (attackPoint == null) return;
            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, targetLayers);   
        // 3. 감지된 적에게 데미지 줌
        foreach (Collider2D obj in hitObjects)
        {
            // 1. 적인지 확인하고 때리기
            EnemyHealth eHealth = obj.GetComponent<EnemyHealth>();
            if (eHealth != null)
            {
                eHealth.TakeDamage(meleeDamage);
            }

            // 2. ★ [추가된 부분] 부서지는 장애물인지 확인하고 부수기
            DestructibleObject obstacle = obj.GetComponent<DestructibleObject>();
            if (obstacle != null)
            {
                obstacle.Break();
                Debug.Log("장애물 파괴!");
            }
        }
    }

    // [기존] 원거리 발사 함수
    void Shoot()
    {
        if (shootRoutine != null) StopCoroutine(shootRoutine);
        shootRoutine = StartCoroutine(HolsterWeaponRoutine());

        Transform currentFirePoint;

        if (playerMovement.isOnWall) currentFirePoint = wallFirePoint;
        else if (playerMovement.isUpShooting) currentFirePoint = upFirePoint;
        else if (playerMovement.isCrouching) currentFirePoint = crouchFirePoint;
        else currentFirePoint = standFirePoint;

        if (bulletPrefab != null && currentFirePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, currentFirePoint.position, currentFirePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            
            float facingDir = Mathf.Sign(transform.localScale.x);

            if (playerMovement.isUpShooting)
            {
                rb.linearVelocity = new Vector2(0f, bulletSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector2(facingDir * bulletSpeed, 0f);
                bullet.transform.localScale = new Vector2(facingDir, 1f); 
            }
        }
    }

    IEnumerator HolsterWeaponRoutine()
    {
        animator.SetBool("isShooting", true);
        yield return new WaitForSeconds(shootHolsterTime);
        animator.SetBool("isShooting", false);
        shootRoutine = null;
    }

    // ★ 에디터에서 공격 범위 눈으로 보기
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}