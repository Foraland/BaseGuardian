using UnityEngine;

public class Launch : MonoBehaviour
{
    private void Awake()
    {
        OP.Ins.root = transform;
    }
    private void Start()
    {
        GameManager.Ins.OnEnterGame();
    }
    private void Update()
    {
        TM.OnUpdate();
        CT.OnUpdate();
    }
}