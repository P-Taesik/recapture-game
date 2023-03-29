using UnityEngine;

public class Deadzone : MonoBehaviour
{
    public int damage = 10; // 플레이어가 입을 피해량 설정

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().TakeDamage(damage);
        }
    }
}
