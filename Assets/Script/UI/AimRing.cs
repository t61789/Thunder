using Assets.Script.Turret;
using Tool;
using UnityEngine;

namespace Assets.Script.UI
{
    public class AimRing : BaseUi
    {
        [Range(3, 35)]
        public int Detail = 50;
        public float Width = 0.5f;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Material ringMaterial;
        private Color color;
        private Aircraft aircraft;
        private Transform aimPoint;
        private Vector3 preAimPos;

        protected override void Awake()
        {
            base.Awake();
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            aimPoint = RectTrans.Find("aimPoint");
            ringMaterial = meshRenderer.material;
            color = ringMaterial.GetColor("_Color");

            if (Detail > 2) SetMesh(Detail);
        }

        private void FixedUpdate()
        {
            if (aircraft == null)
            {
                Destroy(gameObject);
                return;
            }

            RectTrans.position = aircraft.trans.position;
            if (!aircraft.aimmingPos.Equals(preAimPos))
            {
                preAimPos = aircraft.aimmingPos;
                SetColor(1);
                SetPreference(aircraft.aimmingPos.magnitude, Width);
                aimPoint.localPosition = aircraft.aimmingPos;
            }
            else
            {
                SetColor(0);
            }
        }

        public void Init(Aircraft aircraft)
        {
            this.aircraft = aircraft;
        }

        public void SetMesh(int detail)
        {
            meshFilter.mesh = Tools.CreateRingMesh(detail);
        }

        public void SetPreference(float radius, float width)
        {
            ringMaterial.SetFloat("_Radius", radius);
            ringMaterial.SetFloat("_Width", width);
        }

        public void SetColor(Color color)
        {
            this.color = color;
            ringMaterial.SetColor("_Color", color);
        }

        public void SetColor(float alpha)
        {
            color.a = alpha;
            ringMaterial.SetColor("_Color", color);
        }
    }
}
