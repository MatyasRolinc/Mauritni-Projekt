using UnityEngine;

public class TurrentMovement: MonoBehaviour
{
      void Update()
    {
        // Převod pozice myši do světových souřadnic
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Směr z věže k myši
        Vector3 direction = mousePos - transform.position;

        // Výpočet úhlu ve stupních
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 🔥 -90f kompenzuje, že tvoje věž (sprite) míří DOPRAVA
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }
}
