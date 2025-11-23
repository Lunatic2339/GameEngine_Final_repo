using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("설정")]
    public GameObject bulletPrefab; // ★중요: 여기에 프리팹을 넣으세요!
    public float attackInterval = 2f;
    public float bulletSpeed = 10f;

    [Header("상태 (자동 설정)")]
    public Transform playerTransform; // 플레이어 위치 저장용
    private float attackTimer;

    void Start()
    {
        attackTimer = 0f;
        
        // 1. 게임 시작할 때 플레이어를 딱 한 번만 찾아서 기억해둠 (캐싱)
        GameObject playerObj = GameObject.FindWithTag("Player"); 
        // 혹은 GameObject.Find("Player"); 도 되지만 Tag가 더 빠름
        
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("플레이어를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // 플레이어가 없으면 공격 로직 실행 안 함 (에러 방지)
        if (playerTransform == null) return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            Attack();
            attackTimer = 0f;
        }
    }

    void Attack()
    {
        // 2. 프리팹으로 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        
        // 3. 지역 변수 사용
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // 4. 방향 계산 (플레이어 위치 - 내 위치)
        Vector2 shootDirection = (playerTransform.position - transform.position).normalized; // Normalize() 대신 소문자 normalized 속성 추천

        // 5. 발사
        rb.linearVelocity = shootDirection * bulletSpeed;

        // 6. 삭제
        Destroy(bullet, 5f);
        
        Debug.Log("Enemy Fired at Player!");
    }
}