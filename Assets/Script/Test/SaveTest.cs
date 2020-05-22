using UnityEngine;

public class SaveTest : MonoBehaviour
{
    private void Awake()
    {
        ControllerInput.AttachTo(GameObject.Find("battleShip"), true);
    }
}
