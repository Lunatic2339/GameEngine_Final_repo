using UnityEngine;
using System.Collections;

public class CannonTrap : MonoBehaviour
{
    [Header("발사 설정")]
    public GameObject bulletPrefab; // 총알 프리팹 (EnemyBullet 재사용 가능)
    public Transform firePoint;     // 총알 나가는 위치 (대포 주둥이)
    public float fireRate = 2f;     // 발사 간격 (초)
    public float startDelay = 0f;   // 시작 딜레이 (엇박자 발사 용도)
    
    [Header("총알 속도")]
    public Vector2 fireDirection = Vector2.left; // 날아갈 방향 (왼쪽: -1, 0 / 오른쪽: 1, 0)
    public float bulletSpeed = 5f;

    [Header("옵션")]
    private Animator animator; // 발사 애니메이션이 있다면 사용

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        // 1. 엇박자를 위해 처음에 잠깐 대기
        if (startDelay > 0)
            yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // 2. 발사!
            Shoot();

            // 3. 쿨타임 대기
            yield return new WaitForSeconds(fireRate);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 애니메이션 재생 (있으면)
        if (animator != null) animator.SetTrigger("Shoot");

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // 총알 방향 및 속도 설정
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 설정한 방향(fireDirection)으로 날리기
            // .normalized는 대각선이어도 속도를 일정하게 맞추기 위함
            rb.linearVelocity = fireDirection.normalized * bulletSpeed;
        }

        // (중요) 적 총알 프리팹에 Projectile 스크립트가 붙어있어야 데미지를 줍니다!
        // 회전 처리: 총알이 날아가는 방향을 보게 돌리기 (선택사항)
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // 에디터에서 발사 방향 미리보기
    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + (Vector3)fireDirection.normalized * 2f);
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}