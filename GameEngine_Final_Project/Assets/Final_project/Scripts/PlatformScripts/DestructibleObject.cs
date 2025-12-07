using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("파괴 효과")]
    public GameObject destroyEffect; // 부서질 때 튀는 파편 이펙트 (선택사항)
    public float destroyDelay = 0f;  // 파괴 딜레이 (바로 사라질지)

    // 외부에서 이 함수를 부르면 깨짐
    public void Break()
    {
        // 1. 파편 이펙트 생성
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // 2. 소리 재생 (SoundManager가 있다면 추가)
        // AudioManager.instance.PlaySfx("BoxBreak");

        // 3. 오브젝트 삭제
        Destroy(gameObject, destroyDelay);
    }
}