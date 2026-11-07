# SPEX Softlock Fix

修复《雨中冒险 2》合金意念 DLC 里的一个软锁：在**计算交易所**供奉「暴露小脑」开启解密传送门后，如果误伤 SPEX 被赶出交易所，这一局就再也进不去神经圣所。

## 软锁是怎么发生的

1. 供奉台 `SolusVendorShrineController.HandleGivenItem` 收到 `MasterCore`（暴露小脑）时，会 `RemoveItemPermanent` 永久删除它、把台子切到 `Disabled`，并让状态机进入 `SolusWebPortalSequence` 打开解密传送门。
2. SPEX 本体（`EntityStates.SolusVendor`）受到伤害超过阈值会进入 `StealItem`：随机偷走道具，并在 `OnExit` 里激活场景中的 `KickOutOfShop`，把所有人赶出交易所，同时永久禁用自己。
3. 结果是：小脑被消耗、台子关闭、SPEX 哑火。重新进入交易所时既没有小脑可交，也没有门可走。

## 修复内容

| 配置项 | 默认 | 作用 |
| --- | --- | --- |
| `Server.PreventSpexAnger` | `true` | 拦截 `SetStateOnHurt.SetPain()`（仅当对象带 `SolusVendorController`）与 `SolusVendorMainState.CanExecuteSkill()`，SPEX 不会进入偷取状态，也就不会触发赶人。 |
| `Server.RefundExposedCerebellum` | `true` | 记录真正交出暴露小脑的玩家；一旦 `StealItem.OnExit` 发生（被赶出），若该玩家背包里既没有暴露小脑也没有加密小脑，就补发一个。 |
| `Server.AnnounceRefund` | `true` | 补发时向全队广播一条提示。 |

补发只给**最初提交供奉的那名玩家**，不是房主、也不是现场随机玩家；多人局里别人乱开枪导致被赶，物品仍回到提交者身上。补发前会检查背包，已有小脑时不会重复发放。

## 与同类 mod 的关系

社区里已经有一个做同一件事的 mod：[NoSpexKickout](https://thunderstore.io/c/riskofrain2/p/Jeffdev/NoSpexKickout)（作者 Jeffdev，2025-11-19 发布）。它同样能阻止 SPEX 把人赶出计算交易所，但实现和覆盖范围不同：

- 它把 `StealItem.FixedUpdate` 整个替换成空函数。`StealItem` 状态仍然会被进入，导致 SPEX **卡在这个状态里出不来**：主状态 `SolusVendorMainState` 停止运行，`OnEnter` 里创建的 `ItemStealController` 永远悬空。
- 它只做“阻断”，不处理已经发生的情况——如果小脑已经被消耗、人已经被赶出去，它救不回来。
- 该项目**未开源**：包内没有 LICENSE，也没有源码仓库链接，只能通过反编译查看实现。

本 mod 的差异：拦截发生在进入 `StealItem` 之前，SPEX 状态机保持正常；额外的补发逻辑可以修复已经中招的局；仅依赖 BepInExPack；源码公开。

> 两个 mod 都能阻止赶人，**不要同时安装**，没有意义。

## 安装（r2modman）

1. 打开 r2modman，选择 **Risk of Rain 2** 配置。
2. **Online** 页搜索 `SPEX Softlock Fix`（Thunderstore 上传后可用）；或 **Settings → Import local mod** 选择下载到的 zip。
3. 点击 **Start modded** 启动。

多人游戏中只需**房主**安装即可生效，逻辑全部是服务端判定。

## 兼容性

- 只挂钩 SPEX 专用的类/方法，不影响其他敌人的硬直、冰冻、嘲讽。
- `SetPain` 是通用方法，但 Prefix 第一行就判断对象是否带 `SolusVendorController`，其他单位直接走原逻辑。
- DLC 更新若改动相关类名，补丁会失效而不是崩溃；日志里能看到四条 Harmony 挂载记录。
- 配置项说明为中英双语；补发提示会跟随游戏语言（中文界面显示中文，其他语言显示英文）。

## 已知限制

- 补发按**本局**记录，只有在同一局里亲自供奉过暴露小脑才会触发。
- 玩家断线重连会换 `CharacterMaster`，记录随之失效（这种情况不补发）。
- 开启 `PreventSpexAnger` 后，SPEX 不会再有「被打就偷东西并赶人」的反应，这是有意的行为变更。

## 构建

需要 .NET SDK 与游戏目录下的 `RoR2.dll` / BepInEx 5 + HarmonyX：

```bash
dotnet build -c Release
dotnet build -c Release -p:GameDir="<游戏目录>" -p:BepInExCoreDir="<profile>\BepInEx\core"
```

英文说明见 [README_EN.md](README_EN.md)。

## 许可证

本项目以 **GNU General Public License v3.0 or later（GPL-3.0-or-later）** 发布，完整条款见 [LICENSE](LICENSE)。

你可以自由使用、修改和再分发，但分发修改版时需要同样以 GPL 授权并提供源码。

