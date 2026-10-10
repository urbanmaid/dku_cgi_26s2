using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class RotateCubeWithMouseNewInputSystem : MonoBehaviour
{
    public float rotSpeed = 10f;
    GameObject cube;

    void Start()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);	
        cube.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 1.0f, 1.0f); 
        cube.transform.parent = this.gameObject.transform;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();

        }
        else if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("space key");
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            // Mouse X/Y axis: delta.
            Vector2 md = Mouse.current.delta.ReadValue();
            float polar = md.x * rotSpeed;
            float elevation = md.y * rotSpeed;
            transform.Rotate(Vector3.down, polar);
            transform.Rotate(Vector3.right, elevation);
        }

        // The value is in the range -1 to 1
        // WASD-key Arrow-Key
        Vector2 movement = InputSystem.actions["Move"].ReadValue<Vector2>();
        float h = movement.x; // -1 .. +1
        float v = movement.y; // -1 .. +1

        transform.Rotate(Vector3.down, h * rotSpeed * 0.1f);
        transform.Rotate(Vector3.right, v * rotSpeed * 0.1f);
    }

}
