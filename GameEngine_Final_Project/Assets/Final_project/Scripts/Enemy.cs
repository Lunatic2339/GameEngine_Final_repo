using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject bulletObject;
    private Rigidbody2D bulletrb;
    public float attackInterval = 2f;
    public Vector2 shootDirection;
    private float attackTimer;
    private Transform firePoint;
    public Transform playerPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attackTimer = 0f;
        bulletObject = GameObject.Find("cherry-1");
        firePoint = transform; // 현재 오브젝트의 위치를 발사 지점으로 설정
    }

    // Update is called once per frame
    void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            GameObject bullet = Instantiate(bulletObject, firePoint.position , Quaternion.identity);
            bulletrb = bullet.GetComponent<Rigidbody2D>();
            playerPos = GameObject.Find("Player").transform;
            shootDirection = playerPos.position - firePoint.position;
            shootDirection.Normalize();
            bulletrb.linearVelocity = shootDirection * 10f;
            Destroy(bullet, 5f);
            Debug.Log("Enemy Attacked!" );
        }
    }
}
