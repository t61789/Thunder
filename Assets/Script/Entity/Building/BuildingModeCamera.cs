using DG.Tweening;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class BuildingModeCamera : BaseEntity
    {
        public static BuildingModeCamera Ins { private set; get; }

        public int StartMoveShieldValue = 15;
        public Vector3 StartOffsetPos = new Vector3(0,10,0);
        public Vector3 StartRotVec = Vector3.down;
        public float Height = 10;
        public float MoveToStartPosTime = 1f;
        public string MoveKey = "building_mode_camera_move_key";
        public string ScrollHeightKey = "building_mode_camera_scroll_height_key";
        public string FastMoveKey = "building_mode_camera_fast_move_key";
        public float MoveDamp = 0.3f;
        public float MoveSpeed = 0.1f;
        public float ScrollHeightSpeed = 2;
        public float FastMoveFactor=2;

        [HideInInspector]
        public Vector3 AimPos { private set; get; }
            = Vector3.positiveInfinity;

        private Camera _Camera;
        private int _StableLayer;
        private Vector3 _MoveTarget;
        private bool _AutoMoving;
        private Quaternion _StartRot;
        private InputSynchronizer _MoveSyner = new InputSynchronizer();
        private InputSynchronizer _FastMoveSyner = new InputSynchronizer();
        private InputSynchronizer _ScrollHeightSyner = new InputSynchronizer();
        private Process _Process;

        protected override void Awake()
        {
            base.Awake();
            Ins = this;
            _StableLayer = LayerMask.NameToLayer(Config.StableLayerName);
            _StartRot = Quaternion.LookRotation(StartRotVec);
            gameObject.SetActive(false);
            _Camera = GetComponent<Camera>();

            PublicEvents.StartingBuildingMode.AddListener(StartingBuildingMode);
            PublicEvents.StartBuildingMode.AddListener(StartBuildingMode);
            PublicEvents.EndingBuildingMode.AddListener(EndingBuildingMode);
            PublicEvents.EndBuildingMode.AddListener(EndBuildingMode);
        }

        private void Update()
        {
            MoveCtrl();
        }

        private void FixedUpdate()
        {
            if (_AutoMoving) return;

            UpdateAimPos();

            Move(_MoveSyner.Get().Axis,
                _FastMoveSyner.Get().Stay,
                -_ScrollHeightSyner.Get().Axis.x);

            FollowMoveTarget();
        }

        private void OnDestroy()
        {
            Ins = null;

            PublicEvents.StartingBuildingMode.RemoveListener(StartingBuildingMode);
            PublicEvents.StartBuildingMode.RemoveListener(StartBuildingMode);
            PublicEvents.EndingBuildingMode.RemoveListener(EndingBuildingMode);
            PublicEvents.EndBuildingMode.RemoveListener(EndBuildingMode);
        }

        private void MoveCtrl()
        {
            var ctrlInfo = ControlSys.RequireKey(CtrlKeys.GetKey(MoveKey));
            ctrlInfo.HandleRawAxis();
            _MoveSyner.Set(ctrlInfo);

            ctrlInfo = ControlSys.RequireKey(CtrlKeys.GetKey(FastMoveKey));
            _FastMoveSyner.Set(ctrlInfo);

            ctrlInfo = ControlSys.RequireKey(CtrlKeys.GetKey(ScrollHeightKey));
            _ScrollHeightSyner.Set(ctrlInfo);
        }

        private void Move(Vector3 dir,bool fastMove,float heightChange)
        {
            Height += heightChange;
            _MoveTarget += dir*MoveSpeed* (fastMove ? FastMoveFactor:1);
            _MoveTarget.y = Height;
        }

        private void FollowMoveTarget()
        {
            Trans.position = Tools.Lerp(Trans.position, _MoveTarget, MoveDamp);
        }

        private void UpdateAimPos()
        {
            var hits = Physics.RaycastAll(
                _Camera.ScreenPointToRay(Input.mousePosition), 
                Mathf.Infinity, 
                1 << _StableLayer);

            if (hits.Length == 0)
            {
                AimPos = Vector3.positiveInfinity;
                return;
            }

            AimPos = hits[0].point;
        }

        private void StartingBuildingMode(Process process)
        {
            gameObject.SetActive(true);

            _Process = process;
            _Process.Start();

            Trans.position = FpsCamera.Ins.transform.position;
            Trans.rotation = FpsCamera.Ins.transform.rotation;

            ControlSys.ShieldValue.Request(nameof(BuildingModeCamera), StartMoveShieldValue);
            _AutoMoving = true;

            var targetPos = new Vector3(0, Height, 0) + Map.Ins.Center + StartOffsetPos;

            Trans.DOMove(
                targetPos,
                MoveToStartPosTime).OnComplete(StartMoveComplete);
            Trans.DORotateQuaternion(_StartRot, MoveToStartPosTime);
        }

        private void StartMoveComplete()
        {
            _Process.End();
        }

        private void StartBuildingMode()
        {
            ControlSys.ShieldValue.Release(nameof(BuildingModeCamera));
            _AutoMoving = false;
            _MoveTarget = Trans.position;
            ControlSys.LockCursor = false;
        }

        private void EndingBuildingMode(Process process)
        {
            _Process = process;
            _Process.Start();

            ControlSys.ShieldValue.Request(nameof(BuildingModeCamera), StartMoveShieldValue);
            _AutoMoving = true;

            Trans.DOMove(
                FpsCamera.Ins.transform.position,
                MoveToStartPosTime).OnComplete(EndMoveComplete);
            Trans.DORotateQuaternion(FpsCamera.Ins.transform.rotation, MoveToStartPosTime);
        }

        private void EndMoveComplete()
        {
            _Process.End();
        }

        private void EndBuildingMode()
        {
            gameObject.SetActive(false);
            ControlSys.ShieldValue.Release(nameof(BuildingModeCamera));
            _AutoMoving = false;
            _MoveTarget = Trans.position;
            ControlSys.LockCursor = true;
        }
    }
}
