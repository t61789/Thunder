using Thunder.Utility;
using UnityEngine;

namespace Thunder.Test
{
    public class SaveTest : MonoBehaviour
    {
        private void Awake()
        {
            ControllerInput.AttachTo(GameObject.Find("battleShip"), true);
        }
    }
}
