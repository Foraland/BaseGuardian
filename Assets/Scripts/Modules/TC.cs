using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Events;
#endif
[AddComponentMenu("Transform/TC", 30)]
//TC
public class TC : MonoBehaviour
{
    public enum ActiveMode
    {
        Forbidden,      //变换过程中调用无效
        Interrupt,          //变换过程中调用将会打断当前变换重新计算初始位置
        Refresh,            //变换过程中调用将使当前目标立刻达成，进行下一次变换
        Queue,                  //将以队列的形式记录和前往目标
    }
    private enum ArgsID
    {
        Curve = 0,
        DelayTime,
        AutoFit,
        Duration,
        ActiveMode,
        NodeAct,
        IntervelTime,
        count
    }
    public enum FuncID
    {
        Move = 0,
        Rotate,
        Scale,
        count
    }
    public AnimationCurve[] funcCurve = new AnimationCurve[(int)FuncID.count]
    {AnimationCurve.Linear(0,0,1,1),AnimationCurve.Linear(0,0,1,1),AnimationCurve.Linear(0,0,1,1)};
    public float[] delayTime = new float[(int)FuncID.count] { 0, 0, 0 };
    public bool[] autoFit = new bool[(int)FuncID.count] { false, false, false };
    public float[] duration = new float[(int)FuncID.count] { 1f, 1f, 1f };
    public ActiveMode[] activeMode = new ActiveMode[(int)FuncID.count];
    public bool[] nodeAct = new bool[(int)FuncID.count] { true, true, true };
    public float[] intervalTime = new float[(int)FuncID.count] { 0, 0, 0 };
    bool[] flag = new bool[] { true, true, true };

    public Vector3 tarPos = new Vector3(0, 0, 0);
    public Vector3 tarEuler = new Vector3(0, 0, 0);
    public Vector3 tarScale = new Vector3(1, 1, 1);
    Vector3[] offsetTrans = new Vector3[(int)FuncID.count] { Vector3.zero, Vector3.zero, Vector3.zero };
    public bool[] TransAllow = new bool[(int)FuncID.count] { false, false, false };
    private Coroutine[] TransCor = new Coroutine[(int)FuncID.count] { null, null, null };
    private Queue<Vector3>[] TransQueue = new Queue<Vector3>[(int)FuncID.count]
    {new Queue<Vector3>(),new Queue<Vector3>(),new Queue<Vector3>()};
    //事件是否是结点事件，若true则在Queue模式下每次到达目标后都会触发Act，否则完成了所有目标才触发Act
    [SerializeField]
    public UnityEvent MoveAct = new UnityEvent();
    [SerializeField]
    public UnityEvent RotateAct = new UnityEvent();
    [SerializeField]
    public UnityEvent ScaleAct = new UnityEvent();
    [SerializeField]
    public UnityEvent MoveTick = new UnityEvent();
    [SerializeField]
    public UnityEvent RotateTick = new UnityEvent();
    [SerializeField]
    public UnityEvent ScaleTick = new UnityEvent();
    public void SetActiveMode(FuncID func, ActiveMode mode) { activeMode[(int)func] = mode; }
    public void StopCor(FuncID func)
    {
        int index = (int)func;
        if (TransCor[index] != null)
            StopCoroutine(TransCor[index]);
        TransCor[index] = null;
    }
    public void StopCor(int index)          //批量停止协程
    {
        // #if UNITY_EDITOR
        // Debug.Log("请在运行模式下测试动画！");
        // #else
        for (int i = 0; i < (int)FuncID.count && index > 0; i++, index /= 2)
        {
            if (index % 2 == 1 && TransCor[i] != null)
            {
                StopCoroutine(TransCor[i]);
                TransCor[i] = null;
            }
        }
        // #endif
    }
    public void StartCor(int index)         //批量启动协程
    {
        // #if UNITY_EDITOR
        // Debug.Log("请在运行模式下测试动画！");
        // #else
        bool flag = true;
        if (index < 0)
        {
            flag = false;
            index = -index;
        }
        for (int i = 0; i < (int)FuncID.count && index > 0; i++, index /= 2)
        {
            if (index % 2 == 1)
            {
                if (flag)
                    switch (i)
                    {
                        case 0:
                            BesselPosition(tarPos);
                            break;
                        case 1:
                            BesselRotation(tarEuler);
                            break;
                        case 2:
                            BesselScale(tarScale);
                            break;
                    }
                else
                    switch (i)
                    {
                        case 0:
                            BesselMove(offsetTrans[i]);
                            break;
                        case 1:
                            BesselRotate(offsetTrans[i]);
                            break;
                        case 2:
                            BesselScale2(offsetTrans[i]);
                            break;
                    }
            }
        }
        // #endif
    }
    public int DetectCor()                      //检测协程启动情况
    {
        int res = 0;
        for (int i = 0; i < (int)FuncID.count; i++)
            if (TransCor[i] != null)
                res += 1 << i;
        return res;
    }
    public IEnumerator MoveIE(Vector3 offset)
    {
        tarPos = transform.position + offset;
        yield return MoveIE();
    }
    public IEnumerator PositionIE(Vector3 target)
    {
        tarPos = target;
        yield return MoveIE();
    }
    public IEnumerator MoveIE()               //移动迭代程序
    {
        float t = 0;
        float init_x = transform.position.x;
        float init_y = transform.position.y;
        float init_z = transform.position.z;
        Vector3 tarPosition;
        if (activeMode[(int)FuncID.Move] == ActiveMode.Queue)
        {
            Vector3 tmpPos = TransQueue[(int)FuncID.Move].Dequeue();
            tarPosition = new Vector3(tmpPos.x, tmpPos.y, tmpPos.z);
        }
        else
            tarPosition = new Vector3(tarPos.x, tarPos.y, tarPos.z);
        float delta;
        if (autoFit[(int)FuncID.Move])
            delta = Vector3.Distance(transform.position, tarPosition) / duration[(int)FuncID.Move];
        else
            delta = duration[(int)FuncID.Move];
        if (delayTime[(int)FuncID.Move] > 0)
            yield return new WaitForSeconds(delayTime[(int)FuncID.Move]);
        while (t < delta)
        {
            transform.position = new Vector3(init_x + funcCurve[(int)FuncID.Move].Evaluate(t / delta) * (tarPosition.x - init_x),
            init_y + funcCurve[(int)FuncID.Move].Evaluate(t / delta) * (tarPosition.y - init_y),
            init_z + funcCurve[(int)FuncID.Move].Evaluate(t / delta) * (tarPosition.z - init_z));
            t += Time.deltaTime;
            MoveTick.Invoke();
            yield return 0;
        }
        transform.position = new Vector3(init_x + funcCurve[(int)FuncID.Move].Evaluate(1) * (tarPosition.x - init_x),
            init_y + funcCurve[(int)FuncID.Move].Evaluate(1) * (tarPosition.y - init_y),
            init_z + funcCurve[(int)FuncID.Move].Evaluate(1) * (tarPosition.z - init_z));
        if (activeMode[(int)FuncID.Move] == ActiveMode.Queue)
        {
            if (TransQueue[(int)FuncID.Move].Count > 0)
            {
                if (nodeAct[(int)FuncID.Move])
                    MoveAct.Invoke();
                if (intervalTime[(int)FuncID.Move] > 0)
                    yield return new WaitForSeconds(intervalTime[(int)FuncID.Move]);
                TransCor[(int)FuncID.Move] = StartCoroutine(MoveIE());
            }
            else
            {
                TransCor[(int)FuncID.Move] = null;
                MoveAct.Invoke();
            }
        }
        else
        {
            TransQueue[(int)FuncID.Move].Clear();
            TransCor[(int)FuncID.Move] = null;
            MoveAct.Invoke();
        }
    }
    public IEnumerator RotateIE(Vector3 angle)
    {
        tarEuler = transform.rotation.eulerAngles + angle;
        yield return RotateIE();
    }
    public IEnumerator RotationIE(Vector3 target)
    {
        tarEuler = target;
        yield return RotateIE();
    }
    public IEnumerator RotateIE()             //旋转迭代程序
    {
        float t = 0;
        float init_x = transform.rotation.eulerAngles.x;
        float init_y = transform.rotation.eulerAngles.y;
        float init_z = transform.rotation.eulerAngles.z;
        Vector3 tarAngle;
        if (activeMode[(int)FuncID.Rotate] == ActiveMode.Queue)
        {
            Vector3 tmpRot = TransQueue[(int)FuncID.Rotate].Dequeue();
            tarAngle = new Vector3(tmpRot.x, tmpRot.y, tmpRot.z);
        }
        else
            tarAngle = new Vector3(tarEuler.x, tarEuler.y, tarEuler.z);
        float delta;
        if (autoFit[(int)FuncID.Rotate])
            delta = Vector3.Distance(transform.position, tarAngle) / duration[(int)FuncID.Rotate];
        else
            delta = duration[(int)FuncID.Rotate];
        if (delayTime[(int)FuncID.Rotate] > 0)
            yield return new WaitForSeconds(delayTime[(int)FuncID.Rotate]);
        while (t < delta)
        {
            transform.rotation = Quaternion.Euler(init_x + funcCurve[(int)FuncID.Rotate].Evaluate(t / delta) * (tarAngle.x - init_x),
            init_y + funcCurve[(int)FuncID.Rotate].Evaluate(t / delta) * (tarAngle.y - init_y),
            init_z + funcCurve[(int)FuncID.Rotate].Evaluate(t / delta) * (tarAngle.z - init_z));
            t += Time.deltaTime;
            RotateTick.Invoke();
            yield return 0;
        }
        transform.rotation = Quaternion.Euler(init_x + funcCurve[(int)FuncID.Rotate].Evaluate((int)FuncID.Rotate) * (tarAngle.x - init_x),
            (init_y + funcCurve[(int)FuncID.Rotate].Evaluate((int)FuncID.Rotate) * (tarAngle.y - init_y)),
            init_z + funcCurve[(int)FuncID.Rotate].Evaluate((int)FuncID.Rotate) * (tarAngle.z - init_z));
        if (activeMode[(int)FuncID.Rotate] == ActiveMode.Queue)
        {
            if (TransQueue[(int)FuncID.Rotate].Count > 0)
            {
                if (nodeAct[(int)FuncID.Rotate])
                    RotateAct.Invoke();
                if (intervalTime[(int)FuncID.Rotate] > 0)
                    yield return new WaitForSeconds(intervalTime[(int)FuncID.Rotate]);
                TransCor[(int)FuncID.Rotate] = StartCoroutine(RotateIE());
            }
            else
            {
                TransCor[(int)FuncID.Rotate] = null;
                RotateAct.Invoke();
            }
        }
        else
        {
            TransQueue[(int)FuncID.Rotate].Clear();
            TransCor[(int)FuncID.Rotate] = null;
            RotateAct.Invoke();
        }
    }
    public IEnumerator ScaleIE(Vector3 target)
    {
        tarScale = target;
        yield return ScaleIE();
    }
    public IEnumerator ScaleIE()              //缩放迭代程序
    {
        float t = 0;
        float init_x = transform.localScale.x;
        float init_y = transform.localScale.y;
        float init_z = transform.localScale.z;
        Vector3 tarScale_inter;
        if (activeMode[(int)FuncID.Scale] == ActiveMode.Queue)
        {
            Vector3 tmpPos = TransQueue[(int)FuncID.Scale].Dequeue();
            tarScale_inter = new Vector3(tmpPos.x, tmpPos.y, tmpPos.z);
        }
        else
            tarScale_inter = new Vector3(tarScale.x, tarScale.y, tarScale.z);
        float delta;
        if (autoFit[(int)FuncID.Scale])
            delta = Vector3.Distance(transform.localScale, tarScale_inter) / duration[(int)FuncID.Scale];
        else
            delta = duration[(int)FuncID.Scale];
        if (delayTime[(int)FuncID.Scale] > 0)
            yield return new WaitForSeconds(delayTime[(int)FuncID.Scale]);
        while (t < delta)
        {
            transform.localScale = new Vector3(init_x + funcCurve[(int)FuncID.Scale].Evaluate(t / delta) * (tarScale_inter.x - init_x),
            init_y + funcCurve[(int)FuncID.Scale].Evaluate(t / delta) * (tarScale_inter.y - init_y),
            init_z + funcCurve[(int)FuncID.Scale].Evaluate(t / delta) * (tarScale_inter.z - init_z));
            t += Time.deltaTime;
            ScaleTick.Invoke();
            yield return 0;
        }
        transform.localScale = new Vector3(init_x + funcCurve[(int)FuncID.Scale].Evaluate(1) * (tarScale_inter.x - init_x),
            init_y + funcCurve[(int)FuncID.Scale].Evaluate(1) * (tarScale_inter.y - init_y),
            init_z + funcCurve[(int)FuncID.Scale].Evaluate(1) * (tarScale_inter.z - init_z));
        if (activeMode[(int)FuncID.Scale] == ActiveMode.Queue)
        {
            if (TransQueue[(int)FuncID.Scale].Count > 0)
            {
                if (nodeAct[(int)FuncID.Scale])
                    ScaleAct.Invoke();
                if (intervalTime[(int)FuncID.Scale] > 0)
                    yield return new WaitForSeconds(intervalTime[(int)FuncID.Scale]);
                TransCor[(int)FuncID.Scale] = StartCoroutine(ScaleIE());
            }
            else
            {
                TransCor[(int)FuncID.Scale] = null;
                ScaleAct.Invoke();
            }
        }
        else
        {
            TransQueue[(int)FuncID.Scale].Clear();
            TransCor[(int)FuncID.Scale] = null;
            ScaleAct.Invoke();
        }
    }
    public void BesselMove(Vector3 vector)  //向指定方向移动
    {
        BesselPosition(transform.position + vector);
    }
    public void BesselPosition()            //向缓存目标位置移动
    {
        BesselPosition(tarPos);
    }
    public void BesselPosition(Vector3 targetPosition)  //向指定地点移动
    {
        Debug.Log("BesselPosition " + targetPosition);
        if (!TransAllow[(int)FuncID.Move])
            return;
        if (TransCor[(int)FuncID.Move] != null)
        {
            switch (activeMode[(int)FuncID.Move])
            {
                case ActiveMode.Forbidden:
                    return;
                case ActiveMode.Interrupt:
                    StopCoroutine(TransCor[(int)FuncID.Move]);
                    tarPos = targetPosition;
                    TransCor[(int)FuncID.Move] = StartCoroutine(MoveIE());
                    break;
                case ActiveMode.Queue:
                    TransQueue[(int)FuncID.Move].Enqueue(targetPosition);
                    break;
                case ActiveMode.Refresh:
                    StopCoroutine(TransCor[(int)FuncID.Move]);
                    transform.position = tarPos;
                    tarPos = targetPosition;
                    TransCor[(int)FuncID.Move] = StartCoroutine(MoveIE());
                    break;
            }
        }
        else
        {
            if (activeMode[(int)FuncID.Move] == ActiveMode.Queue)
                TransQueue[(int)FuncID.Move].Enqueue(targetPosition);
            else
                tarPos = targetPosition;
            TransCor[(int)FuncID.Move] = StartCoroutine(MoveIE());
        }

    }
    public void BesselRotate(Vector3 angle) //旋转指定角度
    {
        BesselRotation(transform.rotation.eulerAngles + angle);
    }
    public void BesselRotation()            //旋转至缓存的角度
    {
        BesselRotation(tarEuler);
    }
    public void BesselRotation(Vector3 targetEuler) //旋转至指定角度
    {
        if (!TransAllow[(int)FuncID.Rotate])
            return;
        if (TransCor[(int)FuncID.Rotate] != null)
        {
            switch (activeMode[(int)FuncID.Rotate])
            {
                case ActiveMode.Forbidden:
                    return;
                case ActiveMode.Interrupt:
                    StopCoroutine(TransCor[(int)FuncID.Rotate]);
                    tarEuler = targetEuler;
                    TransCor[(int)FuncID.Rotate] = StartCoroutine(RotateIE());
                    break;
                case ActiveMode.Queue:
                    TransQueue[(int)FuncID.Rotate].Enqueue(targetEuler);
                    break;
                case ActiveMode.Refresh:
                    StopCoroutine(TransCor[(int)FuncID.Rotate]);
                    transform.rotation = Quaternion.Euler(tarEuler);
                    tarEuler = targetEuler;
                    TransCor[(int)FuncID.Rotate] = StartCoroutine(RotateIE());
                    break;
            }
        }
        else
        {
            if (activeMode[(int)FuncID.Rotate] == ActiveMode.Queue)
                TransQueue[(int)FuncID.Rotate].Enqueue(targetEuler);
            else
                tarEuler = targetEuler;
            TransCor[(int)FuncID.Rotate] = StartCoroutine(RotateIE());
        }
    }
    public void BesselScale2(Vector3 scale) //缩放指定比例
    {
        BesselScale(transform.localScale + scale);
    }
    public void BesselScale()               //缩放到缓存的比例
    {
        BesselScale(tarScale);
    }
    public void BesselScale(Vector3 targetScale)//缩放到目标比例
    {
        if (!TransAllow[(int)FuncID.Scale])
            return;
        if (TransCor[(int)FuncID.Scale] != null)
        {
            switch (activeMode[(int)FuncID.Scale])
            {
                case ActiveMode.Forbidden:
                    return;
                case ActiveMode.Interrupt:
                    StopCoroutine(TransCor[(int)FuncID.Scale]);
                    tarScale = targetScale;
                    TransCor[(int)FuncID.Scale] = StartCoroutine(ScaleIE());
                    break;
                case ActiveMode.Queue:
                    TransQueue[(int)FuncID.Scale].Enqueue(targetScale);
                    break;
                case ActiveMode.Refresh:
                    StopCoroutine(TransCor[(int)FuncID.Scale]);
                    transform.localScale = tarScale;
                    tarScale = targetScale;
                    TransCor[(int)FuncID.Scale] = StartCoroutine(ScaleIE());
                    break;
            }
        }
        else
        {
            if (activeMode[(int)FuncID.Scale] == ActiveMode.Queue)
                TransQueue[(int)FuncID.Scale].Enqueue(targetScale);
            else
                tarScale = targetScale;
            TransCor[(int)FuncID.Scale] = StartCoroutine(ScaleIE());
        }
    }

    //==================以下为编辑器重载扩展=========================================//
#if UNITY_EDITOR
    [CustomEditor(typeof(TC))]
    public class MoveLerp_Inspector : Editor
    {
        private bool[] fold = new bool[(int)FuncID.count] { true, true, true };   //下标使用调用ID
        private int[] unif_index = new int[(int)ArgsID.count] { -1, -1, -1, -1, -1, -1, -1 };            //下标使用参数ID
        private bool[] uni = new bool[(int)ArgsID.count] { false, false, false, false, false, false, false };       //下标使用参数ID
        bool[] buttonFlag = new bool[(int)FuncID.count] { true, true, true };
        private SerializedProperty serMoveAct;
        private SerializedProperty serRotateAct;
        private SerializedProperty serScaleAct;
        private SerializedProperty serMoveTick;
        private SerializedProperty serRotateTick;
        private SerializedProperty serScaleTick;
        TC script;
        public void OnEnable()
        {
            script = target as TC;
            serMoveAct = serializedObject.FindProperty("MoveAct");
            serRotateAct = serializedObject.FindProperty("RotateAct");
            serScaleAct = serializedObject.FindProperty("ScaleAct");
            serMoveTick = serializedObject.FindProperty("MoveTick");
            serRotateTick = serializedObject.FindProperty("RotateTick");
            serScaleTick = serializedObject.FindProperty("ScaleTick");
        }
        public override void OnInspectorGUI()
        {
            #region 全局控制面板----------------------------------------------------------------------------------//

            GUILayout.BeginHorizontal();
            GUILayout.Label("全局控制");
            GUILayout.Space(102);
            if (GUILayout.Button("停止所有动画", GUILayout.MinWidth(100f)))
            {
                script.StopCor(7);
            }
            GUILayout.Space(20);
            if (GUILayout.Button("启动所有动画", GUILayout.Width(100f)))
            {
                script.StartCor(7);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            for (int p = 0; p < (int)FuncID.count; p++)
            {
                if (script.TransAllow[p])
                {
                    for (int i = 0; i < (int)ArgsID.count; i++)
                    {
                        if (unif_index[i] >= 0 && unif_index[i] < (int)FuncID.count)
                        {
                            GUILayout.BeginHorizontal();
                            if (i == (int)ArgsID.Curve)
                            {
                                AnimationCurve tmp = script.funcCurve[unif_index[i]];
                                tmp = EditorGUILayout.CurveField("动画曲线", tmp, GUILayout.Width(400f));
                                for (int j = 0; j < (int)FuncID.count; j++)
                                    script.funcCurve[j] = tmp;
                            }
                            else if (i == (int)ArgsID.DelayTime)
                            {
                                float tmp = script.delayTime[unif_index[i]];
                                tmp = EditorGUILayout.FloatField("起始延迟", tmp, GUILayout.Width(400f));
                                for (int j = 0; j < (int)FuncID.count; j++)
                                    script.delayTime[j] = tmp;
                            }
                            else if (i == (int)ArgsID.AutoFit)
                            {
                                bool tmp = script.autoFit[unif_index[i]];
                                tmp = EditorGUILayout.Toggle("速度自适应", tmp, GUILayout.Width(400f));
                                for (int j = 0; j < (int)FuncID.count; j++)
                                    script.autoFit[j] = tmp;
                            }
                            else if (i == (int)ArgsID.Duration)
                            {
                                float tmp = script.duration[unif_index[i]];
                                tmp = EditorGUILayout.FloatField("持续时间/速度", tmp, GUILayout.Width(400f));
                                for (int j = 0; j < (int)FuncID.count; j++)
                                    script.duration[j] = tmp;
                            }
                            else if (i == (int)ArgsID.ActiveMode)
                            {
                                ActiveMode tmp = script.activeMode[unif_index[i]];
                                tmp = (ActiveMode)EditorGUILayout.EnumPopup("打断模式", tmp, GUILayout.Width(400f));
                                for (int j = 0; j < (int)FuncID.count; j++)
                                    script.activeMode[j] = tmp;
                            }
                            else if (i == (int)ArgsID.NodeAct)
                            {
                                bool tmp = script.nodeAct[unif_index[i]];
                                tmp = EditorGUILayout.Toggle("Is node Act", tmp, GUILayout.Width(400f));
                                for (int j = 0; j < (int)FuncID.count; j++)
                                    script.nodeAct[j] = tmp;
                            }
                            else if (i == (int)ArgsID.IntervelTime)
                            {
                                float tmp = script.intervalTime[unif_index[i]];
                                tmp = EditorGUILayout.FloatField("间隔时间", tmp, GUILayout.Width(400f));
                                for (int j = 0; j < (int)FuncID.count; j++)
                                    script.intervalTime[j] = tmp;
                            }
                            uni[i] = EditorGUILayout.Toggle(uni[i]);
                            GUILayout.EndHorizontal();
                            if (!uni[i])
                            {
                                unif_index[i] = -1;
                                if (i == (int)ArgsID.Curve)
                                    for (int j = 0; j < (int)FuncID.count; j++)
                                        script.funcCurve[j] = new AnimationCurve();
                            }
                        }
                    }
                    break;
                }
            }
            #endregion
            #region 信息提示框------------------------------------------------------------------------------------//
            bool flag = true;
            for (int i = 0; i < (int)FuncID.count; i++)
            {
                if (script.TransAllow[i])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
            }
            #endregion
            #region 功能控制面板----------------------------------------------------------------------------------//
            string[] funcName = new string[] { "坐标", "欧拉角", "比例" };
            string[] funcName2 = new string[] { "位移", "旋转", "缩放" };

            for (int f = 0; f < (int)FuncID.count; f++)
            {
                GUILayout.Space(10);
                if (!script.TransAllow[f])
                {
                    script.TransAllow[f] = EditorGUILayout.Toggle("开启" + funcName[f] + "曲线控制", script.TransAllow[f]);
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    fold[f] = EditorGUILayout.Foldout(fold[f], funcName[f]);
                    GUILayout.Space(135);
                    if (buttonFlag[f])
                    {
                        if (GUILayout.Button(funcName2[f] + "到目标", GUILayout.MaxWidth(200f)))
                            script.StartCor(1 << f);
                    }
                    else
                    {
                        if (GUILayout.Button(funcName2[f] + "差值", GUILayout.MaxWidth(200f)))
                            script.StartCor(-(1 << f));
                    }
                    buttonFlag[f] = EditorGUILayout.Toggle(buttonFlag[f]);
                    GUILayout.EndHorizontal();
                    if (fold[f])
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        script.TransAllow[f] = EditorGUILayout.Toggle("关闭组件功能", script.TransAllow[f]);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        if (buttonFlag[f])
                        {
                            if (f == (int)FuncID.Move)
                                script.tarPos = EditorGUILayout.Vector3Field("目标" + funcName[f], script.tarPos);
                            else if (f == (int)FuncID.Rotate)
                                script.tarEuler = EditorGUILayout.Vector3Field("目标" + funcName[f], script.tarEuler);
                            else if (f == (int)FuncID.Scale)
                                script.tarScale = EditorGUILayout.Vector3Field("目标" + funcName[f], script.tarScale);
                        }
                        else
                        {
                            script.offsetTrans[f] = EditorGUILayout.Vector3Field("偏移" + funcName[f], script.offsetTrans[f]);
                        }
                        GUILayout.EndHorizontal();
                        //参数判断
                        for (int i = 0; i < (int)ArgsID.count; i++)
                        {
                            if (unif_index[i] == -1)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(15);
                                if (i == (int)ArgsID.Curve)
                                    script.funcCurve[f] = EditorGUILayout.CurveField(funcName2[f] + "曲线", script.funcCurve[f], GUILayout.Width(400f));
                                else if (i == (int)ArgsID.DelayTime)
                                    script.delayTime[f] = EditorGUILayout.FloatField("起始延迟", script.delayTime[f], GUILayout.Width(400f));
                                else if (i == (int)ArgsID.AutoFit)
                                    script.autoFit[f] = EditorGUILayout.Toggle("速度自适应", script.autoFit[f], GUILayout.Width(400f));
                                else if (i == (int)ArgsID.Duration)
                                    script.duration[f] = EditorGUILayout.FloatField(script.autoFit[f] ? "速度" : "持续时间", script.duration[f], GUILayout.Width(400f));
                                else if (i == (int)ArgsID.ActiveMode)
                                    script.activeMode[f] = (ActiveMode)EditorGUILayout.EnumPopup(funcName2[f] + "动画打断模式", script.activeMode[f], GUILayout.Width(400f));
                                else if (i == (int)ArgsID.NodeAct && script.activeMode[f] == ActiveMode.Queue)
                                    script.nodeAct[f] = EditorGUILayout.Toggle("是否是行为节点", script.nodeAct[f], GUILayout.Width(400f));
                                else if (i == (int)ArgsID.IntervelTime && script.activeMode[f] == ActiveMode.Queue)
                                    script.intervalTime[f] = EditorGUILayout.FloatField("间隔时间", script.intervalTime[f], GUILayout.Width(400f));
                                if ((i == (int)ArgsID.NodeAct || i == (int)ArgsID.IntervelTime) && script.activeMode[f] != ActiveMode.Queue)
                                {
                                    GUILayout.EndHorizontal();
                                    continue;
                                }
                                if (uni[i] = EditorGUILayout.Toggle(uni[i]))
                                    unif_index[i] = f;
                                GUILayout.EndHorizontal();
                            }
                        }
                        if (f == (int)FuncID.Move) EditorGUILayout.PropertyField(serMoveAct, new GUIContent("位移结束事件"));
                        if (f == (int)FuncID.Rotate) EditorGUILayout.PropertyField(serRotateAct, new GUIContent("旋转结束事件"));
                        if (f == (int)FuncID.Scale) EditorGUILayout.PropertyField(serScaleAct, new GUIContent("缩放结束事件"));
                        if (f == (int)FuncID.Move) EditorGUILayout.PropertyField(serMoveTick, new GUIContent("位移Tick事件"));
                        if (f == (int)FuncID.Rotate) EditorGUILayout.PropertyField(serRotateTick, new GUIContent("旋转Tick事件"));
                        if (f == (int)FuncID.Scale) EditorGUILayout.PropertyField(serScaleTick, new GUIContent("缩放Tick事件"));
                    }
                    serializedObject.ApplyModifiedProperties();
                }
            }
            #endregion
            #region 信息提示框------------------------------------------------------------------------------------//
            if (flag)
            {
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                EditorGUILayout.HelpBox("所有按钮请在运行时测试", MessageType.Info);
                EditorGUILayout.HelpBox("变换曲线只有在横坐标（时间进度）为0~1的范围有效", MessageType.Info);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            #endregion
        }
    }
#endif

}

