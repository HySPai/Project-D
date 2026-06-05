using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class Helper
{
    public static Camera _camera;
    private static Vector3 RandomVec;

    // tìm đường cong bergen
    public static List<Vector3> FindBergenCurve(Vector3 startPos, Vector3 endPos, float dis)
    {
        Vector3 A, B, C, D;
        A = startPos;
        B = startPos + new Vector3(0, dis, 0);
        C = endPos + new Vector3(0, dis, 0);
        D = endPos;

        float resolution = 0.3f / dis;
        int loops = Mathf.FloorToInt(1f / resolution);
        List<Vector3> listResult = new List<Vector3>();
        for (int i = 1; i <= loops; i++)
        {
            float t = i * resolution;
            Vector3 newPos = DeCasteljausAlgorithm(t);
            listResult.Add(newPos);
        }
        return listResult;

        Vector3 DeCasteljausAlgorithm(float t)
        {
            float oneMinusT = 1f - t;

            //Layer 1
            Vector3 Q = oneMinusT * A + t * B;
            Vector3 R = oneMinusT * B + t * C;
            Vector3 S = oneMinusT * C + t * D;

            //Layer 2
            Vector3 P = oneMinusT * Q + t * R;
            Vector3 T = oneMinusT * R + t * S;

            //Final interpolated position
            Vector3 U = oneMinusT * P + t * T;

            return U;
        }
    }

    private static List<int> listSort = new List<int>();

    public static int GetSort(int min)
    {
        int x = 0;
        for (int i = 0; i < listSort.Count - 1; i++)
        {
            if (listSort[0] > min)
            {
                x = min;
                listSort.Add(x);
                listSort.Sort();
                break;
            }
            if (listSort[i] + 1 >= min && listSort[i] + 1 != listSort[i + 1])
            {
                x = listSort[i] + 1;
                listSort.Add(x);
                listSort.Sort();
                break;
            }
        }
        if (x == 0)
        {
            if (listSort.Count == 0)
            {
                x = min;
            }
            else
            {
                x = listSort[listSort.Count - 1] + 1 >= min ? listSort[listSort.Count - 1] + 1 : min;
            }

            listSort.Add(x);
        }
        return x;
    }

    public static Vector3 GetRandomVecter()
    {
        RandomVec.y = UnityEngine.Random.Range(180, 370);
        return RandomVec;
    }
    public static void listReset() { listSort.Clear(); }

    private static Dictionary<float, WaitForSeconds> waitDictionary = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds GetWait(float time)
    {
        if (waitDictionary.TryGetValue(time, out var wait))
        {
            return wait;
        }

        waitDictionary[time] = new WaitForSeconds(time);
        return waitDictionary[time];
    }

    private static PointerEventData _eventDataCurrentPos;
    private static List<RaycastResult> _result;

    public static bool IsOverUi()
    {
        _eventDataCurrentPos = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        _result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(_eventDataCurrentPos, _result);
        return _result.Count > 0;
    }

    public static T GetEnumValue<T>(int _value) where T : Enum
    {
        T[] values = (T[])Enum.GetValues(typeof(T));

        if (_value < 0)
        {
            _value = values.Length - 1;
        }
        T resul = values[0];
        foreach (T value in values)
        {
            int intValue = Convert.ToInt32(value);
            if (intValue == _value)
            {
                resul = value;
            }
        }
        return resul;
    }

    public static string ConvertNumberToStringUlong(ulong numb)
    {
        ulong Billion = 1000000000;
        ulong Million = 1000000;
        ulong Thousand = 1000;

        string result = "";

        if (numb > Billion * Billion * 10)
        {
            numb = numb / (Billion * Billion);
            result = numb + "BB";
        }
        else if (numb > Million * Billion * 10)
        {
            numb = numb / (Billion * Million);
            result = numb + "MB";
        }
        else if (numb > Thousand * Billion * 10)
        {
            numb = numb / (Billion * Thousand);
            result = numb + "KB";
        }
        else if (numb > Billion * 10)
        {
            numb = numb / Billion;
            result = numb + "B";
        }
        else if (numb > Million * 10)
        {
            numb = numb / Million;
            result = numb + "M";
        }
        else if (numb > Thousand * 10)
        {
            numb = numb / Thousand;
            result = numb + "K";
        }
        else
        {
            result = numb.ToString();
        }
        return result;
    }

    public static string ConvertNumberToString(int numb)
    {
        ulong Thousand = 1000;

        string result = "";

        if (numb > 1000000 * 10)
        {
            numb /= 1000000;
            result = numb + "M";
        }
        else if (numb > 1000 * 10)
        {
            numb /= 1000;
            result = numb + "K";
        }
        else
        {
            result = numb.ToString();
        }
        return result;
    }

    public static ulong[] stringToUlongArray(string input)
    {
        input = input.Replace(",", "");
        List<ulong> _reward = new List<ulong>();
        string[] alls = input.Split(' ');
        foreach (var item in alls)
        {
            ulong _out = 0;
            if (ulong.TryParse(item, out _out))
            {
                _reward.Add(_out);
            }
        }

        return _reward.ToArray();
    }
    public static int[] stringToIntArray(string input)
    {
        input = input.Replace(",", "");
        List<int> _reward = new List<int>();
        string[] alls = input.Split(' ');
        foreach (var item in alls)
        {
            int _out = -1;
            if (int.TryParse(item, out _out))
            {
                _reward.Add(_out);
            }
        }

        return _reward.ToArray();
    }

    public static float[] stringToFloatArray(string input)
    {
        input = input.Replace(",", ".");
        List<float> _reward = new List<float>();
        string[] alls = input.Split(' ');
        foreach (var item in alls)
        {
            float _out = -1;
            if (float.TryParse(item, out _out))
            {
                _reward.Add(_out);
            }
        }

        return _reward.ToArray();
    }


    public static bool DifferentDate(string _oldTime)
    {
        var tempOff = Convert.ToInt64(_oldTime);
        var oldTime = DateTime.FromBinary(tempOff);
        var current = DateTime.Now;
        if (oldTime.Day != current.Day || oldTime.Month != current.Month || oldTime.Year != current.Year)
        {
            return true;
        }
        return false;
    }
    public static DateTime GetNextMonday(DateTime dateTime)
    {
        DateTime nextMonday;
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)dateTime.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0)
        {
            nextMonday = dateTime.AddDays(7).Date;
        }
        else
        {
            nextMonday = dateTime.AddDays(daysUntilMonday).Date;
        }

        return nextMonday;
    }

    public static float DifferentTimeNowSecond(string _oldTime)
    {
        var tempOff = Convert.ToInt64(_oldTime);
        var oldTime = DateTime.FromBinary(tempOff);
        var current = DateTime.Now;
        var diff = current.Subtract(oldTime);
        return (float)diff.TotalSeconds;
    }

    public static float DifferentTimeNowMinute(string _oldTime)
    {
        var tempOff = Convert.ToInt64(_oldTime);
        var oldTime = DateTime.FromBinary(tempOff);
        var current = DateTime.Now;
        var diff = current.Subtract(oldTime);
        return (float)diff.TotalMinutes;
    }

    public static float DifferentTimeNowDate(string _oldTime)
    {
        var tempOff = Convert.ToInt64(_oldTime);
        var oldTime = DateTime.FromBinary(tempOff);
        var current = DateTime.Now;
        var diff = current.Subtract(oldTime);
        return (float)diff.TotalDays;
    }

    public static int Offline(string dateTime)
    {
        if (dateTime == string.Empty)
        {
            return 0;
        }
        return (int)Math.Round(System.DateTime.Now.Subtract(System.Convert.ToDateTime(dateTime)).TotalSeconds);
    }
    public static string ConvertToRealTime(float remainingTime, float timeDay)
    {
        // Giới hạn thời gian trong khoảng 1-200
        remainingTime = Mathf.Clamp(remainingTime, 0f, timeDay);

        // Tính toán số phút từ 9:00 sáng
        // 200 giây = 12 tiếng = 720 phút
        float totalMinutes = (timeDay - remainingTime) * (720f / timeDay);

        // Tính giờ và phút
        int hours = 9 + (int)(totalMinutes / 60f); // Bắt đầu từ 9 giờ sáng
        int minutes = (int)(totalMinutes % 60f);

        // Xác định AM/PM
        string period = "AM";
        if (hours >= 12)
        {
            period = "PM";
            if (hours > 12)
            {
                hours -= 12;
            }
        }

        // Format thời gian với AM/PM
        return $"{hours:00}:{minutes:00} {period}";
    }
    public static string ConvertToRealTime2(float remainingTime)
    {
        // Tính giờ và phút
        int hours = (int)(remainingTime / 60f); // Bắt đầu từ 9 giờ sáng
        int minutes = (int)(remainingTime % 60f);

        // Format thời gian với AM/PM
        return $"{hours:00}:{minutes:00}";
    }

    public static string ConvertToMinutes(float remainingTime)
    {
        int minutes = (int)(remainingTime / 60f);
        int seconds = (int)(remainingTime % 60f);

        // Format thời gian với AM/PM
        return $"{minutes:00}:{seconds:00}";
    }

    public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera.main, out var result);
        return result;
    }


    public static float DistanceSqrt(this Vector3 vec, Vector3 tar)
    {
        return (tar.x - vec.x) * (tar.x - vec.x) + (tar.y - vec.y) * (tar.y - vec.y) + (tar.z - vec.z) * (tar.z - vec.z);
    }

    // Check 2 đường thẳng giao nhau
    public static bool AreLinesIntersecting(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2)
    {
        // Tính toán các hệ số
        float denominator = (q2.z - q1.z) * (p2.x - p1.x) - (q2.x - q1.x) * (p2.z - p1.z);
        float numerator1 = (q2.x - q1.x) * (p1.z - q1.z) - (q2.z - q1.z) * (p1.x - q1.x);
        float numerator2 = (p2.x - p1.x) * (p1.z - q1.z) - (p2.z - p1.z) * (p1.x - q1.x);

        // Kiểm tra xem hai đoạn thẳng có giao nhau không
        if (denominator == 0)
        {
            return numerator1 == 0 && numerator2 == 0;
        }

        // Kiểm tra xem điểm giao nhau nằm trong khoảng của cả hai đoạn thẳng
        float r = numerator1 / denominator;
        float s = numerator2 / denominator;

        return r >= 0 && r <= 1 && s >= 0 && s <= 1;
    }

    public static bool CheckIntersection(Vector3 previousPos, Vector3 currentPos, Vector3[] linePositions)
    {
        for (int i = 0; i < linePositions.Length - 1; i++)
        {
            Vector3 lineStart = linePositions[i];
            Vector3 lineEnd = linePositions[i + 1];

            if (AreLinesIntersecting(previousPos, currentPos, lineStart, lineEnd))
            {
                return true;
            }
        }

        return false;
    }

    public static Vector3 CustomNormalize(this Vector3 v)
    {
        double m = System.Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        Debug.Log(m);

        if (m > 9.99999974737875E-06)
        {
            float fm = (float)m;
            v.x /= fm;
            v.y /= fm;
            v.z /= fm;
            return v;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public static double DistanceCustomOutNormalize(this Vector3 v, Vector3 tar, out Vector3 _result)
    {
        double m = System.Math.Sqrt((tar.x - v.x) * (tar.x - v.x) + (tar.y - v.y) * (tar.y - v.y) + (tar.z - v.z) * (tar.z - v.z));
        _result = Vector3.zero;
        if (m > 0.4f)
        {
            float fm = (float)m;
            v.x = (tar.x - v.x) / fm;
            v.y = (tar.y - v.y) / fm;
            v.z = (tar.z - v.z) / fm;
            _result = v;
        }
        return m;

    }
    public static bool CustomOutNormalize(this Vector3 v, Vector3 tar, out Vector3 _result)
    {
        double m = System.Math.Sqrt((tar.x - v.x) * (tar.x - v.x) + (tar.y - v.y) * (tar.y - v.y) + (tar.z - v.z) * (tar.z - v.z));
        _result = Vector3.zero;
        if (m > 0.4f)
        {
            float fm = (float)m;
            v.x = (tar.x - v.x) / fm;
            v.y = (tar.y - v.y) / fm;
            v.z = (tar.z - v.z) / fm;
            _result = v;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void SetGameLayerRecursive(GameObject _go, int _layer)
    {
        _go.layer = _layer;
        foreach (Transform child in _go.transform)
        {
            child.gameObject.layer = _layer;

            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
            {
                SetGameLayerRecursive(child.gameObject, _layer);
            }
        }
    }


    public static IEnumerator AnimText(Text txt, int x, int start, int end)
    {
        //yield return Helper.GetWait(1.5f);
        start += (x / 5);
        txt.text = ConvertNumberToString(start);
        txt.transform.DOScale(1, 0.15f).From(2);
        yield return GetWait(0.15f);
        start += (x / 5);
        txt.text = ConvertNumberToString(start);
        txt.transform.DOScale(1, 0.15f).From(2);
        yield return GetWait(0.15f);
        start += (x / 5);
        txt.text = ConvertNumberToString(start);
        txt.transform.DOScale(1, 0.15f).From(2);
        yield return GetWait(0.15f);
        start += (x / 5);
        txt.text = ConvertNumberToString(start);
        txt.transform.DOScale(1, 0.15f).From(2);
        yield return GetWait(0.15f);
        start = end;
        txt.text = ConvertNumberToString(start);
        txt.transform.DOScale(1, 0.15f).From(2);
    }


    #region LIST
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }

    public static T GetOneRandom<T>(this IList<T> list)
    {
        // Thay rng.Next() bằng Random.Range() để lấy giá trị ngẫu nhiên
        return list[UnityEngine.Random.Range(0, list.Count)]; // Random chọn giá trị ngẫu nhiên trong dải [0, list.Count)
    }


    // Phương thức Last không thay đổi vì nó chỉ lấy phần tử cuối cùng trong danh sách
    public static T Last<T>(this IList<T> list)
    {
        return list[list.Count - 1];
    }

    // Lấy 1 lượng phần tử ngẫu nhiên trong 1 list
    public static List<T> GetRandomElements<T>(this List<T> list, int elementsCount)
    {
        // Không thay đổi cách sắp xếp ngẫu nhiên
        return list.OrderBy(arg => Guid.NewGuid()).Take(list.Count < elementsCount ? list.Count : elementsCount)
            .ToList();
    }

    #endregion


    #region Debug Log
    public static void LogEditor(this string str)
    {
#if UNITY_EDITOR
        Debug.Log(str);
#endif
    }
    public static void LogErrorEditor(this string str)
    {
#if UNITY_EDITOR
        Debug.LogError(str);
#endif
    }
    public static void LogWarningEditor(this string str)
    {
#if UNITY_EDITOR
        Debug.LogWarning(str);
#endif
    }

    #endregion
    #region UI
    public static bool IsUi
    {
        get
        {
            if (Application.isMobilePlatform)
                return EventSystem.current.IsPointerOverGameObject(0);
            else
                return EventSystem.current.IsPointerOverGameObject();
        }
    }
    /// <summary>
    /// Calulates Position for RectTransform.position from a transform.position. Does not Work with WorldSpace Canvas!
    /// </summary>
    /// <param name="_Canvas"> The Canvas parent of the RectTransform.</param>
    /// <param name="_Position">Position of in world space of the "Transform" you want the "RectTransform" to be.</param>
    /// <param name="_Cam">The Camera which is used. Note this is useful for split screen and both RenderModes of the Canvas.</param>
    /// <returns></returns>
    public static Vector3 CalculatePositionFromTransformToRectTransform(this Canvas _Canvas, Vector3 _Position, Camera _Cam)
    {
        Vector3 Return = Vector3.zero;
        if (_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Return = _Cam.WorldToScreenPoint(_Position);
        }
        else if (_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 tempVector = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_Canvas.transform as RectTransform, _Cam.WorldToScreenPoint(_Position), _Cam, out tempVector);
            Return = _Canvas.transform.TransformPoint(tempVector);
        }

        return Return;
    }

    /// <summary>
    /// Calulates Position for RectTransform.position Mouse Position. Does not Work with WorldSpace Canvas!
    /// </summary>
    /// <param name="_Canvas">The Canvas parent of the RectTransform.</param>
    /// <param name="_Cam">The Camera which is used. Note this is useful for split screen and both RenderModes of the Canvas.</param>
    /// <returns></returns>
    public static Vector3 CalculatePositionFromMouseToRectTransform(this Canvas _Canvas, Camera _Cam)
    {
        Vector3 Return = Vector3.zero;

        if (_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Return = Input.mousePosition;
        }
        else if (_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 tempVector = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_Canvas.transform as RectTransform, Input.mousePosition, _Cam, out tempVector);
            Return = _Canvas.transform.TransformPoint(tempVector);
        }

        return Return;
    }

    /// <summary>
    /// Calculates Position for "Transform".position from a "RectTransform".position. Does not Work with WorldSpace Canvas!
    /// </summary>
    /// <param name="_Canvas">The Canvas parent of the RectTransform.</param>
    /// <param name="_Position">Position of the "RectTransform" UI element you want the "Transform" object to be placed to.</param>
    /// <param name="_Cam">The Camera which is used. Note this is useful for split screen and both RenderModes of the Canvas.</param>
    /// <returns></returns>
    public static Vector3 CalculatePositionFromRectTransformToTransform(this Canvas _Canvas, Vector3 _Position, Camera _Cam)
    {
        Vector3 Return = Vector3.zero;
        if (_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Return = _Cam.ScreenToWorldPoint(_Position);
        }
        else if (_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_Canvas.transform as RectTransform, _Cam.WorldToScreenPoint(_Position), _Cam, out Return);
        }
        return Return;
    }

    public static void SnapTo(ScrollRect scrollRect, RectTransform contentPanel, RectTransform target)
    {
        Canvas.ForceUpdateCanvases();
        int countChild = contentPanel.childCount;
        int siblingIndex = target.GetSiblingIndex();
        float normalize = 1 - ((float)(siblingIndex + 1) / countChild);
        if (scrollRect.vertical)
            scrollRect.verticalNormalizedPosition = normalize;
        if (scrollRect.horizontal)
            scrollRect.horizontalNormalizedPosition = normalize;
    }


    #endregion
}
