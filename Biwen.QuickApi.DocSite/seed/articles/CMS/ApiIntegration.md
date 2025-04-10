# API 集成

Biwen.QuickApi.Contents 提供了完整的 RESTful API，用于管理和操作内容。本文档将详细介绍如何使用这些 API。

## API 概述

所有 API 均以 `/contents` 为基础路径（可配置），支持以下主要功能：

- 内容的增删改查（CRUD）操作
- 内容版本管理
- 内容审计日志查询

## API 端点

### 内容管理 API

#### 获取内容列表

```http
GET /contents/infopages
```

查询参数：
- `pageNumber`：页码（从1开始）
- `pageSize`：每页大小
- `contentType`：内容类型（必填）
- `slug`：按Slug筛选
- `title`：按标题筛选
- `status`：内容状态筛选

响应示例：
```json
{
  "items": [
    {
      "id": "5f9c7b4e-3c1a-4f5b-9d1a-6c2a5e34b7d9",
      "title": "示例文章",
      "slug": "sample-post",
      "contentType": "BlogPost",
      "status": 1,
      "createTime": "2025-04-10T10:00:00Z",
      "updateTime": "2025-04-10T10:00:00Z"
    }
  ],
  "totalCount": 100,
  "pageSize": 10,
  "pageNumber": 1,
  "totalPages": 10
}
```

#### 获取单个内容

```http
GET /contents/{id:guid}
```

响应示例：
```json
{
  "id": "5f9c7b4e-3c1a-4f5b-9d1a-6c2a5e34b7d9",
  "title": "示例文章",
  "slug": "sample-post",
  "contentType": "BlogPost",
  "status": 1,
  "content": {
    "title": {
      "fieldType": "Text",
      "value": "示例文章"
    },
    "content": {
      "fieldType": "Markdown",
      "value": "# 示例内容\n\n这是一篇示例文章。"
    }
  },
  "createTime": "2025-04-10T10:00:00Z",
  "updateTime": "2025-04-10T10:00:00Z"
}
```

#### 创建内容

```http
POST /contents/create
Content-Type: application/json

{
  "title": "新文章",
  "slug": "new-post",
  "contentType": "BlogPost",
  "jsonContent": [
    {
      "fieldName": "Title",
      "value": "新文章标题"
    },
    {
      "fieldName": "Content",
      "value": "# 文章内容\n\n这是新文章的内容。"
    }
  ]
}
```

响应示例：
```json
{
  "id": "8f7e6d5c-4b3a-2d1e-9c8b-7a6b5c4d3e2f",
  "title": "新文章",
  "slug": "new-post",
  "contentType": "BlogPost",
  "status": 0
}
```

#### 更新内容

```http
PUT /contents/{id:guid}
Content-Type: application/json

{
  "title": "更新的文章",
  "jsonContent": [
    {
      "fieldName": "Title",
      "value": "更新后的标题"
    },
    {
      "fieldName": "Content",
      "value": "# 更新的内容\n\n这是更新后的内容。"
    }
  ]
}
```

#### 删除内容

```http
DELETE /contents/{id:guid}
```

### 版本管理 API

#### 获取版本列表

```http
GET /contents/versions/{id:guid}
```

响应示例：
```json
[
  {
    "id": "7d6e5f4c-3b2a-1d9e-8c7b-6a5b4c3d2e1f",
    "contentId": "5f9c7b4e-3c1a-4f5b-9d1a-6c2a5e34b7d9",
    "version": 1,
    "createTime": "2025-04-10T10:00:00Z",
    "creator": "admin",
    "comment": "初始版本"
  }
]
```

#### 获取特定版本

```http
GET /contents/versions/{id:guid}/{version:int}
```

#### 恢复到指定版本

```http
POST /contents/versions/{id:guid}/restore
Content-Type: application/json

{
  "version": 1,
  "comment": "恢复到初始版本"
}
```

### 审计日志 API

#### 获取审计日志

```http
GET /contents/{id:guid}/auditlogs
```

响应示例：
```json
[
  {
    "id": "9e8d7c6b-5a4b-3c2d-1e0f-9a8b7c6d5e4f",
    "contentId": "5f9c7b4e-3c1a-4f5b-9d1a-6c2a5e34b7d9",
    "operation": "Create",
    "operator": "admin",
    "operationTime": "2025-04-10T10:00:00Z",
    "details": "创建内容"
  }
]
```

## C# 客户端示例

### 使用 HttpClient

```csharp
public class ContentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<ContentApiClient> _logger;

    public ContentApiClient(
        string baseUrl,
        ILogger<ContentApiClient> logger)
    {
        _baseUrl = baseUrl;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task<ContentDto> GetContentAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/contents/{id}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ContentDto>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取内容失败: {Id}", id);
            throw;
        }
    }

    public async Task<Guid> CreateContentAsync(CreateContentRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/contents/create", 
                content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateContentResponse>(responseJson);
            return result.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建内容失败");
            throw;
        }
    }
}
```

## 最佳实践

1. **API 使用**
   - 使用适当的 HTTP 方法
   - 正确处理错误响应
   - 实现重试机制
   - 使用异步操作

2. **安全性**
   - 使用 HTTPS
   - 实现认证和授权
   - 验证输入数据
   - 限制请求频率

3. **性能优化**
   - 使用缓存
   - 实现分页
   - 优化请求大小
   - 使用压缩

## 下一步

- [内容类型定义](ContentTypes.md)
- [字段类型](FieldTypes.md)
- [内容渲染](ContentRendering.md)
