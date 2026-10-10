using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class SelectObjectWithMouseNewInputSystem : MonoBehaviour
{
    public GameObject selectedObject;
    Ray ray;
    RaycastHit hitData;

    // Create a plane, sphere and cube in the Scene.
    void Start()
    {
        GameObject plane  = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.parent = this.transform;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0.5f, 0);
        cube.transform.parent = this.transform;
        
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(0, 1.5f, 0);
        sphere.transform.parent = this.transform;

        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.position = new Vector3(2, 1, 0);
        capsule.transform.parent = this.transform;

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = new Vector3(-2, 1, 0);
        cylinder.transform.parent = this.transform;
    }

    void Update()
    {
        if (Camera.main == null) return;

        // mouse position
        Vector2 screenPos = Mouse.current.position.ReadValue();

        // 2D mouse position to 3D world position ray
        ray = Camera.main.ScreenPointToRay(screenPos);
        bool hit = Physics.Raycast(ray, out hitData, 1000f);
        // left mouse button
        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (hit)
        {
            // If the mouse selects on one of game objects
            if (clicked)
            {
                // 이전 선택 해제(흰색) 후 새 선택(파란색)
                if (selectedObject != null)
                {
                    var prevR = selectedObject.GetComponent<Renderer>();
                    if (prevR) prevR.material.color = Color.white;
                }

                selectedObject = hitData.transform.gameObject;
                var renderer = selectedObject.GetComponent<Renderer>();
                if (renderer) renderer.material.color = Color.blue; // blue 
            }
        }
        else 
        {
            // If the mouse selects on empty space
            if (clicked)
            {
                if (selectedObject != null)
                {
                    var renderer = selectedObject.GetComponent<Renderer>();
                    if (renderer) renderer.material.color = Color.white; // white 
                    selectedObject = null;
                }             
            }
        }
    }
}
