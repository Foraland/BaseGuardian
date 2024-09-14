using System.Collections.Generic;
using UnityEngine;

public class CircleOp : OpBase
{
    public GameInfo info => GameManager.Ins.gameInfo;
    public CircleOp(GameObject character) : base(character) { }
    public override void OnEnterOp()
    {
        base.OnEnterOp();
        previewPoses.Clear();
        previewPoses.Add(character.transform.position);

    }
    public override void OnEnter()
    {
        base.OnEnter();
        tc.MoveAct.AddListener(OnNextMove);
    }
    public override void OnExit()
    {
        base.OnExit();
        tc.MoveAct.RemoveListener(OnNextMove);
        LineDrawer.Clear(previewLine);
        LineDrawer.Clear(applyLine);
        previewLine = null;
        applyLine = null;
        tc.StopCor(TC.FuncID.Move);
    }
    public override void OnSample(Vector2 pos)
    {
        base.OnSample(pos);
        float length = Vector2.Distance(character.transform.position, pos) * info.circleRad;
        float t = GameManager.Ins.gameInfo.attemptToAtk(length);
        float dis = Vector2.Distance(character.transform.position, pos) * t;
        float energy = GameManager.Ins.gameInfo.GetEnergyByLength(dis * info.circleRad);
        if (t < 0)
        {
            pos = character.transform.position;
            EC.Send(EC.ERROR_COST);
        }
        else if (t < 1)
        {
            pos = character.transform.position + (new Vector3(pos.x, pos.y, 0) - character.transform.position).normalized * t * length;
            if (previewLine)
            {
                previewLine.startColor = previewLine.endColor = Color.yellow;
            }
            EC.Send(EC.PREVIEW_COST, energy.ToString());
        }
        else
        {
            if (previewLine)
            {
                previewLine.endColor = Color.white;
            }
            EC.Send(EC.PREVIEW_COST, energy.ToString());
        }
        previewPoses = LineDrawer.GetCircle(pos, character.transform.position, info.circleRad, 0.1f, previewLine);
        UpdatePreviewLine();
    }
    public override void OnExitOp()
    {
        base.OnExitOp();
        applyPoses = new List<Vector2>(previewPoses);
        UpdateApplyLine();
        previewPoses.Clear();
        LineDrawer.Clear(previewLine);
        previewLine = null;
        EC.Send(EC.CANCEL_PREVIEW);
    }
    private Queue<Vector2> posQueue = new Queue<Vector2>();
    public override void OnActive()
    {
        base.OnActive();
        tc.SetActiveMode(TC.FuncID.Move, TC.ActiveMode.Queue);
        posQueue = new Queue<Vector2>(applyPoses);

        OnNextMove();
    }
    public void OnNextMove()
    {
        if (posQueue.Count > 0)
            tc.BesselPosition(posQueue.Dequeue());
    }
    public override void OnUpdateActive()
    {
        base.OnUpdateActive();
    }
    public override void UpdatePreviewLine()
    {
        base.UpdatePreviewLine();
        previewLine = LineDrawer.DrawSegmentLines(previewPoses, previewLine);
    }
    public override void UpdateApplyLine()
    {
        base.UpdateApplyLine();
        applyLine = LineDrawer.DrawSegmentLines(applyPoses, applyLine);
    }
}