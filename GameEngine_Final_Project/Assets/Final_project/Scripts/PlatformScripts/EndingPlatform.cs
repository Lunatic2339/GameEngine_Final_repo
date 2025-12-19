using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class EndingPlatform : MonoBehaviour
{
    [Header("설정")]
    public float moveSpeed = 1.0f;
    public string endingSceneName = "EndingScene"; 
    public Image fadePanel; 

    private bool isActivated = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어이고, 아직 발동 안 했으면
        if (collision.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            Animator playerAnim = collision.GetComponent<Animator>();

            if (playerMovement != null)
            {
                // 1. 조작 및 이동 스크립트 잠금
                playerMovement.isCutscene = true; 
                playerMovement.enabled = false; // 아예 스크립트를 꺼서 간섭 차단
            }

            if (playerRb != null)
            {
                // 2. ★ [핵심] 물리 효과 완전히 끄기 (떨림 원인 제거)
                playerRb.linearVelocity = Vector2.zero;      // 속도 초기화
                playerRb.bodyType = RigidbodyType2D.Kinematic;         // 물리력 무시 (Unity 6 이전: isKinematic, 최신: bodyType = Kinematic)
                playerRb.simulated = false;            // ★ 물리 연산 자체를 중단 (충돌 감지 X)
            }

            // 3. 발판의 자식으로 만들기 (같이 움직이게)
            collision.transform.SetParent(this.transform);

            // 4. 애니메이션 정리 (가만히 서 있는 상태로)
            if (playerAnim != null)
            {
                playerAnim.SetFloat("Speed", 0);
                playerAnim.SetBool("isGrounded", true);
                playerAnim.Play("Player_Idle"); // 강제로 Idle 재생
            }
            

            StartCoroutine(EndingSequence());
        }
    }

    void FixedUpdate()
    {
        if (isActivated)
        {
            // FixedUpdate 안에서도 Time.deltaTime을 써야 속도가 정상입니다.
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        }
    }

    IEnumerator EndingSequence()
    {
        yield return new WaitForSeconds(5f); // 2초 상승

        // 페이드 아웃 효과
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            float alpha = 0;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime;
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        SceneManager.LoadScene(endingSceneName);
    }
}