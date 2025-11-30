using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ★ 싱글톤 (Singleton): 어디서든 GameManager.instance로 나를 부를 수 있게 함
    public static GameManager instance;

    [Header("플레이어 능력 정보 (저장됨)")]
    public bool hasDoubleJump = true; // 더블 점프 배웠니?
    public bool hasDash = false;       // 대쉬 배웠니? (나중에 확장)
    public int coinCount = 0;          // 코인 개수

    void Awake()
    {
        // 1. 싱글톤 패턴 구현
        if (instance == null)
        {
            instance = this; // 내가 바로 그 유일한 매니저다!
            DontDestroyOnLoad(gameObject); // 씬이 바껴도 나를 파괴하지 마라!
        }
        else
        {
            // 만약 씬을 이동했는데 또 다른 GameManager가 있다면?
            // 짝퉁은 바로 제거한다. (중복 방지)
            Destroy(gameObject);
        }
    }

    // 능력 해금 함수
    public void UnlockAbility(string abilityName)
    {
        if (abilityName == "DoubleJump")
        {
            hasDoubleJump = true;
            Debug.Log("매니저: 더블 점프 능력이 영구 저장되었습니다.");
        }
        // else if (abilityName == "Dash") ...
    }
}