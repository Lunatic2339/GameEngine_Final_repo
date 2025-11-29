using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public int damage = 10;
    
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
        if (isEnemyBullet)
        {
            // 플레이어만 때려야 함 (적끼리 팀킬 방지)
            if (collision.CompareTag("Player"))
            {
                Debug.Log("플레이어가 맞았습니다! (데미지: " + damage + ")");
                // collision.GetComponent<PlayerHealth>()?.TakeDamage(damage); // 나중에 추가
                Destroy(gameObject);
            }
        }
        // 3. [아군(플레이어) 총알]일 때의 로직
        else
        {
            // 적만 때려야 함 (자살 방지)
            if (collision.CompareTag("Enemy"))
            {
                Debug.Log("적이 맞았습니다! (데미지: " + damage + ")");
                // collision.GetComponent<EnemyHP>()?.TakeDamage(damage); // 나중에 추가
                Destroy(gameObject);
            }
        }
    }
}