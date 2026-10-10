using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

// Draw Quad by Mouse Click 

public class DrawQuadWithMouseNewInputSystem : MonoBehaviour
{
    public void Update()
    {
        //if (Input.GetMouseButtonDown(0)) 
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            //Vector3 mousePos = Input.mousePosition;
            Vector3 mousePos = Mouse.current.position.ReadValue();
            SpawnQuadAtScreen(mousePos);
        }
    }

    private void SpawnQuadAtScreen(Vector2 screenPos)
    {
        if (Camera.main == null) return;

        Debug.Log(screenPos);

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Vector3 mousePos = new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane);
        quad.transform.position = Camera.main.ScreenToWorldPoint(mousePos); // 2D screen space → 3D world space
        quad.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        quad.GetComponent<Renderer>().material.color = Color.yellow;       // yellow
        quad.transform.parent = this.gameObject.transform;
    }

    void OnGUI()
    {
        Vector3 point = new Vector3();
        Event   currentEvent = Event.current;
        Vector3 mousePos = new Vector3();

        // Get the mouse position from Event.
        // New Input System
        if (Mouse.current != null)
        {
            Vector2 p = Mouse.current.position.ReadValue();
            mousePos.x = p.x;
            mousePos.y = p.y;
        }
        // Old Input
        // Note that the y position from Event is inverted. (GUI space -> Screen space)
        else
        {
            mousePos.x = currentEvent.mousePosition.x;
            mousePos.y = Camera.main.pixelHeight - currentEvent.mousePosition.y;
        }
		mousePos.z = Camera.main.nearClipPlane;
        point = Camera.main.ScreenToWorldPoint(mousePos);

        GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        GUILayout.Label("Screen pixels: " + Camera.main.pixelWidth + ":" + Camera.main.pixelHeight);
        GUILayout.Label("Mouse position: " + mousePos);
        GUILayout.Label("World position: " + point.ToString("F3"));
        GUILayout.EndArea();

        // It outputs messages depending on the current Event Type.
        if (currentEvent.type == EventType.MouseDown)
        {
            Debug.Log("Mouse Down.");
        }

        if (currentEvent.type == EventType.MouseDrag)
        {
            Debug.Log("Mouse Dragged.");
        }

        if (currentEvent.type == EventType.MouseUp)
        {
            Debug.Log("Mouse Up.");
        }
    }
}
