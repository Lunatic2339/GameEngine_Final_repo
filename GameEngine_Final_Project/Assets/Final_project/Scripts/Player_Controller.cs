using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5.0f;
    
    private Animator animator;
    private Rigidbody2D rb;  // 새로 추가!
    
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();  // Rigidbody2D 가져오기
        
        // 디버그: 제대로 찾았는지 확인
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D가 없습니다! Player에 추가하세요.");
        }
    }
    
    void Update()
    {
        // 입력 감지
        float moveX = 0f;
        
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;  // 왼쪽
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;   // 오른쪽
        }
        
        // 물리 기반 이동 (새로운 방식!)
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        
        // 애니메이션 제어
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", currentSpeed);
    }
}