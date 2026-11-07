# Changelog

## 0.1.2

- 配置项说明改为中英双语 / Config descriptions are now bilingual
- 补发提示跟随游戏语言（中文界面显示中文，其余显示英文）/ Refund message follows the game language

## 0.1.1

- 新增 `LICENSE`（GPL-3.0-or-later），源码文件加 SPDX 头 / Added GPL-3.0-or-later license and SPDX headers
- README 补充与 NoSpexKickout 的对比说明 / Documented how this differs from NoSpexKickout
- 补上 `website_url` 与 BepInExPack 依赖 / Added `website_url` and the BepInExPack dependency

## 0.1.0

- 首个版本：拦截 `SetStateOnHurt.SetPain()` 与 `SolusVendorMainState.CanExecuteSkill()`，SPEX 不再进入偷取状态（`PreventSpexAnger`）/ Initial release: block the SPEX steal state via `SetStateOnHurt.SetPain()` and `SolusVendorMainState.CanExecuteSkill()` (`PreventSpexAnger`)
- 被赶出交易所时，把暴露小脑返还给最初提交供奉的玩家（`RefundExposedCerebellum`）/ Refund the Exposed Cerebellum to the player who offered it (`RefundExposedCerebellum`)
- 补发时向全队广播提示（`AnnounceRefund`）/ Optional team announcement on refund (`AnnounceRefund`)
