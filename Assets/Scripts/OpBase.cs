using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpBase
{
    protected TC tc;
    protected List<Vector2> previewPoses = new List<Vector2>();
    protected List<Vector2> applyPoses = new List<Vector2>();
    public LineRenderer previewLine = null;
    public LineRenderer applyLine = null;
    protected GameObject character;
    public bool isMoving = false;
    protected OpBase(GameObject character)
    {
        this.character = character;
        tc = character.GetComponent<TC>();
    }
    public virtual void OnEnter()
    {

    }
    public virtual void OnExit()
    {

    }
    public virtual void OnEnterOp()
    {

    }
    public virtual void OnExitOp()
    {

    }
    public virtual void OnSample(Vector2 pos)
    {

    }
    public virtual void OnActive()
    {

    }
    public virtual void OnUpdateActive()
    {

    }
    public virtual void UpdateApplyLine()
    {
    }
    public virtual void UpdatePreviewLine()
    {

    }
}