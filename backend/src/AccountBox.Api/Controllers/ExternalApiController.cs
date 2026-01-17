using AccountBox.Api.DTOs.External;
using AccountBox.Api.Services;
using AccountBox.Core.Enums;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models;
using AccountBox.Core.Services;
using AccountBox.Data.Entities;
using AccountBox.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using InternalCreateAccountRequest = AccountBox.Core.Models.Account.CreateAccountRequest;

namespace AccountBox.Api.Controllers;

/// <summary>
/// 外部API控制器
/// 提供给外部系统的RESTful API接口，需要API密钥认证
/// </summary>
[ApiController]
[Route("api/external")]
public class ExternalApiController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IWebsiteRepository _websiteRepository;
    private readonly IRandomAccountService _randomAccountService;
    private readonly PasswordGeneratorService _passwordGeneratorService;

    public ExternalApiController(
        IAccountService accountService,
        IWebsiteRepository websiteRepository,
        IRandomAccountService randomAccountService,
        PasswordGeneratorService passwordGeneratorService)
    {
        _accountService = accountService;
        _websiteRepository = websiteRepository;
        _randomAccountService = randomAccountService;
        _passwordGeneratorService = passwordGeneratorService;
    }

    /// <summary>
    /// 验证API密钥是否有权访问指定网站
    /// </summary>
    private bool CanAccessWebsite(ApiKey apiKey, int websiteId)
    {
        // 如果作用域是All，可以访问所有网站
        if (apiKey.ScopeType == ApiKeyScopeType.All)
        {
            return true;
        }

        // 如果作用域是Specific，检查是否在允许列表中
        return apiKey.ApiKeyWebsiteScopes.Any(s => s.WebsiteId == websiteId);
    }

    /// <summary>
    /// 从HttpContext中获取API密钥
    /// </summary>
    private ApiKey? GetApiKeyFromContext()
    {
        return HttpContext.Items["ApiKey"] as ApiKey;
    }

    /// <summary>
    /// 创建账号
    /// POST /api/external/accounts
    /// </summary>
    [HttpPost("accounts")]
    public async Task<ActionResult<ApiResponse<object>>> CreateAccount(
        [FromBody] ExternalCreateAccountRequest request)
    {
        try
        {
            var apiKey = GetApiKeyFromContext();
            if (apiKey == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("API_KEY_MISSING", "API密钥缺失"));
            }

            // 验证密码非空（符合 FR-024 和 FR-025：密码不能为空，但不检查强度）
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "PASSWORD_EMPTY",
                    "密码不能为空"));
            }

            // 验证网站是否存在
            var website = await _websiteRepository.GetByIdAsync(request.WebsiteId);
            if (website == null)
            {
                return NotFound(ApiResponse<object>.Fail("WEBSITE_NOT_FOUND", "指定的网站不存在"));
            }

            // 验证API密钥是否有权访问该网站
            if (!CanAccessWebsite(apiKey, request.WebsiteId))
            {
                return StatusCode(403, ApiResponse<object>.Fail(
                    "ACCESS_DENIED",
                    "API密钥无权访问该网站"));
            }

            // 验证并解析扩展字段JSON格式
            Dictionary<string, object>? extendedData = null;
            if (!string.IsNullOrEmpty(request.Extend))
            {
                try
                {
                    extendedData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(request.Extend);
                }
                catch
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        "INVALID_JSON",
                        "扩展字段必须是有效的JSON格式"));
                }
            }

            // 转换为内部CreateAccountRequest
            var internalRequest = new InternalCreateAccountRequest
            {
                WebsiteId = request.WebsiteId,
                Username = request.Username,
                Password = request.Password,
                Tags = request.Tags,
                Notes = request.Notes,
                ExtendedData = extendedData
            };

            // 使用AccountService创建账号（无需 vaultKey）
            var accountResponse = await _accountService.CreateAsync(internalRequest);

            return Ok(ApiResponse<object>.Ok(new
            {
                id = accountResponse.Id,
                websiteId = accountResponse.WebsiteId,
                username = accountResponse.Username,
                status = accountResponse.Status,
                extend = accountResponse.ExtendedData,
                createdAt = accountResponse.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                $"创建账号失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 删除账号（移入回收站）
    /// DELETE /api/external/accounts/{id}
    /// </summary>
    [HttpDelete("accounts/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAccount(int id)
    {
        try
        {
            var apiKey = GetApiKeyFromContext();
            if (apiKey == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("API_KEY_MISSING", "API密钥缺失"));
            }

            var account = await _accountService.GetByIdAsync(id);
            if (account == null)
            {
                return NotFound(ApiResponse<object>.Fail("ACCOUNT_NOT_FOUND", "账号不存在"));
            }

            // 验证API密钥是否有权访问该账号所属的网站
            if (!CanAccessWebsite(apiKey, account.WebsiteId))
            {
                return StatusCode(403, ApiResponse<object>.Fail(
                    "ACCESS_DENIED",
                    "API密钥无权访问该网站"));
            }

            // 软删除（移入回收站）
            await _accountService.SoftDeleteAsync(id);

            return Ok(ApiResponse<object>.Ok(new
            {
                id = account.Id,
                message = "账号已移入回收站"
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                $"删除账号失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 更新账号状态（启用/禁用）
    /// PUT /api/external/accounts/{id}/status
    /// </summary>
    [HttpPut("accounts/{id}/status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateAccountStatus(
        int id,
        [FromBody] UpdateAccountStatusRequest request)
    {
        try
        {
            var apiKey = GetApiKeyFromContext();
            if (apiKey == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("API_KEY_MISSING", "API密钥缺失"));
            }

            var account = await _accountService.GetByIdAsync(id);
            if (account == null)
            {
                return NotFound(ApiResponse<object>.Fail("ACCOUNT_NOT_FOUND", "账号不存在"));
            }

            // 验证API密钥是否有权访问该账号所属的网站
            if (!CanAccessWebsite(apiKey, account.WebsiteId))
            {
                return StatusCode(403, ApiResponse<object>.Fail(
                    "ACCESS_DENIED",
                    "API密钥无权访问该网站"));
            }

            // 更新状态
            if (request.Status == "Active")
            {
                await _accountService.EnableAccountAsync(id);
            }
            else
            {
                await _accountService.DisableAccountAsync(id);
            }

            // 重新获取账号以返回更新后的信息
            var updatedAccount = await _accountService.GetByIdAsync(id);

            return Ok(ApiResponse<object>.Ok(new
            {
                id = updatedAccount!.Id,
                status = updatedAccount.Status,
                updatedAt = updatedAccount.UpdatedAt
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                $"更新账号状态失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取指定网站的账号列表（支持状态过滤）
    /// GET /api/external/websites/{websiteId}/accounts?status=Active
    /// </summary>
    [HttpGet("websites/{websiteId}/accounts")]
    public async Task<ActionResult<ApiResponse<object>>> GetAccountsByWebsite(
        int websiteId,
        [FromQuery] string? status = null)
    {
        try
        {
            var apiKey = GetApiKeyFromContext();
            if (apiKey == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("API_KEY_MISSING", "API密钥缺失"));
            }

            // 验证网站是否存在
            var website = await _websiteRepository.GetByIdAsync(websiteId);
            if (website == null)
            {
                return NotFound(ApiResponse<object>.Fail("WEBSITE_NOT_FOUND", "指定的网站不存在"));
            }

            // 验证API密钥是否有权访问该网站
            if (!CanAccessWebsite(apiKey, websiteId))
            {
                return StatusCode(403, ApiResponse<object>.Fail(
                    "ACCESS_DENIED",
                    "API密钥无权访问该网站"));
            }

            // 获取账号列表（不包含已删除的）
            var pagedResult = await _accountService.GetPagedAsync(
                pageNumber: 1,
                pageSize: 100,
                websiteId: websiteId);

            var accounts = pagedResult.Items;

            // 根据status参数过滤
            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    accounts = accounts.Where(a => a.Status == "Active").ToList();
                }
                else if (status.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                {
                    accounts = accounts.Where(a => a.Status == "Disabled").ToList();
                }
            }

            var result = accounts.Select(a => new
            {
                id = a.Id,
                websiteId = a.WebsiteId,
                username = a.Username,
                password = a.Password,
                tags = a.Tags,
                notes = a.Notes,
                status = a.Status,
                extendedData = a.ExtendedData,
                createdAt = a.CreatedAt,
                updatedAt = a.UpdatedAt
            }).ToList();

            return Ok(ApiResponse<object>.Ok(new
            {
                websiteId = websiteId,
                totalCount = result.Count,
                accounts = result
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                $"获取账号列表失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 随机获取指定网站下的一个启用状态的账号
    /// GET /api/external/websites/{websiteId}/accounts/random
    /// </summary>
    [HttpGet("websites/{websiteId}/accounts/random")]
    public async Task<ActionResult<ApiResponse<object>>> GetRandomEnabledAccount(int websiteId)
    {
        try
        {
            var apiKey = GetApiKeyFromContext();
            if (apiKey == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("API_KEY_MISSING", "API密钥缺失"));
            }

            // 验证网站是否存在
            var website = await _websiteRepository.GetByIdAsync(websiteId);
            if (website == null)
            {
                return NotFound(ApiResponse<object>.Fail("WEBSITE_NOT_FOUND", "指定的网站不存在"));
            }

            // 验证API密钥是否有权访问该网站
            if (!CanAccessWebsite(apiKey, websiteId))
            {
                return StatusCode(403, ApiResponse<object>.Fail(
                    "ACCESS_DENIED",
                    "API密钥无权访问该网站"));
            }

            // 随机获取一个启用状态的账号（带24小时缓存防重复）
            var account = await _randomAccountService.GetRandomEnabledAccountAsync(apiKey.Id, websiteId);

            if (account == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "NO_ENABLED_ACCOUNTS",
                    "该网站没有可用的启用账号"));
            }

            return Ok(ApiResponse<object>.Ok(new
            {
                id = account.Id,
                websiteId = account.WebsiteId,
                username = account.Username,
                password = account.Password,
                tags = account.Tags,
                notes = account.Notes,
                status = account.Status,
                extendedData = account.ExtendedData,
                createdAt = account.CreatedAt,
                updatedAt = account.UpdatedAt
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                $"随机获取账号失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取API密钥有权访问的网站列表
    /// GET /api/external/websites
    /// </summary>
    [HttpGet("websites")]
    public async Task<ActionResult<ApiResponse<object>>> GetAccessibleWebsites()
    {
        try
        {
            var apiKey = GetApiKeyFromContext();
            if (apiKey == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("API_KEY_MISSING", "API密钥缺失"));
            }

            IEnumerable<Data.Entities.Website> websites;

            // 如果作用域是All，返回所有网站
            if (apiKey.ScopeType == ApiKeyScopeType.All)
            {
                websites = await _websiteRepository.GetAllAsync();
            }
            else
            {
                // 如果作用域是Specific，返回授权的网站
                var websiteIds = apiKey.ApiKeyWebsiteScopes.Select(s => s.WebsiteId).ToList();
                websites = await _websiteRepository.GetByIdsAsync(websiteIds);
            }

            var result = websites.Select(w => new
            {
                id = w.Id,
                domain = w.Domain,
                displayName = w.DisplayName,
                createdAt = w.CreatedAt,
                updatedAt = w.UpdatedAt
            }).ToList();

            return Ok(ApiResponse<object>.Ok(new
            {
                totalCount = result.Count,
                websites = result
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                $"获取网站列表失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 生成随机密码
    /// GET /api/external/password/generate?length=8
    /// </summary>
    /// <param name="length">密码长度，默认为8，最小8，最大128</param>
    [HttpGet("password/generate")]
    public ActionResult<ApiResponse<object>> GeneratePassword([FromQuery] int? length = null)
    {
        try
        {
            var apiKey = GetApiKeyFromContext();
            if (apiKey == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("API_KEY_MISSING", "API密钥缺失"));
            }

            // 使用默认长度8或用户提供的长度
            var passwordLength = length ?? 8;

            // 验证长度范围
            if (passwordLength < 8 || passwordLength > 128)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "INVALID_LENGTH",
                    "密码长度必须在8到128之间"));
            }

            // 使用密码生成器服务生成密码
            var request = new Core.Models.PasswordGenerator.GeneratePasswordRequest
            {
                Length = passwordLength,
                IncludeUppercase = true,
                IncludeLowercase = true,
                IncludeNumbers = true,
                IncludeSymbols = true,
                ExcludeAmbiguous = true,
                UseCharacterDistribution = true,
                UppercasePercentage = 25,
                LowercasePercentage = 45,
                NumbersPercentage = 25,
                SymbolsPercentage = 5
            };

            var response = _passwordGeneratorService.Generate(request);

            return Ok(ApiResponse<object>.Ok(new
            {
                password = response.Password,
                length = response.Strength.Length,
                strength = new
                {
                    score = response.Strength.Score,
                    level = response.Strength.Level,
                    entropy = response.Strength.Entropy
                }
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                $"生成密码失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 检查指定网站下是否存在指定用户名的账号
    /// GET /api/external/websites/{websiteId}/accounts/check-username?username=xxx
    /// </summary>
    /// <param name="websiteId">网站ID</param>
    /// <param name="username">要检查的用户名</param>
    [HttpGet("websites/{websiteId}/accounts/check-username")]
    public async Task<ActionResult<ApiResponse<object>>> CheckUsernameExists(
        int websiteId,
        [FromQuery] string username)
    {
        try
        {
            var apiKey = GetApiKeyFromContext();
            if (apiKey == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("API_KEY_MISSING", "API密钥缺失"));
            }

            // 验证用户名参数
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "USERNAME_REQUIRED",
                    "用户名参数不能为空"));
            }

            // 验证网站是否存在
            var website = await _websiteRepository.GetByIdAsync(websiteId);
            if (website == null)
            {
                return NotFound(ApiResponse<object>.Fail("WEBSITE_NOT_FOUND", "指定的网站不存在"));
            }

            // 验证API密钥是否有权访问该网站
            if (!CanAccessWebsite(apiKey, websiteId))
            {
                return StatusCode(403, ApiResponse<object>.Fail(
                    "ACCESS_DENIED",
                    "API密钥无权访问该网站"));
            }

            // 检查用户名是否存在
            var exists = await _accountService.UsernameExistsAsync(websiteId, username);

            return Ok(ApiResponse<object>.Ok(new
            {
                websiteId = websiteId,
                username = username.Trim(),
                exists = exists
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                $"检查用户名失败: {ex.Message}"));
        }
    }
}
