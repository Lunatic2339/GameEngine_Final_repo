using UnityEngine;
using System.Collections; // 코루틴 사용 필수

public class PlayerAttack : MonoBehaviour
{
    [Header("참조")]
    public PlayerMovement playerMovement;
    public Animator animator;

    [Header("발사 위치")]
    public Transform standFirePoint;
    public Transform crouchFirePoint;
    public Transform wallFirePoint;

    [Header("총알 설정")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 15f;
    public float fireRate = 0.2f;    // 연사 속도
    private float nextFireTime = 0f;

    [Header("애니메이션 설정")]
    public float shootHolsterTime = 0.5f; // 공격 후 자세 유지 시간 (1초)
    private Coroutine shootRoutine;       // 현재 돌아가는 타이머 저장용



    void Start()
    {
        // ★ 여기서 자동으로 찾아줍니다! (같은 Player 오브젝트에 붙어있으니까 가능)
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
            
        if (animator == null) animator = GetComponent<Animator>();
    }
    void Update()
    {
        // 공격 키 입력 (Z) & 연사 쿨타임 체크
        if ( (playerMovement.isGrounded || playerMovement.isOnWall) && Input.GetKey(KeyCode.Z) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // 1. 애니메이션 트리거 (자세 유지 시작)
        // 기존에 돌고 있던 '총 내리기 타이머'가 있다면 취소! (연사 중이라는 뜻)
        if (shootRoutine != null) 
            StopCoroutine(shootRoutine);

        // 새로운 타이머 시작
        shootRoutine = StartCoroutine(HolsterWeaponRoutine());


        // 2. 발사 위치 및 로직 (기존과 동일)
        Transform currentFirePoint;
        if (playerMovement.isCrouching)
        {
            currentFirePoint = crouchFirePoint;
        }
        else if (playerMovement.isOnWall)
        {
            currentFirePoint = wallFirePoint;
        }
        else
        {
            currentFirePoint = standFirePoint;
        }

        // 3. 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, currentFirePoint.position, currentFirePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        float facingDir = Mathf.Sign(transform.localScale.x);
        rb.linearVelocity = new Vector2(facingDir * bulletSpeed, 0f);
        bullet.transform.localScale = new Vector2(facingDir, 1f);
    }

    // ★ 자세 유지 코루틴 (핵심)
    IEnumerator HolsterWeaponRoutine()
    {
        // 1. 공격 자세 진입
        animator.SetBool("isShooting", true);

        // 2. 지정된 시간(1초)만큼 대기
        // (만약 이 도중에 또 Shoot()을 하면 이 코루틴은 강제 종료되고 다시 시작됨)
        yield return new WaitForSeconds(shootHolsterTime);

        // 3. 시간 다 됨 -> 자세 풀기
        animator.SetBool("isShooting", false);
        shootRoutine = null; // 초기화
    }
}