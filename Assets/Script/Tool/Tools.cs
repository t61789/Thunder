using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DG.Tweening;
using Newtonsoft.Json;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Thunder.Tool
{
    public static class Tools
    {
        public static Text LogContainer;

        public static Vector3 ScreenMiddle = new Vector3(Screen.width / 2f, Screen.height / 2f);

        public static Vector2 FarPosition = new Vector2(int.MaxValue, int.MaxValue);

        private static readonly Random _Random;

        private static readonly LRUCache<string,object> _JsonCache 
            = new LRUCache<string, object>(20);

        static Tools()
        {
            _Random = new Random(Guid.NewGuid().GetHashCode());
        }

        public static void Log(string message)
        {
            LogContainer.text = message;
        }

        /// <summary>
        ///     获取鼠标世界坐标
        /// </summary>
        public static Vector3 GetMousePosition()
        {
            return (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        /// <summary>
        ///     将向量source的方向转至direction的方向
        /// </summary>
        public static Vector2 ChangeVectorDirection(Vector2 source, Vector2 direction)
        {
            return direction.normalized * source.magnitude;
        }

        /// <summary>
        ///     将origin的长度转为length
        /// </summary>
        public static Vector3 ChangeVectorLength(Vector3 origin, float length)
        {
            return origin.normalized * length;
        }

        /// <summary>
        ///     判断两个向量是否近似相似，精确到0.1^precision
        /// </summary>
        public static bool IfVectorAppromxiate(Vector3 v1, Vector3 v2, int n)
        {
            var precisionF = Mathf.Pow(0.1f, n);

            return Mathf.Abs(v1.x - v2.x) < precisionF && Mathf.Abs(v1.y - v2.y) < precisionF;
        }

        public static bool IfVectorAppromxiate(Vector3 v1, Vector3 v2, double precision)
        {
            return Mathf.Abs(v1.x - v2.x) < precision && Mathf.Abs(v1.y - v2.y) < precision;
        }

        /// <summary>
        ///     判断两个向量是否近似相似
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool IfVectorAppromxiate(Vector3 v1, Vector3 v2)
        {
            return
                Mathf.Approximately(v1.x, v2.x) &&
                Mathf.Approximately(v1.y, v2.y) &&
                Mathf.Approximately(v1.z, v2.z);
        }

        /// <summary>
        ///     判断两个数是否近似相似，精确到0.1^precision
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static bool IfAppromxiate(float n1, float n2, double precision)
        {
            return Mathf.Abs(n1 - n2) <= precision;
        }

        /// <summary>
        ///     如果按下任意键
        /// </summary>
        public static bool IfKeyboardAny()
        {
            bool judge;
            if (Input.anyKey && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
                judge = true;
            else
                judge = false;

            return judge;
        }

        /// <summary>
        ///     如果鼠标任意键正在按住
        /// </summary>
        public static bool IfMouseButtonAny()
        {
            return Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
        }

        /// <summary>
        ///     如果按下鼠标任意键
        /// </summary>
        public static bool IfMouseButtonDownAny()
        {
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
        }

        /// <summary>
        ///     向量转欧拉角，针对2D
        /// </summary>
        public static Vector3 Vec2Eul(Vector2 source)
        {
            var degree = Vector2.Angle(source, Vector2.right);
            if (source.y < 0)
                degree = 360 - degree;

            return new Vector3(0, 0, degree);
        }

        /// <summary>
        ///     欧拉角转向量，针对2D
        /// </summary>
        public static Vector3 Eul2Vec(Vector3 source)
        {
            return new Vector3(
                Mathf.Cos(source.z * Mathf.Deg2Rad),
                (float) Math.Sin(source.z * Mathf.Deg2Rad), 0).normalized;
        }

        /// <summary>
        ///     获取游戏物体的所有子物体，包括子物体的子物体
        /// </summary>
        public static Transform[] GetAllGameObject(Transform target)
        {
            var objects = new List<Transform>
            {
                target
            };

            foreach (Transform temp in target) objects.AddRange(GetAllGameObject(temp));

            return objects.ToArray();
        }

        /// <summary>
        ///     在半径为radius的球内产生随机向量
        /// </summary>
        public static Vector3 RandomVectorInSphere(float radius)
        {
            var x = RandomNormal() * (RandomNormal() > 0.5 ? 1 : -1);
            var y = RandomNormal() * (RandomNormal() > 0.5 ? 1 : -1);
            var z = RandomNormal() * (RandomNormal() > 0.5 ? 1 : -1);

            var direction = new Vector3(x, y, z).normalized;
            radius = RandomNormal() * radius;

            return direction * radius;
        }

        /// <summary>
        ///     在半径为radius的圆内产生随机向量
        /// </summary>
        public static Vector3 RandomVectorInCircle(float radius)
        {
            return Quaternion.AngleAxis(RandomFloat(0, 360), Vector3.forward) * Vector3.up * (RandomNormal() * radius);
        }

        /// <summary>
        ///     关闭游戏
        /// </summary>
        public static void ExitGame()
        {
            Application.Quit();
        }

        /// <summary>
        ///     获取所有组件
        /// </summary>
        public static T[] GetAllComponents<T>(Transform[] target) where T:Component
        {
            var spriteT = new List<T>();

            foreach (var temp in target)
            {
                var tempT = temp.GetComponent<T>();

                bool ifGet;
                try
                {
                    ifGet = !tempT.Equals(null);
                }
                catch
                {
                    ifGet = false;
                }

                if (ifGet) spriteT.Add(tempT);
            }

            return spriteT.ToArray();
        }

        /// <summary>
        ///     格式化报错
        /// </summary>
        public static void LogError(GameObject go, string message)
        {
            Debug.LogError(go.name + " : " + message);
        }

        /// <summary>
        ///     格式化报错
        /// </summary>
        public static void LogError(string scriptName, string message)
        {
            Debug.LogError(scriptName + " : " + message);
        }

        /// <summary>
        ///     格式化报错
        /// </summary>
        public static void LogError(GameObject go, string scriptName, string message)
        {
            Debug.LogError(scriptName + " in " + go.name + " : " + message);
        }

        /// <summary>
        ///     检测游戏物体上是否有某组件，若无则报错
        /// </summary>
        public static void DetectCompement<T>(GameObject go) where T : Component
        {
            var compement = go.GetComponent<T>();
            if (compement == null)
                Debug.LogError(compement.GetType() + " isn't attach in " + go.name);
        }

        /// <summary>
        ///     两点间检测所有碰撞射线的物体
        /// </summary>
        public static RaycastHit2D[] Raycast2DBetween(Vector3 from, Vector3 to)
        {
            return Physics2D.RaycastAll(from, to - from, (to - from).magnitude);
        }

        /// <summary>
        ///     获取当前帧率
        /// </summary>
        public static float GetFps()
        {
            return 1 / Time.deltaTime;
        }

        /// <summary>
        ///     从path路径的分割精灵中获取指定段内的sprite数组
        /// </summary>
        /// <param name="path"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Sprite[] GetSliceSprite(string path, int from, int to)
        {
            var temp = Resources.LoadAll<Sprite>(path);
            var result = new List<Sprite>();

            for (var i = from; i <= to; i++) result.Add(temp[i]);
            return result.ToArray();
        }

        /// <summary>
        ///     向target向量添加add向量，限制长度为limit
        /// </summary>
        /// <param name="target"></param>
        /// <param name="add"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static Vector3 VecAddLimit(Vector3 target, Vector3 add, float limit)
        {
            target += add;
            if (target.magnitude > limit && Vector3.Angle(target, add) < 90)
                target = target.normalized * limit;
            return target;
        }

        /// <summary>
        ///     获取指定区间内的整数集合
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static int[] IntInSection(float from, float to)
        {
            var result = new List<int>();

            if (from > to)
            {
                from += to;
                to = from - to;
                from = from - to;
            }

            for (var i = (int) from; i < to; i++) result.Add(i);

            return result.ToArray();
        }

        /// <summary>
        ///     解决分段问题
        /// </summary>
        /// <param name="unitLength">单元长度</param>
        /// <param name="length">总长度</param>
        /// <param name="intervalMax">最长间距</param>
        /// <param name="intervalMin">最短间距</param>
        /// <param name="unitCount">单元数量</param>
        /// <param name="interval">间距</param>
        public static void GetUnitInterval(float unitLength, float length, float intervalMax, float intervalMin,
            out int unitCount, out float interval)
        {
            //   n:最大间距
            //   m:最小间距
            //   L:限宽
            //   a:单元宽度
            //   b:间距
            //   x:单元数量

            // L+n         L+m
            //----- < x < -----    
            // a+n         a+m

            //     L-x*a
            //b = -------
            //      x+1

            var temp = IntInSection(
                (length + intervalMax) / (unitLength + intervalMax),
                (length + intervalMin) / (unitLength + intervalMin));

            unitCount = temp[temp.Length - 1];

            interval = (length - unitCount * unitLength) / (unitCount - 1);
        }

        /// <summary>
        ///     将字符串数组组合为一个字符串，用,隔开
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string CombineStringArray(string[] target)
        {
            var result = "";

            foreach (var temp in target) result = result + "," + temp;

            return result;
        }

        /// <summary>
        ///     判断某点是否在两点构成的直线上
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IfPointOnLine(Vector3 line1, Vector3 line2, Vector3 point)
        {
            if (IfVectorAppromxiate(point, line1, 3) || IfVectorAppromxiate(point, line2, 3))
                return true;

            var temp1 = (line1 - line2).normalized;
            var temp2 = (point - line2).normalized;

            return IfVectorAppromxiate(temp1, temp2, 3) || IfVectorAppromxiate(temp1, -temp2, 3);
        }

        public static bool IfPointOnLine(Vector3 line1, Vector3 line2, Vector3 point, double precision)
        {
            var a = line1.y - line2.y;
            var b = line2.x - line1.x;
            var c = line1.x * line2.y - line2.x * line1.y;

            float distance = -1;

            if (IfAppromxiate(a, 0, 3) && IfAppromxiate(b, 0, 3))
                distance = Mathf.Abs(point.y - distance);
            else
                distance = Mathf.Abs((a * point.x + b * point.y + c) / Mathf.Sqrt(a * a + b * b));
            if (distance < precision)
                return true;
            return false;
        }

        /// <summary>
        ///     判断某点是否在两点构成的线段上
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="point"></param>
        /// <param name="avoidEndpoint"></param>
        /// <returns></returns>
        public static bool IfPointOnSegment(Vector3 line1, Vector3 line2, Vector3 point, bool avoidEndpoint)
        {
            if ((IfVectorAppromxiate(point, line1, 3) || IfVectorAppromxiate(point, line2, 3)) && !avoidEndpoint)
                return true;

            var temp1 = line1 - line2;
            var temp2 = point - line2;

            var sameDir = IfVectorAppromxiate(temp1.normalized, temp2.normalized, 3);

            var inRange = temp1.magnitude > temp2.magnitude;

            return inRange && sameDir;
        }

        public static bool IfPointOnSegment(Vector3 line1, Vector3 line2, Vector3 point, bool avoidEndpoint,
            double precision)
        {
            if (!IfPointOnLine(line1, line2, point, precision)) return false;

            if (IfVectorAppromxiate(point, line1, precision) || IfVectorAppromxiate(point, line2, precision))
            {
                if (avoidEndpoint)
                    return false;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     获取GUID
        /// </summary>
        /// <returns></returns>
        public static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     获取哈希表中的所有值，存入list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<T> HashTableValueToList<T>(Hashtable hash)
        {
            var result = new List<T>();
            foreach (var value in hash.Values) result.Add((T) value);
            return result;
        }

        /// <summary>
        ///     获取哈希表中的所有键，存入list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<T> HashTableKeyToList<T>(Hashtable hash)
        {
            var result = new List<T>();
            foreach (var value in hash.Keys) result.Add((T) value);
            return result;
        }

        /// <summary>
        ///     获取所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetAllFiles(string path)
        {
            if (File.Exists(path))
                return new[] {path};

            if (!Directory.Exists(path))
                return new string[0];

            var result = new List<string>();

            foreach (var tempDirectory in Directory.GetDirectories(path)) result.AddRange(GetAllFiles(tempDirectory));

            result.AddRange(Directory.GetFiles(path));

            return result.ToArray();
        }

        /// <summary>
        ///     获取所有文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetAllDirectories(string path)
        {
            if (File.Exists(path) || !Directory.Exists(path))
                return new string[0];

            var result = new List<string>();

            foreach (var tempDirectory in Directory.GetDirectories(path))
                result.AddRange(GetAllDirectories(tempDirectory));

            result.AddRange(Directory.GetDirectories(path));

            return result.ToArray();
        }

        /// <summary>
        ///     范围内随机的大量点
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static Vector3[] RandomPoints(Vector3 center, float radius, int number)
        {
            var result = new List<Vector3>();
            for (var i = 0; i < number; i++) result.Add(RandomVectorInCircle(radius) + center);
            return result.ToArray();
        }

        /// <summary>
        ///     from向to增加一定的量
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static float AddTo(float from, float to, float distance)
        {
            distance = Mathf.Abs(distance);
            if (Mathf.Approximately(from, to))
                return to;
            if (to - from > 0)
            {
                from += distance;
                if (from > to)
                    from = to;
            }
            else
            {
                from -= distance;
                if (from < to)
                    from = to;
            }

            return from;
        }

        /// <summary>
        ///     向量from向to增加一定的量
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 AddTo(Vector3 from, Vector3 to, float distance)
        {
            var dir = (to - from).normalized;
            if (dir == Vector3.zero)
                return to;
            from += dir * distance;
            if ((to - from).normalized == -dir)
                from = to;

            return from;
        }

        /// <summary>
        ///     向量转向的lerp
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 LerpAngle(Vector3 from, Vector3 to, float t)
        {
            var angle = Mathf.Lerp(0, Vector3.Angle(from, to), t);
            return Eul2Vec(new Vector3(0, 0, Vec2Eul(from).z + angle));
        }

        /// <summary>
        ///     list转string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string List2String<T>(List<T> t, Func<T, object> f)
        {
            var sb = new StringBuilder(100);
            t.ForEach(x => sb.Append("{" + f(x) + "}"));
            return sb.ToString();
        }

        /// <summary>
        ///     向量转过一定的角度
        /// </summary>
        /// <param name="source"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static Vector3 VecTurnDegree(Vector3 source, float degree)
        {
            var temp = Vec2Eul(source);
            temp.z += degree;
            return Eul2Vec(temp) * source.magnitude;
        }

        /// <summary>
        ///     离散化坐标
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector2 Discretization(Vector2 source)
        {
            source.x = (int) (source.x + (source.x >= 0 ? 0.5f : -0.5f));
            source.y = (int) (source.y + (source.y >= 0 ? 0.5f : -0.5f));
            return source;
        }

        /// <summary>
        ///     欧拉角平滑移动
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="smoothTime"></param>
        /// <param name="maxSpeed"></param>
        /// <returns></returns>
        public static Vector3 EulerAngleDamp(Vector3 origin, Vector3 target, float smoothTime, float maxSpeed)
        {
            if (origin.z < 0)
                origin.z += 360;
            if (target.z < 0)
                target.z += 360;

            var temp = new Vector3();
            if (target.z < origin.z)
            {
                if (origin.z - target.z >= 180)
                    return Vector3.SmoothDamp(origin + Vector3.forward * 360, target, ref temp, smoothTime, maxSpeed);
                return Vector3.SmoothDamp(origin, target, ref temp, smoothTime, maxSpeed);
            }

            if (target.z - origin.z >= 180)
                return Vector3.SmoothDamp(origin, target - Vector3.forward * 360, ref temp, smoothTime, maxSpeed);
            return Vector3.SmoothDamp(origin, target, ref temp, smoothTime, maxSpeed);
        }

        /// <summary>
        ///     字符串转向量，形如：x_y_z
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 StringToVector3(string vector)
        {
            var temp = vector.Split('_');
            return new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
        }

        /// <summary>
        ///     首字母大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstCharUpper(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            if (char.IsUpper(str[0])) return str;
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        /// <summary>
        ///     首字母小写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstCharLower(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            if (char.IsLower(str[0])) return str;
            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static void LogTime(object label = null)
        {
            Debug.Log("[" + label + "] " + DateTime.Now.Minute + "m:" + DateTime.Now.Second + "s:" +
                      DateTime.Now.Millisecond + "mi");
        }

        /// <summary>
        ///     创建一个圆环mesh，宽度1，半径1
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public static Mesh CreateRingMesh(int detail)
        {
            var mesh = new Mesh();

            var angle = 360f / detail;
            var vertex = new List<Vector3>();
            for (var i = 0; i < detail; i++)
            {
                var radius = angle * Mathf.Deg2Rad * i;
                var temp = new Vector3(Mathf.Cos(radius), Mathf.Sin(radius));
                vertex.Add(temp * 0.5f);
                vertex.Add(temp * 1.5f);
            }

            var baseSqu = new[] {0, 2, 1, 2, 3, 1};
            var triangles = new List<int>();
            for (var i = 0; i < detail; i++)
                triangles.AddRange(baseSqu.Select(t => (i * 2 + t) % (detail * 2)));

            mesh.vertices = vertex.ToArray();
            mesh.triangles = triangles.ToArray();

            return mesh;
        }

        /// <summary>
        ///     判断该类型是否有指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HaveAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }

        /// <summary>
        ///     判断该属性是否有指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HaveAttribute<T>(this PropertyInfo type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }

        /// <summary>
        ///     判断该字段是否有指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HaveAttribute<T>(this FieldInfo type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }

        /// <summary>
        ///     对词典添加键值对，若键已经存在，则修改对应的值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="forceAdd"></param>
        public static void AddOrModify<K, V>(this Dictionary<K, V> dic, K key, V value, bool forceAdd = false)
        {
            if (dic.TryGetValue(key, out _))
            {
                if (forceAdd) return;
                dic[key] = value;
                return;
            }

            dic.Add(key, value);
        }

        /// <summary>
        ///     从词典中获取值，若键不存在，则添加键和空值并返回
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="forceGet"></param>
        /// <returns></returns>
        public static V GetOrCreate<K, V>(this Dictionary<K, V> dic, K key, bool forceGet = false)
        {
            if (dic.TryGetValue(key, out var result))
                return result;
            result = default;
            if (forceGet) return result;
            dic.Add(key, result);
            return result;
        }

        public static void DrawLine(Vector3 start, Vector3 end)
        {
            Debug.DrawLine(start, end, Color.red);
        }

        /// <summary>
        ///     三维向量转四维，w值为1，表示坐标
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector4 ToV4Pos(this Vector3 pos)
        {
            return new Vector4(pos.x, pos.y, pos.z, 1);
        }

        /// <summary>
        ///     三维向量转四维，w值为0，表示方向
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Vector4 ToV4Dir(this Vector3 dir)
        {
            return new Vector4(dir.x, dir.y, dir.z, 0);
        }

        /// <summary>
        ///     思维向量转三维，使用正交除法，即xyz值均除以w。若w值为0则返回零向量
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector3 ToV3Pos(this Vector4 pos)
        {
            if (pos.w == 0) return default;
            return pos / pos.w;
        }

        /// <summary>
        ///     判断指定坐标是否在摄像机的视野内
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static bool InCameraView(Vector3 pos, Camera camera = null)
        {
            camera = camera ?? Camera.main;
            var result = camera.projectionMatrix * camera.worldToCameraMatrix * pos.ToV4Pos();
            var x = result.x <= result.w && result.x >= -result.w;
            var y = result.y <= result.w && result.y >= -result.w;
            return x && y;
        }

        /// <summary>
        ///     精确地Log一个向量
        /// </summary>
        /// <param name="v"></param>
        public static void LogVector(Vector3 v)
        {
            Debug.Log($"({v.x},{v.y},{v.z})");
        }

        /// <summary>
        ///     将y轴旋转角转向由target向量指定的方向
        /// </summary>
        /// <param name="preLocalEulerAngleY">之前的旋转角</param>
        /// <param name="target">目标向量</param>
        /// <param name="deltaAngle">转动步长</param>
        /// <returns></returns>
        public static float TurnToVectorXz(float preLocalEulerAngleY, Vector3 target, float deltaAngle)
        {
            var targetAngle =
                Mathf.Sign(Vector3.Cross(Vector3.forward, target).y) *
                Vector3.Angle(target, Vector3.forward);
            return Mathf.MoveTowardsAngle(preLocalEulerAngleY, targetAngle, deltaAngle);
        }

        /// <summary>
        ///     递归地查找指定名称的物体，包括根物体
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Transform RecursiveFind(Transform root, string name)
        {
            if (root == null) return null;
            var findTrans = new Stack<Transform>();
            findTrans.Push(root);
            while (findTrans.Count != 0)
            {
                var curTrans = findTrans.Pop();
                if (curTrans.name == name) return curTrans;
                for (var i = 0; i < curTrans.childCount; i++)
                    findTrans.Push(curTrans.GetChild(i));
            }

            return null;
        }

        /// <summary>
        ///     将向量投影到xz平面上
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector3 ProjectToxz(this Vector3 source)
        {
            source.y = 0;
            return source;
        }

        /// <summary>
        ///     将向量投影到xy平面上
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector3 ProjectToxy(this Vector3 source)
        {
            source.z = 0;
            return source;
        }

        /// <summary>
        ///     将向量投影到yz平面上
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector3 ProjectToyz(this Vector3 source)
        {
            source.x = 0;
            return source;
        }

        /// <summary>
        ///     判断下标是否超出数组范围
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool InRange(this Array arr, int index)
        {
            return index >= 0 && index < arr.Length;
        }

        /// <summary>
        ///     获得形如 {x,y} 的字符串
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string GetStr(this Vector2 v)
        {
            return $"({v.x},{v.y})";
        }

        /// <summary>
        ///     获得形如 {x,y,z} 的字符串
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string GetStr(this Vector3 v)
        {
            return $"({v.x},{v.y},{v.z})";
        }

        /// <summary>
        ///     获得形如 {x,y,z,w} 的字符串
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string GetStr(this Vector4 v)
        {
            return $"({v.x},{v.y},{v.z},{v.w})";
        }

        /// <summary>
        ///     获取指定的低维向量
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 Xyz(this Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        /// <summary>
        ///     获取指定的低维向量
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 Xy(this Vector4 v)
        {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        ///     获取指定的低维向量
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 Zw(this Vector4 v)
        {
            return new Vector2(v.z, v.w);
        }

        /// <summary>
        ///     获取指定的低维向量
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 Xy(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        ///     对所有的分量取平均值
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float Average(this Vector2 v)
        {
            return (v.x + v.y) / 2;
        }

        /// <summary>
        ///     对所有的分量取平均值
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float Average(this Vector3 v)
        {
            return (v.x + v.y + v.z) / 2;
        }

        /// <summary>
        ///     对所有的分量取平均值
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float Average(this Vector4 v)
        {
            return (v.x + v.y + v.z + v.w) / 2;
        }

        /// <summary>
        ///     限定于360内
        /// </summary>
        public static float LerpAngle(float a, float b, float t)
        {
            var result = Mathf.LerpAngle(a, b, t);
            return result + (result < 0 ? 360 : 0);
        }

        /// <summary>
        ///     返回给定范围内的随机float值
        /// </summary>
        public static float RandomFloat(float min, float max)
        {
            var temp = (max - min) * RandomNormal();
            return min + temp;
        }

        /// <summary>
        ///     返回给定范围内的随机int值，闭区间
        /// </summary>
        public static int RandomInt(int min, int max)
        {
            return _Random.Next(min, max + 1);
        }

        /// <summary>
        ///     返回0，1之间的float值
        /// </summary>
        public static float RandomNormal()
        {
            return (float) _Random.NextDouble();
        }

        /// <summary>
        ///     施密特正交化，获取新左手坐标系的三个轴
        /// </summary>
        /// <param name="z"></param>
        /// <param name="auxiliary">辅助向量，用于构造坐标系</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void GramSchimidt(Vector3 z, Vector3 auxiliary, out Vector3 x, out Vector3 y)
        {
            z = z.normalized;
            x = (auxiliary - Vector3.Dot(z, auxiliary) * z).normalized;
            y = Vector3.Cross(z, x);
        }

        /// <summary>
        ///     施密特正交化，获取新左手坐标系的变换矩阵
        /// </summary>
        /// <param name="z"></param>
        /// <param name="auxiliary">辅助向量，用于构造坐标系</param>
        /// <param name="o"></param>
        /// <returns>新左手坐标系的变换矩阵</returns>
        public static Matrix4x4 GramSchimidt(Vector3 z, Vector3 auxiliary, Vector3 o = default)
        {
            GramSchimidt(z, auxiliary, out Vector3 x, out Vector3 y);
            return BuildTransferMatrix(x, y, z, o);
        }

        /// <summary>
        ///     根据三个坐标轴以及坐标原点构造变换矩阵
        /// </summary>
        /// <param name="x">轴x</param>
        /// <param name="y">轴y</param>
        /// <param name="z">轴z</param>
        /// <param name="o">坐标原点</param>
        /// <returns></returns>
        public static Matrix4x4 BuildTransferMatrix(Vector3 x, Vector3 y, Vector3 z, Vector3 o = default)
        {
            return new Matrix4x4(new Vector4(x.x, x.y, x.z, 0),
                new Vector4(y.x, y.y, y.z, 0),
                new Vector4(z.x, z.y, z.z, 0),
                new Vector4(o.x, o.y, o.z, 1));
        }

        /// <summary>
        ///     构造简单坐标变换矩阵，坐标系的y轴与世界坐标系平行
        /// </summary>
        /// <param name="z">轴z</param>
        /// <param name="o">坐标原点</param>
        /// <returns></returns>
        public static Matrix4x4 BuildTransferMatrix(Vector3 z, Vector3 o = default)
        {
            var y = Vector3.up;
            z = z.normalized;
            var x = Vector3.Cross(y, z);
            return BuildTransferMatrix(x, y, z, o);
        }

        /// <summary>
        ///     插值，经过clamp处理
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Lerp(float a, float b, float value)
        {
            return Mathf.Lerp(a, b, value);
        }

        /// <summary>
        ///     插值，未经clamp处理
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float LerpUc(float a, float b, float value)
        {
            return Mathf.LerpUnclamped(a, b, value);
        }

        /// <summary>
        ///     插值，经过clamp处理
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Vector3 Lerp(Vector3 a, Vector3 b, float value)
        {
            return new Vector3(Lerp(a.x, b.y, value),
                Lerp(a.y, b.y, value),
                Lerp(a.z, b.z, value));
        }

        /// <summary>
        ///     插值，未经clamp处理
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Vector3 LerpUc(Vector3 a, Vector3 b, float value)
        {
            return new Vector3(LerpUc(a.x, b.y, value),
                LerpUc(a.y, b.y, value),
                LerpUc(a.z, b.z, value));
        }

        /// <summary>
        ///     插值，经过clamp处理
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color Lerp(Color a, Color b, float value)
        {
            return new Color(Lerp(a.r, b.r, value),
                Lerp(a.g, b.g, value),
                Lerp(a.b, b.b, value),
                Lerp(a.a, b.a, value));
        }

        /// <summary>
        ///     插值，未经clamp处理
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color LerpUc(Color a, Color b, float value)
        {
            return new Color(LerpUc(a.r, b.r, value),
                LerpUc(a.g, b.g, value),
                LerpUc(a.b, b.b, value),
                LerpUc(a.a, b.a, value));
        }

        /// <summary>
        ///     反插值，经过clamp处理。ab不可相等
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float InLerp(float a, float b, float value)
        {
            Assert.IsFalse(a == b, $"a:{a} 与 b:{b} 不可相等");
            return Mathf.InverseLerp(a, b, value);
        }

        /// <summary>
        ///     反插值，未经clamp处理。ab不可相等
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float InLerpUc(float a, float b, float value)
        {
            Assert.IsFalse(a == b, $"a:{a} 与 b:{b} 不可相等");
            return (value - a) / (b - a);
        }

        /// <summary>
        ///     获取rect的尺寸
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector2 Size(this Rect rect)
        {
            return new Vector2(rect.width, rect.height);
        }

        /// <summary>
        ///     获取rect的坐标
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector2 Pos(this Rect rect)
        {
            return new Vector2(rect.x, rect.y);
        }

        /// <summary>
        ///     设置rectTransform的长宽
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="size"></param>
        public static void SetSize(this RectTransform rt, Vector3 size)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        /// <summary>
        ///     生成改变固定尺寸的tween
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="endValue"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static Tweener DOFixedSize(this RectTransform rt, Vector2 endValue, float duration)
        {
            return DOTween.To(() => rt.rect.Size(),
                x => rt.SetSize(x),
                endValue,
                duration);
        }

        /// <summary>
        ///     clamp函数的扩展方法形式
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Clamp(this float f, float a, float b)
        {
            return Mathf.Clamp(f, a, b);
        }

        /// <summary>
        ///     clamp01函数的扩展方法形式
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static float Clamp01(this float f)
        {
            return Mathf.Clamp01(f);
        }

        /// <summary>
        ///     clamp函数的扩展方法形式
        /// </summary>
        /// <param name="i"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Clamp(this int i, int a, int b)
        {
            return Mathf.Clamp(i, a, b);
        }

        /// <summary>
        ///     创建一个src尺寸/downSample大小的renderTexture
        /// </summary>
        /// <param name="src"></param>
        /// <param name="downSample"></param>
        /// <returns></returns>
        public static RenderTexture GetTemporary(this RenderTexture src, int downSample = 1)
        {
            return RenderTexture.GetTemporary(src.width / downSample, src.height / downSample, 0);
        }

        /// <summary>
        ///     RenderTexture.ReleaseTemporary的扩展方法形式
        /// </summary>
        /// <param name="src"></param>
        public static void ReleaseTemporary(this RenderTexture src)
        {
            RenderTexture.ReleaseTemporary(src);
        }

        /// <summary>
        ///     独立一个新材质并赋给目标渲染器
        /// </summary>
        /// <param name="re"></param>
        /// <returns></returns>
        public static Material StandaloneMaterial(this Renderer re)
        {
            var newMat = re.material;
            re.material = newMat;
            return newMat;
        }

        /// <summary>
        ///     随机获取数组内的一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static T RandomTake<T>(this T[] arr)
        {
            if (arr == null || arr.Length == 0) return default;
            return arr[RandomInt(0, arr.Length - 1)];
        }

        /// <summary>
        ///     在数组的指定范围内随机获取一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="start">起始下标</param>
        /// <param name="end">结束下标</param>
        /// <returns></returns>
        public static T RandomTake<T>(this T[] arr, int start, int end)
        {
            if (arr == null || arr.Length == 0) return default;
            return arr[RandomInt(start, end)];
        }

        /// <summary>
        ///     获取数组最后一个元素的下标
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static int End<T>(this T[] arr)
        {
            return arr.Length - 1;
        }

        /// <summary>
        ///     向量全相乘
        /// </summary>
        /// <param name="v"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Vector3 Mul(this Vector3 v, Vector3 other)
        {
            return new Vector3(v.x * other.x, v.y * other.y, v.z * other.z);
        }

        /// <summary>
        ///     查找第一个符合条件的对象，返回其下标
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public static int FindIndex<T>(this IEnumerable<T> arr, Func<T, bool> check)
        {
            var count = 0;
            foreach (var unit in arr)
            {
                if (check(unit)) return count;
                count++;
            }

            return -1;
        }

        /// <summary>
        ///     遍历目标迭代器，对于每个元素执行foreachAction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="foreachAction"></param>
        public static void Foreach<T>(this IEnumerable<T> arr, Action<T> foreachAction)
        {
            foreach (var a in arr)
                foreachAction?.Invoke(a);
        }

        /// <summary>
        ///     交换数组中的两个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        public static void Swap<T>(this T[] arr, int index1, int index2)
        {
            var temp = arr[index1];
            arr[index1] = arr[index2];
            arr[index2] = temp;
        }

        /// <summary>
        ///     获取目标物体的一个克隆
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GameObject GetInstantiate(this GameObject obj)
        {
            return Object.Instantiate(obj);
        }

        /// <summary>
        ///     在一个区间内循环
        /// </summary>
        /// <param name="val"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int Repeat(this int val, int length)
        {
            val %= length;
            if (val > 0) return val;
            val += length;
            return val;
        }

        /// <summary>
        ///     在一个区间内循环
        /// </summary>
        /// <param name="val"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static float Repeat(this float val, float length)
        {
            return Mathf.Repeat(val, length);
        }

        /// <summary>
        /// 检查object是否为目标类或其派生类，若非则尝试使用json解析，并输出转型好的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="cast">转型后的对象</param>
        /// <returns></returns>
        public static bool TypeCheck<T>(this object obj,out T cast)
        {
            if (!(obj is T))
            {
                if (obj is string str)
                {
                    if (_JsonCache.TryGet(str,out obj))
                    {
                        cast = (T) obj;
                        return true;
                    }
                    cast = JsonConvert.DeserializeObject<T>(str);
                    _JsonCache.Put(str,cast);
                    return true;
                }

                cast = default;
                return false;
            }

            cast = (T) obj;
            return true;
        }
    }
}