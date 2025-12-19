using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("파괴 효과 설정")]
    public GameObject destroyEffect; // 부서질 때 나타날 파편 이펙트
    public float destroyDelay = 0f;  // 삭제 딜레이

    // 외부(총알 스크립트 등)에서 직접 호출할 때 사용하는 함수
    public void Break()
    {
        ExecuteDestruction();
    }

    /// <summary>
    /// [중요] 총알과 부딪혔을 때 자동으로 파괴되는 로직
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 부딪힌 물체의 태그가 "Bullet"인지 확인 (태그를 Bullet으로 설정해주세요)
        // 2. 혹은 물체의 레이어가 총알 레이어인지 확인
        if (collision.CompareTag("Bullet"))
        {
            ExecuteDestruction();

            // 총알도 같이 없애고 싶다면 아래 주석 해제
            // Destroy(collision.gameObject);
        }
    }

    // 실제 파괴 처리 (이펙트 생성 및 오브젝트 삭제)
    private void ExecuteDestruction()
    {
        // 파편 이펙트가 있다면 생성
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // 자기 자신 삭제
        Destroy(gameObject, destroyDelay);
    }
}