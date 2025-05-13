using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 25f;           
    void Start()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        // Kiểm tra xem đối tượng va chạm có Health component hay không
        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);        // Gây sát thương
            gameObject.SetActive(false);
        }
    }
}