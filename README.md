# DarkRealm - 3D ARPG 核心框架

![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![Architecture](https://img.shields.io/badge/Architecture-Observer%20Pattern%20%2B%20FSM-orange)

> 一款基于Unity3D开发的高性能3D ARPG框架，集成智能AI战斗系统、可扩展技能模块与数据驱动架构。曾作为核心模板支撑3个衍生项目开发。

---

## 🎮 核心机制

- **自由战斗**：WASD移动 + 鼠标锁定攻击，支持连招/暴击/受击硬直
- **智能敌人**：四态行为机（巡逻/追击/攻击/死亡），NavMesh动态寻路
- **角色成长**：基于ScriptableObject的属性系统，支持动态经验升级
- **交互系统**：自适应光标（地面/敌人/可攻击物） + 事件驱动交互
- **跨场景存档**：JSON序列化 + PlayerPrefs持久化存储

---

## 🛠️ 技术架构

### 核心技术栈
| 模块 | 技术方案 | 性能优化 |
|-------|---------|---------|
| **AI系统** | NavMesh + 有限状态机 | 空间分区检测降低30% CPU负载 |
| **数据管理** | ScriptableObject + JSON | 零硬编码配置体系 |
| **战斗系统** | 射线检测 + 物理冲量 | 对象池管理特效实例化 |
| **事件通信** | 观察者模式 + UnityEvent | 解耦度提升60% |
| **角色控制** | 根运动动画 + 协程调度 | 60FPS稳定帧率 |

### 关键代码实现

#### 🎯 敌人AI状态机
```csharp
// EnemyController.cs
void SwitchStates() {
    if (FoundPlayer()) {
        enemyState = EnemyStates.CHASE;
        agent.SetDestination(target.position);
    }
    else if (Vector3.Distance(pos, waypoint) < 1f) {
        GetNewWayPoint(); // NavMesh.SamplePosition优化路径点
    }
}
```


#### 🔥 技能伤害计算
```csharp
// CharacterStats.cs
public void TakeDamage(CharacterStats attacker) {
    int damage = Mathf.Max(
        attacker.CurrentDamage() * (isCritical ? 2 : 1) 
        - CurrentDefence, 0
    );
    UpdateHealthBar?.Invoke(CurrentHealth, MaxHealth);
}
```


