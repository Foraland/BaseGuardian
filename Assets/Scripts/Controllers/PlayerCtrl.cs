using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PlayerCtrl : Controller
{
    public override EGameState gameState => EGameState.Game;
    public LineDrawer lineDrawer = null;
    OpBase op = null;
    public Sprite sampleSprite;
    public float speed = 1f;
    public GameObject character = null;
    bool isLine = true;
    bool isInOp = false;
    public override void OnEnter()
    {
        base.OnEnter();
        LineDrawer.bindRoot(transform);
        op = new LineOp(character);
        op.OnEnter();
        character.GetComponent<TC>().MoveTick.AddListener(op.OnUpdateActive);
        gameObject.SetActive(true);
    }
    public override void OnExit()
    {
        base.OnExit();
        gameObject.SetActive(false);
    }
    public override void OnUpdate()
    {
        Vector2 v = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && !isInOp)
        {
            op.OnEnterOp();
            character.GetComponent<TC>().MoveTick.AddListener(op.OnUpdateActive);
            EnterDropTime(1f);
            isInOp = true;
        }
        if (Input.GetMouseButton(0) && isInOp)
        {
            op.OnSample(v);
            op.UpdatePreviewLine();
        }
        if (Input.GetMouseButtonUp(0) && isInOp)
        {
            op.OnExitOp();
            op.OnActive();
            ExitDropTime();
            Time.timeScale = 1;
            isInOp = false;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {

            isLine = !isLine;
            character.GetComponent<TC>().MoveTick.RemoveListener(op.OnUpdateActive);
            op.OnExit();
            if (isLine)
                op = new LineOp(character);
            else
                op = new CircleOp(character);
            op.OnEnter();
            character.GetComponent<TC>().MoveTick.AddListener(op.OnUpdateActive);
            isInOp = false;
        }
    }

    Coroutine dropTimeCor = null;
    void EnterDropTime(float time)
    {
        dropTimeCor = StartCoroutine(DropTimescaleIE(time));
    }
    void ExitDropTime()
    {
        if (dropTimeCor != null)
            StopCoroutine(dropTimeCor);
    }
    IEnumerator DropTimescaleIE(float time)
    {
        float timer = 0;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            Time.timeScale = 1 - TweenFuncs.QuadOut(timer / time);
            yield return 0;
        }
        Time.timeScale = 0;

    }
}
