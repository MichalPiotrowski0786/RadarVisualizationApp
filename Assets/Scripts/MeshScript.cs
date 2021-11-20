using UnityEngine;

public class MeshScript : MonoBehaviour
{
  public Material material;
  public Mesh mesh;
  GameObject go;
  TextureScript TexScript;

  int rays = 361;
  int bins = 250;

  void Start()
  {
    Generate();
  }

  void Update()
  {
    Vector3 meshRotation = new Vector3(TexScript.angles[TexScript.datatype] - 180f, 90f, -90f);
    go.transform.rotation = Quaternion.Euler(meshRotation.x, meshRotation.y, meshRotation.z);
  }

  void Generate()
  {
    TexScript = this.GetComponent<TextureScript>();
    if (TexScript != null)
    {
      if (TexScript.rays > 0) rays = TexScript.rays;
      if (TexScript.bins > 0) bins = TexScript.bins;
    }

    go = new GameObject("RadarMesh");
    mesh = new Mesh();
    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.Clear();

    go.AddComponent<MeshFilter>().mesh = mesh;
    go.AddComponent<MeshRenderer>().material = material;

    Vector3[] verts = new Vector3[(rays) * (bins + 1)];
    Vector2[] uvs = new Vector2[verts.Length];
    int[] tris = new int[verts.Length * 6];

    for (int index = 0, i = 0; i < rays; i++)
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
    for (int i = 0; i < rays - 1; i++)
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
