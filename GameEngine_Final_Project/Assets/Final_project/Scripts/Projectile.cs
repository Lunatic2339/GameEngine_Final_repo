using UnityEngine;

public class Projectile : MonoBehaviour // 여기 이름이 파일명과 같아야 함!
{
    [Header("투사체 설정")]
    public int damage = 10; // 밖에서 체리는 10, 왕체리는 50으로 조절 가능

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 플레이어에게 맞았을 때
        if (collision.CompareTag("Player"))
        {
            Debug.Log("으악! 데미지: " + damage);
            
            // 나중에 여기에 PlayerHP 깎는 코드를 넣으면 됩니다.
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            Destroy(gameObject); // 투사체 삭제
        }
        
        // 2. 땅(Ground)에 닿았을 때 (벽 뚫기 방지)
        // 레이어 이름이 "Ground"인 경우
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject); // 벽에 맞으면 사라짐
        }
    }
}