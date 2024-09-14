
using UnityEngine;
using UnityEngine.UI;

public class GameUICtrl : Controller
{
    public Image bar;
    public Image previewCost;
    public Image cost;
    public GameInfo info => GameManager.Ins.gameInfo;
    public override void OnEnter()
    {
        base.OnEnter();
        EC.On(EC.PREVIEW_COST, OnPreviewCost);
        EC.On(EC.CANCEL_PREVIEW, OnCancelPreview);
        EC.On(EC.COST, OnCost);
        EC.On(EC.REFRESH, OnRefresh);
    }
    private void OnCost(string arg)
    {
        float energy = float.Parse(arg);
        float standardWidth = bar.GetComponent<RectTransform>().rect.width;
        float width = energy / info.playerMaxEnergy * standardWidth;
        float startX = standardWidth * (info.playerEnergy + energy) / info.playerMaxEnergy - width;
        RectTransform rt = cost.GetComponent<RectTransform>();
        Vector2 pos = rt.anchoredPosition;
        cost.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX, pos.y);
        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
        cost.GetComponent<Animator>().Play("CostEaseOut");
    }
    private void OnRefresh()
    {
        bar.fillAmount = info.playerEnergy / info.playerMaxEnergy;
    }

    public override void OnExit()
    {
        base.OnExit();
        EC.Off(EC.PREVIEW_COST, OnPreviewCost);
        EC.Off(EC.CANCEL_PREVIEW, OnCancelPreview);
        EC.Off(EC.COST, OnCost);
        EC.Off(EC.REFRESH, OnRefresh);
    }
    private void OnPreviewCost(string arg)
    {
        float energy = float.Parse(arg);
        previewCost.enabled = true;
        float standardWidth = bar.GetComponent<RectTransform>().rect.width;
        float width = energy / info.playerMaxEnergy * standardWidth;
        float startX = standardWidth * info.playerEnergy / info.playerMaxEnergy - width;
        RectTransform rt = previewCost.GetComponent<RectTransform>();
        Vector2 pos = rt.anchoredPosition;
        previewCost.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX, pos.y);
        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
        Debug.Log("PreviewCost:");

    }
    private void OnCancelPreview()
    {
        previewCost.enabled = false;
        Debug.Log("CancelPreview:");
    }

    public void Reset()
    {

    }
}

