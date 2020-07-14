using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Thunder.Tool
{
    public static class Tools
    {
        public static Text logContainer;

        public const long bigNumber = 9223372036854775;

        public static Vector3 screenMiddle = new Vector3(Screen.width / 2, Screen.height / 2);

        public static Vector2 farPosition = new Vector2(bigNumber, bigNumber);

        public static void Log(string message)
        {
            logContainer.text = message;
        }

        ///<summary>
        ///获取鼠标世界坐标
        ///</summary>
        public static Vector3 GetMousePosition()
        {
            return (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        ///<summary>
        ///将向量source的方向转至direction的方向
        ///</summary>
        public static Vector2 ChangeVectorDirection(Vector2 source, Vector2 direction)
        {
            return direction.normalized * source.magnitude;
        }

        ///<summary>
        ///将origin的长度转为length
        ///</summary>
        public static Vector3 ChangeVectorLength(Vector3 origin, float length)
        {
            return origin.normalized * length;
        }

        ///<summary>
        ///判断两个向量是否近似相似，精确到0.1^precision
        ///</summary>
        public static bool IfVectorAppromxiate(Vector3 v_1, Vector3 v_2, int n)
        {
            float precision_f = Mathf.Pow(0.1f, n);

            return (Mathf.Abs(v_1.x - v_2.x) < precision_f && Mathf.Abs(v_1.y - v_2.y) < precision_f);
        }
        public static bool IfVectorAppromxiate(Vector3 v_1, Vector3 v_2, double precision)
        {
            return (Mathf.Abs(v_1.x - v_2.x) < precision && Mathf.Abs(v_1.y - v_2.y) < precision);
        }

        /// <summary>
        /// 判断两个向量是否近似相似
        /// </summary>
        /// <param name="v_1"></param>
        /// <param name="v_2"></param>
        /// <returns></returns>
        public static bool IfVectorAppromxiate(Vector3 v_1, Vector3 v_2)
        {
            return
                Mathf.Approximately(v_1.x, v_2.x) &&
                Mathf.Approximately(v_1.y, v_2.y) &&
                Mathf.Approximately(v_1.z, v_2.z);
        }

        /// <summary>
        /// 判断两个数是否近似相似，精确到0.1^precision
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static bool IfAppromxiate(float n1, float n2, double precision)
        {
            return Mathf.Abs(n1 - n2) <= precision;
        }

        ///<summary>
        ///如果按下任意键
        ///</summary>
        public static bool IfKeyboardAny()
        {
            bool judge;
            if (Input.anyKey && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
                judge = true;
            else
                judge = false;

            return judge;
        }

        ///<summary>
        ///如果鼠标任意键正在按住
        ///</summary>
        public static bool IfMouseButtonAny()
        {
            return Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
        }

        ///<summary>
        ///如果按下鼠标任意键
        ///</summary>
        public static bool IfMouseButtonDownAny()
        {
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
        }

        ///<summary>
        ///向量转欧拉角，针对2D
        ///</summary>
        public static Vector3 Vec2Eul(Vector2 source)
        {
            float degree = Vector2.Angle(source, Vector2.right);
            if (source.y < 0)
                degree = 360 - degree;

            return new Vector3(0, 0, degree);
        }

        ///<summary>
        ///欧拉角转向量，针对2D
        ///</summary>
        public static Vector3 Eul2Vec(Vector3 source)
        {
            return new Vector3(
                (float)Mathf.Cos(source.z * Mathf.Deg2Rad),
                (float)Math.Sin(source.z * Mathf.Deg2Rad), 0).normalized;
        }

        ///<summary>
        ///获取游戏物体的所有子物体，包括子物体的子物体
        ///</summary>
        public static Transform[] GetAllGameObject(Transform target)
        {
            List<Transform> objects = new List<Transform>
            {
                target
            };

            foreach (Transform temp in target)
            {
                objects.AddRange(GetAllGameObject(temp));
            }

            return objects.ToArray();
        }

        ///<summary>
        ///在半径为radius的球内产生随机向量
        ///</summary>
        public static Vector3 RandomVectorInSphere(float radius)
        {
            Vector3 direction;

            float x = (float)GenerateRandomD() * (GenerateRandomD() > 0.5 ? 1 : -1);
            float y = (float)GenerateRandomD() * (GenerateRandomD() > 0.5 ? 1 : -1);
            float z = (float)GenerateRandomD() * (GenerateRandomD() > 0.5 ? 1 : -1);

            direction = new Vector3(x, y, z).normalized;
            radius = (float)GenerateRandomD() * radius;

            return direction * radius;
        }

        ///<summary>
        ///在半径为radius的圆内产生随机向量
        ///</summary>
        public static Vector3 RandomVectorInCircle(float radius)
        {
            return Quaternion.AngleAxis(GenerateRandom(360, 0), Vector3.forward) * Vector3.up * ((float)GenerateRandomD() * radius);
        }

        ///<summary>
        ///关闭游戏
        ///</summary>
        public static void ExitGame()
        {
            Application.Quit();
        }

        ///<summary>
        ///获取所有组件
        ///</summary>
        public static T[] GetAllComponents<T>(Transform[] target)
        {
            List<T> spriteT = new List<T>();

            T tempT;

            bool ifGet = false;

            foreach (Transform temp in target)
            {
                tempT = temp.GetComponent<T>();

                try
                {
                    ifGet = !tempT.Equals(null);
                }
                catch
                {
                    ifGet = false;
                }

                if (ifGet)
                {
                    spriteT.Add(tempT);
                }
            }

            return spriteT.ToArray();
        }

        ///<summary>
        ///产生[min,max]范围内的随机数
        ///</summary>
        public static float GenerateRandom(int max, int min)
        {
            return new global::System.Random(Guid.NewGuid().GetHashCode()).Next(min, max + 1);
        }

        ///<summary>
        ///产生[0,1]内的double数
        ///</summary>
        public static double GenerateRandomD()
        {
            return new global::System.Random(global::System.Guid.NewGuid().GetHashCode()).NextDouble();
        }

        ///<summary>
        ///格式化报错
        ///</summary>
        public static void LogError(GameObject go, string message)
        {
            Debug.LogError(go.name + " : " + message);
        }
        ///<summary>
        ///格式化报错
        ///</summary>
        public static void LogError(string scriptName, string message)
        {
            Debug.LogError(scriptName + " : " + message);
        }
        ///<summary>
        ///格式化报错
        ///</summary>
        public static void LogError(GameObject go, string scriptName, string message)
        {
            Debug.LogError(scriptName + " in " + go.name + " : " + message);
        }

        ///<summary>
        ///检测游戏物体上是否有某组件，若无则报错
        ///</summary>
        public static void DetectCompement<T>(GameObject go)
        {
            T compement = go.GetComponent<T>();
            if (compement == null)
                Debug.LogError(compement.GetType().ToString() + " isn't attach in " + go.name);
        }

        ///<summary>
        ///vector2转vector3
        ///</summary>
        public static Vector3 V2tV3(Vector2 v2)
        {
            return new Vector3(v2.x, v2.y, 0);
        }

        ///<summary>
        ///两点间检测所有碰撞射线的物体
        ///</summary>
        public static RaycastHit2D[] Raycast2DBetween(Vector3 from, Vector3 to)
        {
            return Physics2D.RaycastAll(from, to - from, (to - from).magnitude);
        }

        /// <summary>
        /// 获取当前帧率
        /// </summary>
        public static float GetFps()
        {
            return 1 / Time.deltaTime;
        }

        /// <summary>
        /// 获取输入数的正负号
        /// </summary>
        /// <param name="number"></param>
        /// <returns>正为1，负为-1，零为0</returns>
        public static int GetSign(float number)
        {
            if (Mathf.Approximately(number, 0))
            {
                return 0;
            }

            return (int)(Mathf.Abs(number) / number);
        }

        /// <summary>
        /// 从path路径的分割精灵中获取指定段内的sprite数组
        /// </summary>
        /// <param name="path"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Sprite[] GetSliceSprite(string path, int from, int to)
        {
            Sprite[] temp = Resources.LoadAll<Sprite>(path);
            List<Sprite> result = new List<Sprite>();

            for (int i = from; i <= to; i++)
            {
                result.Add(temp[i]);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 向target向量添加add向量，限制长度为limit
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
        /// 获取指定区间内的整数集合
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static int[] IntInSection(float from, float to)
        {
            List<int> result = new List<int>();

            if (from > to)
            {
                from += to;
                to = from - to;
                from = from - to;
            }

            for (int i = (int)from; i < to; i++)
            {
                result.Add(i);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 解决分段问题
        /// </summary>
        /// <param name="unitLength">单元长度</param>
        /// <param name="length">总长度</param>
        /// <param name="intervalMax">最长间距</param>
        /// <param name="intervalMin">最短间距</param>
        /// <param name="unitCount">单元数量</param>
        /// <param name="interval">间距</param>
        public static void GetUnitInterval(float unitLength, float length, float intervalMax, float intervalMin, out int unitCount, out float interval)
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

            int[] temp = Tools.IntInSection(
                (length + intervalMax) / (unitLength + intervalMax),
                (length + intervalMin) / (unitLength + intervalMin));

            unitCount = temp[temp.Length - 1];

            interval = (length - unitCount * unitLength) / (unitCount - 1);
        }

        /// <summary>
        /// 将字符串数组组合为一个字符串，用,隔开
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string CombineStringArray(string[] target)
        {
            string result = "";

            foreach (string temp in target)
            {
                result = result + "," + temp;
            }

            return result;
        }

        /// <summary>
        /// 判断某点是否在两点构成的直线上
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IfPointOnLine(Vector3 line1, Vector3 line2, Vector3 point)
        {
            if (IfVectorAppromxiate(point, line1, 3) || IfVectorAppromxiate(point, line2, 3))
                return true;

            Vector3 temp1 = (line1 - line2).normalized;
            Vector3 temp2 = (point - line2).normalized;

            return IfVectorAppromxiate(temp1, temp2, 3) || IfVectorAppromxiate(temp1, -temp2, 3);
        }
        public static bool IfPointOnLine(Vector3 line1, Vector3 line2, Vector3 point, double precision)
        {
            float a = line1.y - line2.y;
            float b = line2.x - line1.x;
            float c = line1.x * line2.y - line2.x * line1.y;

            float distance = -1;

            if (IfAppromxiate(a, 0, 3) && IfAppromxiate(b, 0, 3))
            {
                distance = Mathf.Abs(point.y - distance);
            }
            else
            {
                distance = Mathf.Abs((a * point.x + b * point.y + c) / Mathf.Sqrt(a * a + b * b));
            }
            if (distance < precision)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 判断某点是否在两点构成的线段上
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IfPointOnSegment(Vector3 line1, Vector3 line2, Vector3 point, bool avoidEndpoint)
        {
            if ((IfVectorAppromxiate(point, line1, 3) || IfVectorAppromxiate(point, line2, 3)) && !avoidEndpoint)
                return true;

            Vector3 temp1 = (line1 - line2);
            Vector3 temp2 = (point - line2);

            bool sameDir = IfVectorAppromxiate(temp1.normalized, temp2.normalized, 3);

            bool inRange = temp1.magnitude > temp2.magnitude;

            return inRange && sameDir;
        }
        public static bool IfPointOnSegment(Vector3 line1, Vector3 line2, Vector3 point, bool avoidEndpoint, double precision)
        {
            if (!IfPointOnLine(line1, line2, point, precision))
            {
                return false;
            }

            if ((IfVectorAppromxiate(point, line1, precision) || IfVectorAppromxiate(point, line2, precision)))
            {
                if (avoidEndpoint)
                    return false;
                else
                    return true;
            }

            return false;

        }

        /// <summary>
        /// 获取GUID
        /// </summary>
        /// <returns></returns>
        public static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 获取哈希表中的所有值，存入list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<T> HashTableValueToList<T>(Hashtable hash)
        {
            List<T> result = new List<T>();
            foreach (object value in hash.Values)
            {
                result.Add((T)value);
            }
            return result;
        }

        /// <summary>
        /// 获取哈希表中的所有键，存入list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<T> HashTableKeyToList<T>(Hashtable hash)
        {
            List<T> result = new List<T>();
            foreach (object value in hash.Keys)
            {
                result.Add((T)value);
            }
            return result;
        }

        /// <summary>
        /// 获取所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetAllFiles(string path)
        {
            if (File.Exists(path))
                return new string[] { path };

            if (!Directory.Exists(path))
                return new string[0];

            List<string> result = new List<string>();

            foreach (string tempDirectory in Directory.GetDirectories(path))
            {
                result.AddRange(GetAllFiles(tempDirectory));
            }

            result.AddRange(Directory.GetFiles(path));

            return result.ToArray();
        }

        /// <summary>
        /// 获取所有文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetAllDirectories(string path)
        {
            if (File.Exists(path) || !Directory.Exists(path))
                return new string[0];

            List<string> result = new List<string>();

            foreach (string tempDirectory in Directory.GetDirectories(path))
            {
                result.AddRange(GetAllDirectories(tempDirectory));
            }

            result.AddRange(Directory.GetDirectories(path));

            return result.ToArray();
        }

        /// <summary>
        /// Log向量
        /// </summary>
        /// <param name="vec"></param>
        public static void LogVector(Vector3 vec)
        {
            Debug.Log("(" + vec.x + "," + vec.y + "," + vec.z + ")");
        }

        /// <summary>
        /// 范围内随机的大量点
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static Vector3[] RandomPoints(Vector3 center, float radius, int number)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < number; i++)
            {
                result.Add(Tools.RandomVectorInCircle(radius) + center);
            }
            return result.ToArray();
        }

        /// <summary>
        /// from向to增加一定的量
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
        /// 向量from向to增加一定的量
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 AddTo(Vector3 from, Vector3 to, float distance)
        {
            Vector3 dir = (to - from).normalized;
            if (dir == Vector3.zero)
                return to;
            from += dir * distance;
            if ((to - from).normalized == -dir)
                from = to;

            return from;
        }

        /// <summary>
        /// 向量转向的lerp
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 LerpAngle(Vector3 from, Vector3 to, float t)
        {
            float angle = Mathf.Lerp(0, Vector3.Angle(from, to), t);
            return Eul2Vec(new Vector3(0, 0, Vec2Eul(from).z + angle));
        }

        /// <summary>
        /// list转string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string List2String<T>(List<T> t, Func<T, object> f)
        {
            StringBuilder sb = new StringBuilder(100);
            t.ForEach(x => sb.Append("{" + f(x).ToString() + "}"));
            return sb.ToString();
        }

        /// <summary>
        /// 向量转过一定的角度
        /// </summary>
        /// <param name="source"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static Vector3 VecTurnDegree(Vector3 source, float degree)
        {
            Vector3 temp = Vec2Eul(source);
            temp.z += degree;
            return Eul2Vec(temp) * source.magnitude;
        }

        /// <summary>
        /// 离散化坐标
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector2 Discretization(Vector2 source)
        {
            source.x = (int)(source.x + (source.x >= 0 ? 0.5f : -0.5f));
            source.y = (int)(source.y + (source.y >= 0 ? 0.5f : -0.5f));
            return source;
        }

        /// <summary>
        /// 欧拉角平滑移动
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="smoothTime"></param>
        /// <returns></returns>
        public static Vector3 EulerAngleDamp(Vector3 origin, Vector3 target, float smoothTime, float maxSpeed)
        {
            if (origin.z < 0)
                origin.z += 360;
            if (target.z < 0)
                target.z += 360;

            Vector3 temp = new Vector3();
            if (target.z < origin.z)
            {
                if (origin.z - target.z >= 180)
                    return Vector3.SmoothDamp(origin + Vector3.forward * 360, target, ref temp, smoothTime, maxSpeed);
                else
                    return Vector3.SmoothDamp(origin, target, ref temp, smoothTime, maxSpeed);
            }
            else
            {
                if (target.z - origin.z >= 180)
                    return Vector3.SmoothDamp(origin, target - Vector3.forward * 360, ref temp, smoothTime, maxSpeed);
                else
                    return Vector3.SmoothDamp(origin, target, ref temp, smoothTime, maxSpeed);
            }
        }

        /// <summary>
        /// 字符串转向量，形如：x_y_z
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 StringToVector3(string vector)
        {
            string[] temp = vector.Split('_');
            return new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstCharUpper(string str)
        {
            if (str == null || str == "")
                return str;
            if (char.IsUpper(str[0])) return str;
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// 首字母小写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstCharLower(string str)
        {
            if (str == null || str == "")
                return str;
            if (char.IsLower(str[0])) return str;
            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static void LogTime(object label = null)
        {
            Debug.Log("[" + label.ToString() + "] " + DateTime.Now.Minute + "m:" + DateTime.Now.Second + "s:" + DateTime.Now.Millisecond + "mi");
        }

        /// <summary>
        /// 创建一个圆环mesh，宽度1，半径1
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public static Mesh CreateRingMesh(int detail)
        {
            Mesh mesh = new Mesh();

            float angle = 360 / detail;
            List<Vector3> vertex = new List<Vector3>();
            for (int i = 0; i < detail; i++)
            {
                float radius = angle * Mathf.Deg2Rad * i;
                Vector3 temp = new Vector3(Mathf.Cos(radius), Mathf.Sin(radius));
                vertex.Add(temp * 0.5f);
                vertex.Add(temp * 1.5f);
            }

            int[] baseSqu = new int[] { 0, 2, 1, 2, 3, 1 };
            List<int> triangles = new List<int>();
            for (int i = 0; i < detail; i++)
                for (int j = 0; j < baseSqu.Length; j++)
                    triangles.Add((i * 2 + baseSqu[j]) % (detail * 2));

            mesh.vertices = vertex.ToArray();
            mesh.triangles = triangles.ToArray();

            return mesh;
        }

        public static bool HaveAttribute<T>(this Type type,bool inherit=false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length >0;
        }

        public static bool HaveAttribute<T>(this PropertyInfo type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }

        public static bool HaveAttribute<T>(this FieldInfo type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }

        public static void AddOrModify<K, V>(this Dictionary<K, V> dic, K key, V value,bool forceAdd=false)
        {
            if (dic.TryGetValue(key, out _))
            {
                if (forceAdd) return;
                dic[key] = value;
                return;
            }
            dic.Add(key,value);
        }

        public static V GetOrCreate<K, V>(this Dictionary<K, V> dic, K key,bool forceGet=false)
        {
            if (dic.TryGetValue(key, out var result))
                return result;
            result = default;
            if (forceGet) return result;
            dic.Add(key, result);
            return result;
        }

        public static void DrawLine(Vector3 start,Vector3 end)
        {
            Debug.DrawLine(start,end, Color.red);
        }

        public static Vector4 ToV4Pos(this Vector3 pos)
        {
            return new Vector4(pos.x,pos.y,pos.z,1);
        }

        public static Vector4 ToV4Dir(this Vector3 dir)
        {
            return new Vector4(dir.x, dir.y, dir.z, 0);
        }

        public static Vector3 ToV3Pos(this Vector4 pos)
        {
            if (pos.w == 0) return default;
            return pos/pos.w;
        }
    }
}