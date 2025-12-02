using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public int damage = 1;
    private Animator animator;
    // true: 적 총알(플레이어 공격), false: 플레이어 총알(적 공격)
    public bool isEnemyBullet = false; 

    // ★ [추가됨] 총알 수명 (기본 3초)
    [Header("수명 설정 (자동 파괴)")]
    public float lifeTime = 5.0f; 

    void Start()
    {
        animator = GetComponent<Animator>();
        // ★ [추가됨] 태어나자마자 "3초 뒤에 나를 파괴해라"라고 예약
        // 이렇게 하면 허공으로 날아가도 3초 뒤에 알아서 사라집니다. (메모리 누수 해결!)
        Destroy(gameObject, lifeTime);
        if (isEnemyBullet)
        {
            animator.SetBool("isPlayer", false);
        }
        else
        {
            animator.SetBool("isPlayer", true);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 지형(Terrain)에 닿으면 무조건 삭제
        // (아까 PlayerMovement에서 땅, 벽 태그를 모두 "Terrain"으로 통일했으므로 이걸로 체크합니다)
        if (collision.CompareTag("Terrain"))
        {
            Destroy(gameObject);
            return;
        }

        // 2. [적군 총알]일 때 -> 플레이어 피격
        if (isEnemyBullet)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerHealth player = collision.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damage, transform); // 플레이어 체력 깎기
                }
                
                Destroy(gameObject); // 총알 삭제
            }
        }
        // 3. [아군(플레이어) 총알]일 때 -> 적 피격
        else
        {
            // 적만 때려야 함 (자살 방지)
            if (collision.CompareTag("Enemy"))
            {
                Debug.Log("적이 맞았습니다! (데미지: " + damage + ")");
                
                EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage); // 적 체력 깎기
                }
                
                Destroy(gameObject); // 총알 삭제
            }
        }
    }
}