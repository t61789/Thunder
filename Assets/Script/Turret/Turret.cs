using UnityEngine;

namespace Assets.Script.Turret
{
    public class Turret : Controller
    {
        [HideInInspector]
        public Ship ship;

        protected Transform trans;
        protected Animator animator;

        public virtual void Install(Ship ship, Vector3 position, Vector3 rotaion)
        {
            this.ship = ship;
            GetComponent<SpriteRenderer>().sortingOrder = ship.GetComponent<SpriteRenderer>().sortingOrder + 1;
            animator = GetComponent<Animator>();
            trans = transform;
            trans.SetParent(ship.transform);
            trans.localPosition = position;
            trans.localRotation = Quaternion.FromToRotation(Vector3.up, rotaion);
        }

        public virtual void Remove()
        {
            Destroy(gameObject);
        }
    }
}
