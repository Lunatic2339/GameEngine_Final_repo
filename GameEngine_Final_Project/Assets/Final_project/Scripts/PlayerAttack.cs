using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("참조")]
    public PlayerMovement playerMovement;
    public Animator animator;

    [Header("발사 위치")]
    public Transform standFirePoint;
    public Transform crouchFirePoint;
    public Transform wallFirePoint;
    public Transform upFirePoint; // ★ 중요: Rotation Z를 90도로 돌려놓으세요!

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
        // 공격 입력 (Z) & 연사 쿨타임 체크
        if (Input.GetKeyDown(KeyCode.Z) && Time.time >= nextFireTime)
        {
            // ★ 핵심 조건: 땅에 있거나 OR 벽에 붙어있을 때만 발사 가능
            // (즉, 점프 중이거나 떨어지는 중에는 발사 불가)
            if (playerMovement.isGrounded || playerMovement.isOnWall)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void Shoot()
    {
        // 1. 자세 유지 코루틴 (애니메이션)
        if (shootRoutine != null) StopCoroutine(shootRoutine);
        shootRoutine = StartCoroutine(HolsterWeaponRoutine());

        // 2. 발사 위치(FirePoint) 선정
        Transform currentFirePoint;

        // 우선순위: 벽타기 > 위로 쏘기 > 앉아 쏘기 > 서서 쏘기
        if (playerMovement.isOnWall)
        {
            currentFirePoint = wallFirePoint;
            // 벽타기 중 사격 애니메이션이 따로 없다면, 여기서 트리거를 추가하거나
            // 기본 Shoot 애니메이션이 재생되도록 둡니다.
        }
        else if (playerMovement.isUpShooting) // 위로 쏘기
        {
            currentFirePoint = upFirePoint;
            // Animator 파라미터가 isUpShooting이면 애니메이터가 알아서 위를 보고 있을 것임
        }
        else if (playerMovement.isCrouching) // 앉아 쏘기
        {
            currentFirePoint = crouchFirePoint;
        }
        else // 서서 쏘기
        {
            currentFirePoint = standFirePoint;
        }

        // 3. 총알 생성
        // (Rotation은 FirePoint의 회전값을 그대로 따라갑니다. UpPoint가 90도 돌아가 있어야 함)
        GameObject bullet = Instantiate(bulletPrefab, currentFirePoint.position, currentFirePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // 4. 총알 속도 및 방향 설정
        float facingDir = Mathf.Sign(transform.localScale.x);

        // 위로 쏠 때
        if (playerMovement.isUpShooting)
        {
            rb.linearVelocity = new Vector2(0f, bulletSpeed);
            // 위로 쏘는 총알은 좌우 반전이 필요 없으므로 스케일 조정 생략 가능
            // (단, 총알 그림에 좌우가 있다면 y축 회전이 필요할 수도 있음)
        }
        // 정면(혹은 벽 반대)으로 쏠 때
        else
        {
            rb.linearVelocity = new Vector2(facingDir * bulletSpeed, 0f);
            
            // 총알 그림도 캐릭터 방향에 맞춰 뒤집기
            bullet.transform.localScale = new Vector2(facingDir, 1f); 
        }
    }

    IEnumerator HolsterWeaponRoutine()
    {
        animator.SetBool("isShooting", true);
        yield return new WaitForSeconds(shootHolsterTime);
        animator.SetBool("isShooting", false);
        shootRoutine = null;
    }
}