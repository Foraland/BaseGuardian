
using System.Collections.Generic;
using UnityEngine;

public class LineOp : OpBase
{
    public LineOp(GameObject character) : base(character) { }
    public override void OnEnterOp()
    {
        base.OnEnterOp();
        previewPoses.Clear();
        previewPoses.Add(character.transform.position);
    }
    public override void OnSample(Vector2 pos)
    {
        base.OnSample(pos);
        float dis = Vector2.Distance(character.transform.position, pos);
        float t = GameManager.Ins.gameInfo.attemptToAtk(dis);
        float energy = GameManager.Ins.gameInfo.GetEnergyByLength(dis * t);
        if (t <= 0)
        {
            pos = character.transform.position;
            EC.Send(EC.ERROR_COST);
        }
        else if (t < 1)
        {
            pos = character.transform.position + (new Vector3(pos.x, pos.y, 0) - character.transform.position).normalized * t * dis;
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
        if (previewPoses.Count < 2)
        {
            previewPoses.Add(pos);
        }
        else
        {
            previewPoses[0] = character.transform.position;
            previewPoses[1] = pos;
        }
    }
    public override void OnExit()
    {
        base.OnExit();
        LineDrawer.Clear(previewLine);
        LineDrawer.Clear(applyLine);
        previewLine = null;
        applyLine = null;
        tc.StopCor(TC.FuncID.Move);
    }
    public override void OnExitOp()
    {
        base.OnExitOp();
        applyPoses = new List<Vector2>(previewPoses);
        UpdateApplyLine();
        LineDrawer.Clear(previewLine);
        previewLine = null;
        EC.Send(EC.CANCEL_PREVIEW);
    }
    public override void OnActive()
    {
        base.OnActive();
        TC tc = character.GetComponent<TC>();
        tc.SetActiveMode(TC.FuncID.Move, TC.ActiveMode.Interrupt);
        tc.BesselPosition(applyPoses[1]);
        float energy = GameManager.Ins.gameInfo.GetEnergyByLength(Vector2.Distance(applyPoses[0], applyPoses[1]));
        GameManager.Ins.gameInfo.playerEnergy -= energy;
        EC.Send(EC.COST, energy.ToString());
        EC.Send(EC.REFRESH);
    }
    public override void OnUpdateActive()
    {
        base.OnUpdateActive();
        applyPoses[0] = character.transform.position;
        previewPoses[0] = character.transform.position;
        UpdateApplyLine();
    }
    public override void UpdateApplyLine()
    {
        base.UpdateApplyLine();
        applyLine = LineDrawer.DrawLine(applyPoses[0], applyPoses[1], applyLine);
    }
    public override void UpdatePreviewLine()
    {
        base.UpdatePreviewLine();
        previewLine = LineDrawer.DrawLine(previewPoses[0], previewPoses[1], previewLine);
    }
}