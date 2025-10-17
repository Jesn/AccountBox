# 移除加密功能实施计划

## 目标
将 AccountBox 从加密存储模式改为明文存储模式，简化系统架构，解决外部 API 的 VaultKey 问题。

## 已完成 ✅

1. **数据模型简化**
   - ✅ Account 实体：删除加密字段（PasswordEncrypted, PasswordIV, PasswordTag, NotesEncrypted等），添加 Password 字段
   - ✅ AccountConfiguration：更新 EF Core 配置
   - ✅ AccountService：完全重写，移除所有加密逻辑和 vaultKey 参数

## 待完成 📋

### Backend（预计 3-4 小时）

#### 1. 更新控制器（移除 vaultKey 参数）
- [ ] AccountController.cs
  - 删除 `GetVaultKey()` 方法
  - 移除所有方法调用中的 vaultKey 参数
  - 更新：GetPagedAsync, GetByIdAsync, CreateAsync, UpdateAsync
  
- [ ] ExternalApiController.cs  
  - 删除 `GetVaultKeyFromContext()` 方法
  - 移除所有 VaultKey 检查逻辑
  - 更新：CreateAccount, GetAccountsList
  
- [ ] SearchController.cs
  - 删除 `GetVaultKey()` 方法
  - 更新 SearchAsync 调用
  
- [ ] RecycleBinController.cs
  - 删除 `GetVaultKey()` 方法
  - 更新 GetDeletedAccountsAsync 调用

#### 2. 更新服务层
- [ ] RecycleBinService.cs
  - 移除 vaultKey 参数
  - 移除解密逻辑
  
- [ ] SearchService.cs
  - 移除 vaultKey 参数
  - 移除解密逻辑

#### 3. 删除 Vault 相关组件
- [ ] 删除 VaultController.cs
- [ ] 删除 VaultService.cs
- [ ] 删除 VaultSessionMiddleware.cs
- [ ] 删除 KeySlot 实体和仓储
- [ ] 删除 AccountBox.Security 项目引用

#### 4. 更新 Program.cs
- [ ] 移除 VaultSessionMiddleware 注册
- [ ] 移除加密服务注册：
  - Argon2Service
  - IEncryptionService
  - IVaultManager
- [ ] 移除 VaultService 注册
- [ ] 移除 KeySlotRepository 注册

#### 5. 数据库迁移
- [ ] 创建迁移：`dotnet ef migrations add RemoveEncryption`
- [ ] 应用迁移：`dotnet ef database update`

### Frontend（预计 1-2 小时）

#### 6. 删除 Vault UI
- [ ] 删除解锁页面组件
- [ ] 删除 vault 相关服务
- [ ] 移除路由中的解锁页面
- [ ] 更新 App.tsx：移除 Session ID 管理
- [ ] 简化 HTTP 拦截器

### Testing（预计 1 小时）

#### 7. 测试和验证
- [ ] 后端编译测试
- [ ] 前端编译测试
- [ ] 手动测试：创建/更新/删除账号
- [ ] 外部 API 测试：验证无需 VaultKey 即可工作

### Documentation（预计 0.5 小时）

#### 8. 更新文档
- [ ] 更新 specs/006-api-management/spec.md：移除 Vault 解锁模式说明
- [ ] 更新 specs/006-api-management/plan.md：说明架构变更
- [ ] 更新 CLAUDE.md：记录此次重大架构变更

## 实施顺序

**阶段 1：完成 Backend 改造**
1. 更新所有控制器（1h）
2. 更新服务层（0.5h）
3. 删除 Vault 组件（0.5h）
4. 更新 Program.cs（0.5h）
5. 数据库迁移（0.5h）
6. 测试 Backend 编译（0.5h）

**阶段 2：Frontend 改造**
1. 删除 Vault UI（1h）
2. 测试 Frontend 编译（0.5h）

**阶段 3：集成测试和文档**
1. 端到端测试（0.5h）
2. 更新文档（0.5h）

## 总工作量：5-7 小时

## 注意事项

⚠️ **数据迁移风险**
- 如果有现有加密数据，迁移会失败
- 建议：先清空数据库或导出数据

⚠️ **不可逆变更**
- 一旦移除加密，后续难以恢复
- 建议：Git 分支保护

⚠️ **安全提醒**
- 明文存储意味着数据库泄露 = 完全泄露
- 适用于个人自托管场景
- 需要通过其他方式保护（防火墙、磁盘加密、备份加密）

## 下一步

执行命令查看具体修改点：
```bash
# 查看所有需要修改 vaultKey 的地方
grep -r "vaultKey" backend/src/AccountBox.Api/Controllers/
grep -r "vaultKey" backend/src/AccountBox.Api/Services/
```

开始实施：
```bash
# 从 Backend 控制器开始
code backend/src/AccountBox.Api/Controllers/AccountController.cs
```
