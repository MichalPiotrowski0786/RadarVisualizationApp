using UnityEngine;

public class CameraController : MonoBehaviour
{
  Camera cam;

  Vector3 mouseStart;
  int maxZoom = 10;
  int minZoom = 260;

  void Start()
  {
    cam = GetComponent<Camera>();
  }

  void Update()
  {
    CameraZoom();
    CameraMovement();
    CameraConstrain();
  }

  void CameraZoom()
  {
    float value = Input.mouseScrollDelta.y * -10f;
    cam.orthographicSize += value;
    cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, maxZoom, minZoom);
  }

  void CameraMovement()
  {
    if (Input.GetMouseButtonDown(0))
    {
      mouseStart = GetWorldPosition(0f);
    }
    if (Input.GetMouseButton(0))
    {
      Vector3 direction = mouseStart - GetWorldPosition(0f);
      cam.transform.position += direction;
    }
  }

  void CameraConstrain()
  {
    float zoom = Mathf.InverseLerp(minZoom, maxZoom, cam.orthographicSize);
    if (cam.transform.position.x < -250f * zoom)
      cam.transform.position = new Vector3(
          -250f * zoom,
          cam.transform.position.y,
          cam.transform.position.z);

    if (cam.transform.position.x > 250f * zoom)
      cam.transform.position = new Vector3(
          250f * zoom,
          cam.transform.position.y,
          cam.transform.position.z);

    if (cam.transform.position.y < -250f * zoom)
      cam.transform.position = new Vector3(
          cam.transform.position.x,
          -250f * zoom,
          cam.transform.position.z);

    if (cam.transform.position.y > 250f * zoom)
      cam.transform.position = new Vector3(
          cam.transform.position.x,
          250f * zoom,
          cam.transform.position.z);
  }

  private Vector3 GetWorldPosition(float z)
  {
    Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
    Plane ground = new Plane(Vector3.forward, new Vector3(0, 0, z));
    float distance = 0f;
    ground.Raycast(mousePos, out distance);
    return mousePos.GetPoint(distance);
  }
}
