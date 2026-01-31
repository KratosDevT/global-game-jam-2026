using UnityEngine;


[ExecuteAlways]
public class TestPerShaderino : MonoBehaviour
{
    [Header("Reveal Settings")]
    public float radius = 2f;
    public float edgeSoftness = 0.3f;
    public Color edgeColor = Color.cyan;

    [Header("Debug")]
    public bool followMouse = true;

    Camera cam;

    void OnEnable()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector3 pos = transform.position;

        if (followMouse && cam != null)
        {
            Vector3 mouse = Input.mousePosition;
            mouse.z = -cam.transform.position.z;
            pos = cam.ScreenToWorldPoint(mouse);
            pos.z = 0;
        }

        Shader.SetGlobalVector("_GlobalRevealPos", pos);
        Shader.SetGlobalFloat("_GlobalRevealRadius", radius);
        Shader.SetGlobalFloat("_GlobalEdgeSoftness", edgeSoftness);
        Shader.SetGlobalColor("_GlobalEdgeColor", edgeColor);
    }
}