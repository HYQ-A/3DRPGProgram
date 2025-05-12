using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;
    private CinemachineFreeLook followCamera;

    //观察者列表
    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    /// <summary>
    /// 注册玩家，找到玩家并将摄像机锁定跟随玩家的LookAtPoint
    /// </summary>
    /// <param name="player"></param>
    public void RigisterPlayer(CharacterStats player)
    {
        playerStats = player;

        followCamera = FindObjectOfType<CinemachineFreeLook>();
        if (followCamera != null)
        {
            followCamera.Follow = playerStats.transform.GetChild(2);
            followCamera.LookAt = playerStats.transform.GetChild(2);
        }
    }

    /// <summary>
    /// 添加观察者
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    /// <summary>
    /// 移除观察者
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    /// <summary>
    /// 广播观察者，发送广播
    /// </summary>
    public void NotifyObservers()
    {
        foreach (var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }

    public Transform GetEntrance()
    {
        foreach(var item in FindObjectsOfType<TransitionDestinnation>())
        {
            if (item.destinationTag == TransitionDestinnation.DestinationTag.A)
                return item.transform;
        }
        return null;
    }
}
