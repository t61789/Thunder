using System;
using System.Collections.Generic;
using Framework;
using Newtonsoft.Json;
using UnityEngine;

namespace Thunder
{
    public class Map : MonoBehaviour
    {
        public static Map Ins { private set; get; }

        public Coord2 MapSize = new Coord2(100, 100);
        public float GridSize = 1;
        public Vector3 Center = Vector3.zero;

        private Coord2 _MapCenter;
        private Cell[,] _Map;
        private Dictionary<int, BuildingInfo> _BuildingInfos;

        private void Awake()
        {
            Ins = this;
            _Map = new Cell[MapSize.x, MapSize.y];
            _MapCenter = MapSize / 2;
            if (_BuildingInfos == null)
                LoadBuildingInfos();
        }

        public bool GroundEmpty(IEnumerable<Coord2> coords)
        {
            foreach (var coord2 in coords)
                if (GetCell(coord2).BuildingId != 0)
                    return false;

            return true;
        }

        /// <summary>
        /// 放置建筑
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="rotation">向某方向旋转多次，大于0为逆时针，小于0为顺时针</param>
        /// <param name="center">建筑的中心点</param>
        public void PutBuilding(int buildingId, int rotation, Coord2 center)
        {
            if (buildingId == 0) return;

            foreach (var coord in _BuildingInfos[buildingId].GetRotatedCoords(rotation))
            {
                var newCoord = coord + center;
                var cell = new Cell()
                {
                    BuildingId = buildingId
                };

                SetCell(newCoord,cell);
            }
        }

        public BuildingInfo GetBuildingInfo(int buildingId)
        {
            return _BuildingInfos[buildingId];
        }

        public Coord2 GetCoord(Vector3 xzPos)
        {
            xzPos = SnapToGrid(xzPos);
            xzPos -= Center;
            return new Coord2(Tools.Round(xzPos.x / GridSize), Tools.Round(xzPos.z / GridSize));
        }

        public Vector3 SnapToGrid(Vector3 xzOrigin)
        {
            return new Vector3(SnapToGrid(xzOrigin.x, true), xzOrigin.y, SnapToGrid(xzOrigin.z, false));
        }

        private float SnapToGrid(float origin, bool x)
        {
            var offset = (x ? Center.x : Center.z) % GridSize;
            origin -= offset;

            var temp = origin;
            var mod = temp % GridSize;
            temp -= mod;

            if (Mathf.Abs(mod) > GridSize / 2)
                temp += GridSize * Mathf.Sign(origin);

            origin += offset;
            return temp;
        }

        private void LoadBuildingInfos()
        {
            _BuildingInfos = new Dictionary<int, BuildingInfo>();
            foreach (var buildingInfo in ValueSys.GetValue<BuildingInfo[]>(Config.BuildingInfoValueAssetPath))
            {
                buildingInfo.Init(GridSize);
                _BuildingInfos.Add(buildingInfo.Id, buildingInfo);
            }
        }

        private Cell GetCell(Coord2 coord)
        {
            coord += _MapCenter;
            return _Map[coord.x, coord.y];
        }

        private void SetCell(Coord2 coord, Cell cell)
        {
            coord += _MapCenter;
            _Map[coord.x, coord.y] = cell;
        }

        private struct Cell
        {
            public int BuildingId;
        }
    }

    [PreferenceAsset]
    public struct BuildingInfo
    {
        public int Id;
        public string ModelPath;
        public string PrefabPath;
        public Coord2[] Coords;

        [JsonIgnore] public Coord2 RotateCenterCoord;
        [JsonIgnore] public Vector3 CenterPosOffset;

        public void Init(float gridSize)
        {
            SortCoords(gridSize);
        }

        public IEnumerable<Coord2> GetRotatedCoords(int rotation)
        {
            rotation %= 4;
            var matrix = rotation > 0 ? Tools.CounterClockwiseRot : Tools.ClockwiseRot;
            rotation = Mathf.Abs(rotation);

            foreach (var coord in Coords)
            {
                var vector = new Vector3(coord.x,0, coord.y);

                for (int i = 0; i < rotation; i++)
                    vector = matrix * vector;

                yield return new Coord2(Tools.Round(vector.x), Tools.Round(vector.z));
            }
        }

        private void SortCoords(float gridSize)
        {
            if (Coords == null || Coords.Length == 0) return;
            var min = new Coord2(int.MaxValue, int.MaxValue);
            var max = new Coord2(int.MinValue, int.MinValue);
            foreach (var coord2 in Coords)
            {
                if (coord2.x < min.x)
                    min.x = coord2.x;
                if (coord2.y < min.y)
                    min.y = coord2.y;
                if (coord2.x > max.x)
                    max.x = coord2.x;
                if (coord2.y > max.y)
                    max.y = coord2.y;
            }

            var center = (min + max) / 2;
            var offset = (min.ToVector2() + max.ToVector2()) * gridSize / 2;
            CenterPosOffset = (offset - center.ToVector2() * gridSize).PutDown();

            for (int i = 0; i < Coords.Length; i++)
                Coords[i] -= center;
        }
    }
}
