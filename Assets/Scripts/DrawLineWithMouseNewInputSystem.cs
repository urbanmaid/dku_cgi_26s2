using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// https://youtu.be/mgmfZRllpzs
public class DrawLineWithMouseNewInputSystem : MonoBehaviour
{
    public float timerDelay = 0.01f;
    float timer;

    List<Vector3> linePoints;
    GameObject lineObject;
    LineRenderer line;

    void Start()
    {
        linePoints = new List<Vector3>();
    }

    Vector3 GetMousePosition()
    {
        if (Camera.main == null) return Vector3.zero;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        return ray.origin + ray.direction * 10f;
    }

    void Update()
    {
        bool mouseDown = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool mouseHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool mouseUp = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;

        // If mouse button down, create a new line gameobject with line renderer component
        if (mouseDown)
        {
            // Create an empty game object
            lineObject = new GameObject();
            lineObject.transform.parent = this.gameObject.transform;

            // Create line renderer component and set its property
            line = lineObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startWidth = 0.2f;
            line.endWidth = 0.2f;

            Color randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            line.startColor = randomColor;
            line.endColor = randomColor;

            linePoints.Clear();
            line.positionCount = 0; // no line yet
            timer = 0f; // timer reset
        }

        // Drawing line when mouse is moving (add the current mouse position to linePoints & lineRenderer)
        if (mouseHeld && line != null) {
            // Debug draw ray
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 origin = Camera.main.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane)
            );
            Vector3 to = GetMousePosition();
            Debug.DrawRay(origin, to - origin, Color.red);

            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                linePoints.Add(GetMousePosition()); // Add 3D mousepos into the list
                line.positionCount = linePoints.Count; // Update line renderer
                line.SetPositions(linePoints.ToArray());
                timer = timerDelay;
            }
        }

        // If mouse button up, clear the linePoints
        if (mouseUp)
        {
            //Debug.Log(linePoints.Count);
            linePoints.Clear(); // Clear the list
        }
    }
}
