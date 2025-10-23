    using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 150f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveInput = 0f;
        float rotateInput = 0f;

        if (Input.GetKey(KeyCode.S))
            moveInput = 1f;
        else if (Input.GetKey(KeyCode.W))
            moveInput = -1f;

        if (Input.GetKey(KeyCode.A))
            rotateInput = 1f;
        else if (Input.GetKey(KeyCode.D))
            rotateInput = -1f;

        MoveTank(moveInput, rotateInput);
    }

    void MoveTank(float moveInput, float rotateInput)
    {
        // 🔥 používáme transform.right, protože model míří DOPRAVA
        Vector2 direction = transform.right;

        rb.MovePosition(rb.position + direction * moveInput * moveSpeed * Time.deltaTime);
        rb.MoveRotation(rb.rotation + rotateInput * rotationSpeed * Time.deltaTime);
    }
}

