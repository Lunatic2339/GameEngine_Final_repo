using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("연결할 컴포넌트들")]
    public BossController boss;         // 보스
    public BossCamSwitcher camSwitcher; // 카메라 전환
    public MusicManager musicManager;   // 음악 전환
    
    // ★ [추가] 보스방 입구를 막을 벽 오브젝트
    public GameObject bossWall;         

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 보스 깨우기
            if (boss != null && !boss.isActivated)
            {
                boss.ActivateBoss();
            }

            // 2. 카메라 전환
            if (camSwitcher != null) 
            {
                camSwitcher.SwitchToBossCam();
                // 보스 죽을 때 카메라 복구를 위해 보스에게 전달
                boss.camSwitcher = camSwitcher; 
            }

            // 3. 음악 전환
            if (musicManager != null)
            {
                musicManager.PlayBossMusic();
                // 보스 죽을 때 음악 복구를 위해 보스에게 전달
                boss.musicManager = musicManager;
            }

            // 4. ★ [추가] 보스방 문 닫기 (퇴로 차단)
            if (bossWall != null)
            {
                bossWall.SetActive(true); // 벽을 켜서 못 나가게 함
            }
            
            // 5. 트리거 비활성화 (더 이상 작동 안 하게)
            // (오브젝트는 남겨두고 충돌만 끕니다)
            GetComponent<Collider2D>().enabled = false;
        }
    }
}