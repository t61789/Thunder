using Assets.Script.PublicScript;
using UnityEngine;

namespace Assets.Script.Test
{
    public class SaveTest : MonoBehaviour
    {
        private void Awake()
        {
            ControllerInput.AttachTo(GameObject.Find("battleShip"), true);
        }
    }
}
