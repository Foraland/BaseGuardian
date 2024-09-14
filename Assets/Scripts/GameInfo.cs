using UnityEngine;

public class GameInfo
{
    public float homeLife = 0;
    public float playerLife = 0;
    public float playerEnergy = 0;
    public readonly float playerMaxEnergy = 50;
    public readonly float playerMaxLife = 100;
    public readonly float homeMaxLife = 100;
    public readonly float costPerM = 1;
    public readonly float threshold = 19;
    public readonly float energySpeed = 5;
    public readonly float circleRad = Mathf.PI / 3;

    public void Init()
    {
        homeLife = homeMaxLife;
        playerLife = playerMaxLife;
        playerEnergy = playerMaxEnergy;
    }
    /// <summary>
    /// 尝试攻击，返回消耗百分比
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public float attemptToAtk(float length)
    {
        if (playerEnergy < threshold)
            return -1;
        float require = length * costPerM;
        if (playerEnergy >= require)
            return 1;
        else
        {
            float percent = playerEnergy / require;
            return percent;
        }
    }
    public float GetEnergyByLength(float length)
    {
        return length * costPerM;
    }
    public void applyToAtk(float length)
    {
        if (playerEnergy < threshold)
            return;
        float costEnergy = length * costPerM;
        playerEnergy -= costEnergy;
        EC.Send(EC.COST, costEnergy.ToString());
    }
}