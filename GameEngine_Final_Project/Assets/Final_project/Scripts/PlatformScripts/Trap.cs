using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("트랩 모드 설정")]
    [Tooltip("체크하면 닿자마자 즉사합니다 (Die 호출)")]
    public bool isInstantKill = false;

    [Tooltip("즉사가 아닐 때 입힐 데미지 양")]
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();

            if (player != null)
            {
                if (isInstantKill)
                {
                    // 모드 1: 즉사 (Die 함수 강제 호출)
                    Debug.Log("즉사 트랩 발동!");
                    player.Die(); 
                }
                else
                {
                    // 모드 2: 데미지 (설정된 값만큼만 깎임)
                    // (밀쳐내는 효과를 위해 transform도 같이 전달)
                    player.TakeDamage(damage, transform); 
                }
            }
        }
    }
}