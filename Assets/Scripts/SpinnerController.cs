using UnityEngine;
using UnityEngine.UI;

public class SpinnerController : MonoBehaviour
{
  void Update()
  {
    if (this.GetComponent<Image>().enabled == true) this.gameObject.transform.Rotate(new Vector3(0f, 0f, -800f * Time.deltaTime), Space.Self);
  }
}
