using Framework;
using System.Collections.Generic;
using Thunder.UI;
using UnityEngine;

namespace Thunder
{
    public class VirtualBuilding : MonoBehaviour
    {
        public string RotateKey = "virtual_building_rotate_key";
        public string PutKey = "virtual_building_put_key";
        public string CancelKey = "virtual_building_cancel_key";

        public bool CanPut { private set; get; }
        public static VirtualBuilding Ins { private set; get; }

        private Transform _Trans;
        private int _Rotation;
        private Vector3 _CenterOffset;
        private BuildingInfo _BuildingInfo;
        private Vector3 _PrePos;
        private GameObject _Model;

        private void Awake()
        {
            Ins = this;
            _Trans = transform;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            PlayerCtrl();
        }

        private void FixedUpdate()
        {
            var newPos = BuildingModeCamera.Ins.AimPos;
            if (newPos == Vector3.positiveInfinity)
                return;

            newPos = Map.Ins.SnapToGrid(newPos - _CenterOffset);
            _Trans.position = newPos + _CenterOffset;

            if (newPos.x != _PrePos.x || newPos.z != _PrePos.z)
            {
                _PrePos = newPos;
                CanPut = Map.Ins.GroundEmpty(GetCurCoords());
            }
        }

        private void OnDestroy()
        {
            Ins = null;
        }

        private void PlayerCtrl()
        {
            if (ControlSys.RequireKey(CtrlKeys.GetKey(PutKey)).Down)
            {
                if (Put()) Debug.Log("Success");
            }

            if (ControlSys.RequireKey(CtrlKeys.GetKey(RotateKey)).Down)
            {
                Rotate(true);
            }

            if (ControlSys.RequireKey(CtrlKeys.GetKey(CancelKey)).Down)
            {
                Cancel();
            }
        }

        private IEnumerable<Coord2> GetCurCoords()
        {
            var coord = Map.Ins.GetCoord(_PrePos);
            foreach (var rotatedCoord in _BuildingInfo.GetRotatedCoords(_Rotation))
            {
                yield return coord + rotatedCoord;
            }
        }

        public void Show(int buildingId)
        {
            gameObject.SetActive(true);

            _BuildingInfo = Map.Ins.GetBuildingInfo(buildingId);

            if (_Model != null)
            {
                Destroy(_Model);
                _Model = null;
            }

            if (!string.IsNullOrEmpty(_BuildingInfo.ModelPath))
            {
                _Model = GameObjectPool.GetPrefab(_BuildingInfo.ModelPath).GetInstantiate();
                _Model.transform.SetParent(_Trans);
                _Model.transform.localPosition = Vector3.zero;
                _Model.transform.localRotation = Quaternion.identity;
            }
            else
            {
                Debug.LogWarning($"未指定建筑模型 Id:{buildingId}");
            }

            _CenterOffset = _BuildingInfo.CenterPosOffset;

            _Rotation = 0;
            _Trans.rotation = Quaternion.identity;
        }

        public void Rotate(bool clockwise)
        {
            Matrix4x4 matrix;
            float rotAngle;
            if (clockwise)
            {
                _Rotation--;
                matrix = Tools.ClockwiseRot;
                rotAngle = 90;
            }
            else
            {
                _Rotation++;
                matrix = Tools.CounterClockwiseRot;
                rotAngle = -90;
            }
            _Rotation %= 4;

            _CenterOffset = matrix * _CenterOffset;
            _Trans.rotation *= Quaternion.AngleAxis(rotAngle, Vector3.up);
            CanPut = Map.Ins.GroundEmpty(GetCurCoords());
        }

        public void Cancel()
        {
            gameObject.SetActive(false);
        }

        public bool Put()
        {
            if (!CanPut) return false;
            if (Map.Ins.PutBuilding(_BuildingInfo.Id, _Rotation, Map.Ins.GetCoord(_PrePos)))
            {
                gameObject.SetActive(false);
                return true;
            }
            else
            {
                LogPanel.Ins.LogSystem($"Resource doesn't enough");
                return false;
            }
        }
    }
}
