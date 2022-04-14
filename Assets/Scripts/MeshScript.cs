using UnityEngine;

public class MeshScript : MonoBehaviour
{
    public Material material;
    public Mesh mesh;
    public ComputeShader cs;
    GameObject go;

    int rays = 360;
    int bins = 250;

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        go = new GameObject("RadarMesh");
        go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        go.tag = "radar";

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.Clear();

        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = material;

        int karnelIndex = cs.FindKernel("Main");

        int buffer_lenght = (rays + 1) * (bins + 1);
        ComputeBuffer verts_buffer = new ComputeBuffer(buffer_lenght, sizeof(float) * 3);
        ComputeBuffer uvs_buffer = new ComputeBuffer(buffer_lenght, sizeof(float) * 2);
        Vector3[] verts = new Vector3[buffer_lenght];
        Vector2[] uvs = new Vector2[buffer_lenght];

        cs.SetBuffer(karnelIndex, "verts", verts_buffer);
        cs.SetBuffer(karnelIndex, "uvs", uvs_buffer);
        cs.SetInt("rays", rays);
        cs.SetInt("bins", bins);

        cs.Dispatch(karnelIndex, 1, 1, 1);

        verts_buffer.GetData(verts);
        uvs_buffer.GetData(uvs);

        verts_buffer.Release();
        uvs_buffer.Release();

        int[] tris = new int[verts.Length * 6];
        int tri_index = 0;
        int vert_index = 0;
        for (int i = 0; i < rays; i++)
        {
            for (int j = 0; j < bins; j++)
            {
                tris[tri_index] = vert_index;
                tris[tri_index + 1] = vert_index + bins + 1;
                tris[tri_index + 2] = vert_index + 1;
                tris[tri_index + 3] = vert_index + 1;
                tris[tri_index + 4] = vert_index + bins + 1;
                tris[tri_index + 5] = vert_index + bins + 2;

                vert_index++;
                tri_index += 6;
            }
            vert_index++;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
    }
}
