using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public int damage = 1;
    
    // ★ 핵심: 이게 체크되어 있으면 적의 총알, 꺼져 있으면 플레이어 총알
    public bool isEnemyBullet = false; 




    void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 벽에 닿으면 무조건 삭제
        if (collision.CompareTag("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        // 2. [적군 총알]일 때의 로직
        // 2. [적군 총알] -> 플레이어 피격 (★ 수정됨 ★)
        if (isEnemyBullet)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerHealth player = collision.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damage); // 데미지!
                }
                
                Destroy(gameObject); // 총알 삭제
            }
        }
        // 3. [아군(플레이어) 총알]일 때의 로직
        else
        {
            // 적만 때려야 함 (자살 방지)
            if (collision.CompareTag("Enemy"))
            {
                Debug.Log("적이 맞았습니다! (데미지: " + damage + ")");
                // 부딪힌 적에게서 EnemyHealth 스크립트를 찾아옵니다.
                EnemyHealth enemy = collision.GetComponent<EnemyHealth>();

                // 만약 스크립트가 있다면 (즉, 체력이 있는 적이라면)
                if (enemy != null)
                {
                    enemy.TakeDamage(damage); // 데미지 전달!
                }
                Destroy(gameObject);
            }
        }
    }
}