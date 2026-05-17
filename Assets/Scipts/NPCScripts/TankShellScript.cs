using UnityEngine;

public class TankShellScript : MonoBehaviour
{
    Rigidbody2D rb;
    public int damage = 1;
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("TankShell")) 
        {
            return; 
        }

    Destroy(gameObject);
    }
}
