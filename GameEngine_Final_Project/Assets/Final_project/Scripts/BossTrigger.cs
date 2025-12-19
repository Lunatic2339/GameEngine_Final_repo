using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public BossController boss; // 보스 연결
    public BossCamSwitcher camSwitcher;
// ★ [추가] 음악 매니저 연결
    public MusicManager musicManager;
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {

            if (boss != null && !boss.isActivated)
            {
                boss.ActivateBoss();

                GetComponent<Collider2D>().enabled = false;
            }
            if (camSwitcher != null) 
            {
                camSwitcher.SwitchToBossCam();
                
                // 보스가 죽을 때 카메라를 꺼야 하니까, 보스한테도 스위처를 알려줌
                boss.camSwitcher = camSwitcher; 
            }

            // (주의) 트리거를 Destroy 해버리면 camSwitcher 변수도 사라질 수 있으니
            // 트리거 오브젝트는 남겨두고 Collider만 끄거나,
            // camSwitcher를 다른 오브젝트에 두는 게 안전합니다.
            // ★ [추가] 보스 음악 틀어라!
            if (musicManager != null)
            {
                musicManager.PlayBossMusic();
                
                // 보스가 죽을 때 다시 원래 음악 틀어야 하니까, 보스한테 매니저를 넘겨줌
                boss.musicManager = musicManager;
            }
            
            // 일단 간단하게 Collider만 끄겠습니다.
            GetComponent<Collider2D>().enabled = false;
    }
}
}