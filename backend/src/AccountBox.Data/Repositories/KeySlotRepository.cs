using AccountBox.Data.DbContext;
using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Data.Repositories;

/// <summary>
/// KeySlot 仓储
/// 管理加密的 VaultKey 存储（单例模式）
/// </summary>
public class KeySlotRepository
{
    private readonly AccountBoxDbContext _context;

    public KeySlotRepository(AccountBoxDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 获取 KeySlot（如果存在）
    /// </summary>
    public async Task<KeySlot?> GetAsync()
    {
        return await _context.KeySlots
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 检查是否已初始化
    /// </summary>
    public async Task<bool> ExistsAsync()
    {
        return await _context.KeySlots.AnyAsync();
    }

    /// <summary>
    /// 创建 KeySlot（仅在初始化时调用）
    /// </summary>
    public async Task<KeySlot> CreateAsync(KeySlot keySlot)
    {
        if (keySlot == null)
        {
            throw new ArgumentNullException(nameof(keySlot));
        }

        // 确保单例约束
        if (await ExistsAsync())
        {
            throw new InvalidOperationException("KeySlot already exists. Only one KeySlot is allowed.");
        }

        // 强制设置 Id = 1（单例）
        keySlot.Id = 1;

        _context.KeySlots.Add(keySlot);
        await _context.SaveChangesAsync();

        return keySlot;
    }

    /// <summary>
    /// 更新 KeySlot（用于修改主密码）
    /// </summary>
    public async Task UpdateAsync(KeySlot keySlot)
    {
        if (keySlot == null)
        {
            throw new ArgumentNullException(nameof(keySlot));
        }

        // 确保 Id = 1
        if (keySlot.Id != 1)
        {
            throw new ArgumentException("KeySlot Id must be 1", nameof(keySlot));
        }

        // 先获取已跟踪的实体（如果存在），然后更新
        var existing = await _context.KeySlots.FindAsync(1);
        if (existing == null)
        {
            throw new InvalidOperationException("KeySlot does not exist");
        }

        // 更新属性
        _context.Entry(existing).CurrentValues.SetValues(keySlot);
        await _context.SaveChangesAsync();
    }
}
