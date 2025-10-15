-- SQLite FTS5 全文搜索虚拟表
-- 用于在网站名、域名、用户名、标签和备注中进行全文搜索

-- 创建 FTS5 虚拟表
CREATE VIRTUAL TABLE IF NOT EXISTS AccountsSearchIndex USING fts5(
    AccountId UNINDEXED,          -- 账号ID（不索引，仅用于关联）
    WebsiteId UNINDEXED,          -- 网站ID（不索引，仅用于关联）
    WebsiteDomain,                -- 网站域名（索引）
    WebsiteDisplayName,           -- 网站显示名称（索引）
    Username,                     -- 用户名（索引）
    Notes,                        -- 备注（索引）
    Tags,                         -- 标签（索引）
    content='Accounts',           -- 关联到 Accounts 表
    tokenize='unicode61'          -- 使用 unicode61 分词器（支持多语言）
);

-- 创建触发器：插入账号时同步到搜索索引
CREATE TRIGGER IF NOT EXISTS AccountsSearchIndex_Insert
AFTER INSERT ON Accounts
WHEN NEW.IsDeleted = 0
BEGIN
    INSERT INTO AccountsSearchIndex(
        AccountId,
        WebsiteId,
        WebsiteDomain,
        WebsiteDisplayName,
        Username,
        Notes,
        Tags
    )
    SELECT
        NEW.Id,
        NEW.WebsiteId,
        W.Domain,
        W.DisplayName,
        NEW.Username,
        COALESCE(NEW.Notes, ''),
        COALESCE(NEW.Tags, '')
    FROM Websites W
    WHERE W.Id = NEW.WebsiteId;
END;

-- 创建触发器：更新账号时同步到搜索索引
CREATE TRIGGER IF NOT EXISTS AccountsSearchIndex_Update
AFTER UPDATE ON Accounts
WHEN NEW.IsDeleted = 0
BEGIN
    DELETE FROM AccountsSearchIndex WHERE AccountId = OLD.Id;
    INSERT INTO AccountsSearchIndex(
        AccountId,
        WebsiteId,
        WebsiteDomain,
        WebsiteDisplayName,
        Username,
        Notes,
        Tags
    )
    SELECT
        NEW.Id,
        NEW.WebsiteId,
        W.Domain,
        W.DisplayName,
        NEW.Username,
        COALESCE(NEW.Notes, ''),
        COALESCE(NEW.Tags, '')
    FROM Websites W
    WHERE W.Id = NEW.WebsiteId;
END;

-- 创建触发器：删除账号（软删除或硬删除）时从搜索索引移除
CREATE TRIGGER IF NOT EXISTS AccountsSearchIndex_Delete
AFTER UPDATE ON Accounts
WHEN NEW.IsDeleted = 1
BEGIN
    DELETE FROM AccountsSearchIndex WHERE AccountId = OLD.Id;
END;

CREATE TRIGGER IF NOT EXISTS AccountsSearchIndex_HardDelete
AFTER DELETE ON Accounts
BEGIN
    DELETE FROM AccountsSearchIndex WHERE AccountId = OLD.Id;
END;

-- 创建触发器：更新网站信息时同步到搜索索引
CREATE TRIGGER IF NOT EXISTS AccountsSearchIndex_WebsiteUpdate
AFTER UPDATE ON Websites
BEGIN
    DELETE FROM AccountsSearchIndex WHERE WebsiteId = OLD.Id;
    INSERT INTO AccountsSearchIndex(
        AccountId,
        WebsiteId,
        WebsiteDomain,
        WebsiteDisplayName,
        Username,
        Notes,
        Tags
    )
    SELECT
        A.Id,
        A.WebsiteId,
        NEW.Domain,
        NEW.DisplayName,
        A.Username,
        COALESCE(A.Notes, ''),
        COALESCE(A.Tags, '')
    FROM Accounts A
    WHERE A.WebsiteId = NEW.Id AND A.IsDeleted = 0;
END;

-- 使用示例：
-- 搜索包含 "github" 的账号
-- SELECT AccountId FROM AccountsSearchIndex WHERE AccountsSearchIndex MATCH 'github' ORDER BY rank;

-- 搜索多个关键词（AND）
-- SELECT AccountId FROM AccountsSearchIndex WHERE AccountsSearchIndex MATCH 'github AND john' ORDER BY rank;

-- 搜索多个关键词（OR）
-- SELECT AccountId FROM AccountsSearchIndex WHERE AccountsSearchIndex MATCH 'github OR gitlab' ORDER BY rank;

-- 注意：
-- 1. rank 值越小表示匹配度越高
-- 2. MATCH 查询是大小写不敏感的
-- 3. 需要在应用层处理结果去重和分页
