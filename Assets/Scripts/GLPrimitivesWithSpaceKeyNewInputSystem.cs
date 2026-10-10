using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class GLPrimitivesWithSpaceKeyNewInputSystem : MonoBehaviour
{
    public float radius = 3.0f;
    public int segments = 16;

    private static readonly int[] PrimitiveModes = { GL.LINES, GL.LINE_STRIP, GL.TRIANGLES, GL.TRIANGLE_STRIP, GL.QUADS };
    [Range(0, 4)] private int index = 0;
    private Material mat = null;

    private void OnEnable()
    {
        EnsureMaterial();
    }

    private void OnDisable()
    {
        // Destroy Material
        if (mat != null)
        {
            Destroy(mat);
            mat = null;
        }
    }

    private void EnsureMaterial()
	{
		if (!mat)
		{
			// Unity has a built-in shader that is useful for drawing
			// simple colored things.
			Shader shader = Shader.Find ("Hidden/Internal-Colored");
			mat = new Material (shader);
			mat.hideFlags = HideFlags.HideAndDontSave;
			// Turn on alpha blending
			mat.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			mat.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
            // Turn backface culling off
            //mat.SetInt ("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            mat.SetInt ("_ZWrite", 0);
		}
	}

    private void DrawCircle(int mode)
    {
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
    
        GL.Begin(mode);
        GL.Color(Color.green);

        float deltaTheta = -2.0f * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float theta = i * deltaTheta;
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            GL.Vertex(new Vector3(x, y, 0));
        }

        GL.End();
        GL.PopMatrix();
    }

    // will be called after all regular rendering is done
    public void OnRenderObject()
    {
        EnsureMaterial();
        if (mat == null) return;
        mat.SetPass(0);

        DrawCircle(PrimitiveModes[index]);
    }

    public void Update()
    {
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (spacePressed)
        {
            index = (index + 1) % PrimitiveModes.Length;
            Debug.Log("primitivemode=" + PrimitiveModes[index]);
        }
    }
}
