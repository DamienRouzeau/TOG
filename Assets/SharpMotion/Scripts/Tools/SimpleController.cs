using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{
    public float moveSpeed = 5f;       // Vitesse de déplacement
    public float rotateSpeed = 100f;   // Vitesse de rotation

    void Update()
    {
        // Déplacement
        float moveHorizontal = 0f;
        float moveVertical = 0f;
        float moveUpDown = 0f;

        if (Input.GetKey(KeyCode.Z))
        {
            moveVertical += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVertical -= 1f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            moveHorizontal -= 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveHorizontal += 1f;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            moveUpDown += 1f;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveUpDown -= 1f;
        }

        Vector3 move = new Vector3(moveHorizontal, moveUpDown, moveVertical) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.Self);

        // Rotation
        float rotate = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            rotate -= 1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotate += 1f;
        }

        transform.Rotate(Vector3.up, rotate * rotateSpeed * Time.deltaTime);
    }
}
