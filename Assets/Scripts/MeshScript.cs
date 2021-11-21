using UnityEngine;

public class MeshScript : MonoBehaviour
{
  public Material material;
  public Mesh mesh;
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

    Vector3[] verts = new Vector3[(rays + 1) * (bins + 1)];
    Vector2[] uvs = new Vector2[verts.Length];
    int[] tris = new int[verts.Length * 6];

    for (int index = 0, i = 0; i < rays + 1; i++)
    {
      for (int j = 0; j < bins + 1; j++)
      {
        float theta = Mathf.Deg2Rad * i;
        float x = Mathf.Cos(theta) * (j + 1);
        float z = Mathf.Sin(theta) * (j + 1);

        verts[index] = new Vector3(x, 0f, z);
        uvs[index] = new Vector2((float)i / rays, (float)j / bins);
        index++;
      }
    }

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
