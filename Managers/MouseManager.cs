using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 事件函数，获取一个Vector3
/// </summary>
/// //序列化，可视化特性
//[System.Serializable]
//public class EventVector3:UnityEvent<Vector3>
//{

//}

public class MouseManager : Singleton<MouseManager>
{
    public event Action<Vector3> OnMouseClicked;
    public event Action<GameObject> onEnemyClicked;
    //检测射线与场景中物体的碰撞 存储碰撞的详细信息
    private RaycastHit hitInfo;
    //鼠标类型
    public Texture2D point, doorway, attack, target, arrow;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private MouseManager()
    {

    } 

    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    /// <summary>
    /// 设置鼠标图片
    /// </summary>
    private void SetCursorTexture()
    {
        //射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hitInfo))
        {
            //切换鼠标贴图
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(arrow, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }
        }
    }

    /// <summary>
    /// 鼠标控制
    /// </summary>
    private void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            //检测点击是否为地面
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                OnMouseClicked?.Invoke(hitInfo.point);
            }
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
            {
                onEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
            {
                onEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Portal"))
            {
                OnMouseClicked?.Invoke(hitInfo.point);
            }
        }
    }

}
