using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VolumetricBeam : MonoBehaviour
{
    [Header("Wymiary promienia")]
    public float startRadius = 0.2f;
    public float endRadius = 2.5f;
    public float length = 15.0f;
    public int segments = 32;

    private Mesh mesh;
    private MeshFilter meshFilter;

    void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        BuildBeamMesh();
    }

    void OnValidate()
    {
        BuildBeamMesh();
    }

    public void BuildBeamMesh()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        if (mesh == null)
        {
            mesh = new Mesh { name = "ProceduralBeamCone" };
            meshFilter.sharedMesh = mesh;
        }

        mesh.Clear();

        int vertCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[segments * 6];

        float angleStep = (2f * Mathf.PI) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // Wierzcholek startowy (u zrodla)
            vertices[i] = new Vector3(cos * startRadius, sin * startRadius, 0f);
            normals[i] = new Vector3(cos, sin, 0f).normalized;
            uvs[i] = new Vector2((float)i / segments, 0f);

            // Wierzcholek koncowy (rozszerzony)
            int endIdx = i + (segments + 1);
            vertices[endIdx] = new Vector3(cos * endRadius, sin * endRadius, length);
            normals[endIdx] = new Vector3(cos, sin, 0f).normalized;
            uvs[endIdx] = new Vector2((float)i / segments, 1f);
        }

        int triOffset = 0;
        for (int i = 0; i < segments; i++)
        {
            int currentStart = i;
            int nextStart = i + 1;
            int currentEnd = i + (segments + 1);
            int nextEnd = i + 1 + (segments + 1);

            // Pierwszy trojkat sciany
            triangles[triOffset++] = currentStart;
            triangles[triOffset++] = currentEnd;
            triangles[triOffset++] = nextStart;

            // Drugi trojkat sciany
            triangles[triOffset++] = nextStart;
            triangles[triOffset++] = currentEnd;
            triangles[triOffset++] = nextEnd;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}