# 加密系统移除总结

**日期**: 2025-10-17  
**影响范围**: 全项目架构变更

## 📋 概述

AccountBox 已从**加密存储模式**切换为**明文存储模式**，以简化架构并适配个人小型项目的需求。

## ⚠️ 重要变更

### 之前（加密模式）
- 使用 AES-256-GCM 加密算法
- Argon2id 密钥派生函数
- 信封加密机制（VaultKey + KEK）
- Session-based 密钥管理
- 主密码解锁系统

### 现在（明文模式）
- 密码直接以明文形式存储
- 无需加密服务
- 无需 Vault 和 Session 管理
- API 方法大幅简化

## 📝 规范文档状态

### 001-mvp 规范
**状态**: 保留原内容，添加注释说明

原 MVP 规范中包含完整的加密系统设计（User Story 6），这些内容已**不再适用**于当前版本：
- FR-036 至 FR-043（加密相关需求）
- KeySlot 实体
- VaultKey 管理
- 主密码解锁机制

**原因**: 001-mvp 是项目的历史记录，保留原内容有助于理解项目演进过程。

### 006-api-management 规范
**状态**: 已更新

- ✅ 移除了 Vault 解锁模式相关需求（FR-037 至 FR-044）
- ✅ 添加了明文存储模式说明
- ✅ 更新了安全建议

## 🗂️ 已删除的组件

### 后端
- `VaultController.cs`
- `VaultService.cs`
- `VaultSessionMiddleware.cs`
- `KeySlot.cs`（实体）
- `KeySlotRepository.cs`
- `KeySlotConfiguration.cs`
- AccountBox.Security 项目中的所有加密服务
  - `AesGcmEncryptionService`
  - `Argon2Service`
  - `VaultManager`

### 数据库
- `KeySlots` 表（已删除）
- `Accounts.PasswordEncrypted` 字段（已删除）
- `Accounts.PasswordIV` 字段（已删除）
- `Accounts.PasswordTag` 字段（已删除）
- `Accounts.NotesEncrypted` 字段（已删除）
- `Accounts.NotesIV` 字段（已删除）
- `Accounts.NotesTag` 字段（已删除）
- `ApiKeys.VaultId` 外键（已删除）

### 新增内容
- `Accounts.Password` (TEXT) - 明文存储

### 前端
- `InitializePage.tsx` - 主密码初始化页面（已删除）
- `UnlockPage.tsx` - Vault 解锁页面（已删除）
- `VaultContext.tsx` - Vault 上下文提供者（已删除）
- `useVault.ts` - Vault 状态管理 Hook（已删除）
- `vaultService.ts` - Vault API 服务（已删除）
- `ChangeMasterPasswordDialog.tsx` - 修改主密码对话框（已删除）
- `WebsitesPage.tsx` - 移除了"锁定"和"修改主密码"功能按钮
- `App.tsx` - 移除了 Vault 初始化和解锁路由守卫
- `apiClient.ts` - 移除了 X-Vault-Session 请求头和 Session 管理
- `types/common.ts` - 移除了 VaultStatus 和 VaultSession 类型定义

## 📊 迁移记录

**迁移文件**: `20251017021759_RemoveEncryption.cs`

已成功应用到数据库，包括：
- 删除 KeySlots 表
- 移除 Account 表的所有加密字段
- 添加 Password 明文字段
- 移除 ApiKey 的 VaultId 关联

## 🔒 安全建议

由于现在采用明文存储，**强烈建议**：

1. ✅ **访问控制**: 仅在 localhost 或 VPN 环境访问
2. ✅ **防火墙**: 启用防火墙保护，限制端口访问
3. ✅ **磁盘加密**: 使用操作系统级别的磁盘加密（FileVault/BitLocker/LUKS）
4. ✅ **定期备份**: 定期备份数据库文件并加密存储
5. ✅ **HTTPS**: 生产环境必须使用 HTTPS
6. ✅ **适用场景**: 个人自托管环境，不适合共享或公开服务器

## 📚 相关文档

- `/REMOVE_ENCRYPTION_PLAN.md` - 详细实施计划
- `/REMOVE_ENCRYPTION_PROGRESS.md` - 完成进度记录
- `/Augment-Memories.md` - 项目记忆库（已更新）
- `/specs/006-api-management/spec.md` - 已更新规范
- `/specs/006-api-management/plan.md` - 已更新计划

## ❓ 常见问题

### Q: 为什么移除加密？
A: 项目定位为个人小型工具，加密系统过于复杂（约2000行代码），且影响外部API使用。明文存储配合其他安全措施（防火墙、磁盘加密）可以满足个人自托管场景的需求。

### Q: 数据安全如何保障？
A: 通过多层安全措施：
- 网络层：防火墙 + VPN/localhost访问 + HTTPS
- 系统层：磁盘加密 + 文件权限控制
- 数据层：定期加密备份

### Q: 如果需要恢复加密怎么办？
A: 可以回退到移除加密前的 Git 提交，但需要重新迁移数据库。建议备份当前数据后再操作。

### Q: 001-mvp 规范中的加密内容怎么办？
A: 保留作为历史记录。新功能开发应参考 006-api-management 及后续规范，它们已反映明文存储架构。

## 🚀 后续工作

### 已完成 ✅
- 后端代码更新
- 数据库迁移
- 文档更新
- 前端 Vault 解锁页面删除 (InitializePage.tsx, UnlockPage.tsx)
- 前端 vault 相关服务移除 (vaultService.ts, VaultContext.tsx, useVault.ts)
- HTTP 拦截器简化 (apiClient.ts)
- 路由配置更新 (App.tsx)
- 类型定义清理 (types/common.ts)
- 网站管理页面更新 (WebsitesPage.tsx - 移除"锁定"和"修改主密码"按钮)

### 待处理 ⏳
- 001-mvp 契约文件更新（可选）

---

*本文档记录了 AccountBox 从加密存储到明文存储的完整架构变更过程。*
