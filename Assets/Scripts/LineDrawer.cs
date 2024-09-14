using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineDrawer
{
    static Transform root = null;
    public static void bindRoot(Transform root)
    {
        LineDrawer.root = root;

    }
    public static LineRenderer DrawLine(Vector2 start, Vector2 end, LineRenderer line = null)
    {
        if (line == null)
            line = Get();
        line.positionCount = 2;
        line.SetPositions(new Vector3[] { new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0) });
        return line;
    }
    public static LineRenderer Get()
    {
        GameObject go = GlobalRef.Ins.linePrefab.OPGet();
        go.transform.parent = root;
        go.transform.localScale = new Vector3(1, 1, 1);
        LineRenderer line = go.GetComponent<LineRenderer>();
        return line;

    }
    public static void Clear(LineRenderer line)
    {
        if (line)
            line.gameObject.OPPush();
    }
    public static LineRenderer DrawSegmentLines(List<Vector2> v2, LineRenderer line = null)
    {
        if (line == null)
            line = Get();

        line.positionCount = v2.Count;
        line.SetPositions(v2.Select(e => new Vector3(e.x, e.y, 0)).ToArray());
        return line;
    }
    public static List<Vector2> GetCircle(Vector2 center, Vector2 start, float rad, float smooth, LineRenderer line = null)
    {

        float radius = Vector2.Distance(center, start);
        int cnt = Mathf.FloorToInt(2 * Mathf.PI * radius / smooth * (rad / Mathf.PI * 2));
        float radStep = rad / cnt;
        float startRad = (start - center).Ang() * Mathf.Deg2Rad;

        List<Vector2> points = new List<Vector2>();

        float curRad = startRad;
        for (int i = 0; i < cnt; i++)
        {
            points.Add(center + new Vector2(Mathf.Cos(curRad), Mathf.Sin(curRad)) * radius);
            curRad += radStep;
        }
        points.Add(center + new Vector2(Mathf.Cos(curRad), Mathf.Sin(curRad)) * radius);
        return points;
    }
}