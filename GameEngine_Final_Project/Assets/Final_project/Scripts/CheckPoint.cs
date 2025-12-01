using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("설정")]
    public Sprite activeSprite; // 밟았을 때 바뀔 이미지 (깃발 올라감 등)
    private bool isActivated = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            isActivated = true; // 중복 발동 방지

            // 1. 매니저에게 내 위치 기억하라고 명령
            if (GameManager.instance != null)
            {
                GameManager.instance.UpdateCheckpoint(transform.position);
            }

            // 2. 이미지 변경 (활성화 표시)
            if (activeSprite != null)
            {
                sr.sprite = activeSprite;
            }
            
            // 사운드나 파티클 효과 추가 가능
            Debug.Log("체크포인트 활성화!");
        }
    }
}