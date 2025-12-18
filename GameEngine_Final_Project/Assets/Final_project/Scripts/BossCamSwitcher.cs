using UnityEngine;

public class BossCamSwitcher : MonoBehaviour
{
    public GameObject bossCamera; // 아까 만든 보스용 VCam 오브젝트

    // 보스 트리거에서 부를 함수
    public void SwitchToBossCam()
    {
        if (bossCamera != null)
        {
            bossCamera.SetActive(true); // 보스 카메라를 켜면 시네머신이 알아서 그리로 이동함
        }
    }

    // 보스 죽을 때 부를 함수
    public void SwitchToNormalCam()
    {
        if (bossCamera != null)
        {
            bossCamera.SetActive(false); // 끄면 다시 원래 카메라로 돌아감
        }
    }
}