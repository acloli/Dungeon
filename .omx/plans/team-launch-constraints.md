# Team Launch Constraints — Vox Dungeon

## Source of truth
- PRD: `.omx/plans/prd-vox-dungeon-full-plan.md`
- Test Spec: `.omx/plans/test-spec-vox-dungeon-full-plan.md`

## Latest user overrides (higher priority than earlier docs)
1. 开发项目直接在 `DungeonUnity/Assets/` 下进行，**不使用** `_Project/` 目录。
2. 任何涉及 **prefab / GameObject / scene** 编辑的工作，**必须调用 unity-mcp**，不要只停留在代码或文档层假设。
3. 项目名当前暂定为 **Dungeon**，彻底落地后再做正式命名决策。

## Execution emphasis
- 先按 PRD/Test Spec 的 Gate A 推进，再进入 Gate B。
- MasterData 首批实现必须同时落地 settings / 注册 / 校验入口。
- 所有实现以真实 TFramework 接口为准，不照抄示意 API。
- 若 BossPhase 需要降级，视为 re-plan / scope-change，不能算 Gate B 通过。
