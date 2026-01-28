# Hướng dẫn Structured Output trong .NET với MISA AI API

## Mục lục
1. [Giới thiệu](#giới-thiệu)
2. [Cài đặt môi trường](#cài-đặt-môi-trường)
3. [Cấu trúc Request cơ bản](#cấu-trúc-request-cơ-bản)
4. [Định nghĩa JSON Schema](#định-nghĩa-json-schema)
5. [Ví dụ chi tiết](#ví-dụ-chi-tiết)
6. [Xử lý Response](#xử-lý-response)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

---

## Giới thiệu

**Structured Output** cho phép bạn yêu cầu LLM trả về dữ liệu theo một schema JSON cụ thể, đảm bảo output có cấu trúc nhất quán và dễ dàng parse trong code.

### Ưu điểm:
- Output có định dạng nhất quán
- Dễ dàng deserialize sang object C#
- Giảm thiểu lỗi parse JSON
- Hỗ trợ cho production

---

## Cài đặt môi trường



### Các namespace sử dụng

```csharp
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
```

---

## Cấu trúc Request cơ bản

### Endpoint

```
POST http://test-k8s.misa.local/llm-gateway/v1/chat/completions
```

### Headers

```csharp
client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
client.DefaultRequestHeaders.Add("HTTP-Referer", "yor-openai-url");  
client.DefaultRequestHeaders.Add("X-Title", "Your App Name");
```

### Request Body Structure

```json
{
  "model": "misa-ai-1.0", // model name
  "temperature": 0.2,
  "messages": [
    {"role": "system", "content": "..."}, // role such as tool, system, user, assistant
    {"role": "user", "content": "..."}
  ],
  "response_format": {
    "type": "json_schema",
    "json_schema": {
      "name": "schema_name",
      "strict": true,
      "schema": { ... }
    }
  }
}
```

---

## Định nghĩa JSON Schema

### Cấu trúc cơ bản của `response_format`

```csharp
response_format = new
{
    type = "json_schema",
    json_schema = new
    {
        name = "your_schema_name",    // Tên schema
        strict = true,                 // Bắt buộc tuân theo schema
        schema = new { ... }           // Định nghĩa schema
    }
}
```

### Các kiểu dữ liệu được hỗ trợ

| Kiểu | JSON Schema | C# Type |
|------|-------------|---------|
| Chuỗi | `"type": "string"` | `string` |
| Số nguyên | `"type": "integer"` | `int`, `long` |
| Số thực | `"type": "number"` | `decimal`, `double`, `float` |
| Boolean | `"type": "boolean"` | `bool` |
| Mảng | `"type": "array"` | `T[]`, `List<T>` |
| Object | `"type": "object"` | `class` |
| Null | `"type": "null"` | `null` |

---

## Ví dụ chi tiết

### Ví dụ 1: Object đơn giản

**C# Model:**
```csharp
public class Person
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}
```

**JSON Schema trong request:**
```csharp
response_format = new
{
    type = "json_schema",
    json_schema = new
    {
        name = "person_info",
        strict = true,
        schema = new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string" },
                age = new { type = "integer" },
                email = new { type = "string" }
            },
            required = new[] { "name", "age", "email" },
            additionalProperties = false
        }
    }
}
```

### Ví dụ 2: Array of Objects

**C# Model:**
```csharp
public class Product
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("inStock")]
    public bool InStock { get; set; }
}
```

**JSON Schema trong request:**
```csharp
response_format = new
{
    type = "json_schema",
    json_schema = new
    {
        name = "product_list",
        strict = true,
        schema = new
        {
            type = "array",
            items = new
            {
                type = "object",
                properties = new
                {
                    id = new { type = "integer" },
                    name = new { type = "string" },
                    price = new { type = "number" },
                    inStock = new { type = "boolean" }
                },
                required = new[] { "id", "name", "price", "inStock" },
                additionalProperties = false
            }
        }
    }
}
```

### Ví dụ 3: Nested Objects

**C# Model:**
```csharp
public class Order
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonPropertyName("customer")]
    public Customer Customer { get; set; }

    [JsonPropertyName("items")]
    public OrderItem[] Items { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }
}

public class Customer
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }
}

public class OrderItem
{
    [JsonPropertyName("productName")]
    public string ProductName { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
}
```

**JSON Schema trong request:**
```csharp
response_format = new
{
    type = "json_schema",
    json_schema = new
    {
        name = "order_info",
        strict = true,
        schema = new
        {
            type = "object",
            properties = new
            {
                orderId = new { type = "string" },
                customer = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string" },
                        phone = new { type = "string" }
                    },
                    required = new[] { "name", "phone" },
                    additionalProperties = false
                },
                items = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            productName = new { type = "string" },
                            quantity = new { type = "integer" },
                            unitPrice = new { type = "number" }
                        },
                        required = new[] { "productName", "quantity", "unitPrice" },
                        additionalProperties = false
                    }
                },
                totalAmount = new { type = "number" }
            },
            required = new[] { "orderId", "customer", "items", "totalAmount" },
            additionalProperties = false
        }
    }
}
```

### Ví dụ 4: Enum-like values (sử dụng constraints)

```csharp
response_format = new
{
    type = "json_schema",
    json_schema = new
    {
        name = "task_status",
        strict = true,
        schema = new
        {
            type = "object",
            properties = new
            {
                taskId = new { type = "string" },
                status = new 
                { 
                    type = "string",
                    @enum = new[] { "pending", "in_progress", "completed", "cancelled" }
                },
                priority = new 
                { 
                    type = "string",
                    @enum = new[] { "low", "medium", "high", "critical" }
                }
            },
            required = new[] { "taskId", "status", "priority" },
            additionalProperties = false
        }
    }
}
```

---

## Xử lý Response

### 1. Định nghĩa Response Models

```csharp
public class OpenRouterResponse
{
    [JsonPropertyName("choices")]
    public Choice[] Choices { get; set; }
}

public class Choice
{
    [JsonPropertyName("message")]
    public Message Message { get; set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}
```

### 2. Parse Response

```csharp
// Gửi request
var response = await client.PostAsync(
    "your-url",
    new StringContent(json, Encoding.UTF8, "application/json")
);

// Đọc raw response
var raw = await response.Content.ReadAsStringAsync();

// Parse OpenRouter response
var parsed = JsonSerializer.Deserialize<OpenRouterResponse>(raw, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
});

// Lấy content từ assistant
var assistantContent = parsed?.Choices?.FirstOrDefault()?.Message?.Content;

// Parse structured output sang model của bạn
if (!string.IsNullOrWhiteSpace(assistantContent))
{
    var yourData = JsonSerializer.Deserialize<YourModel>(assistantContent, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });
    
    // Sử dụng yourData...
}
```

### 3. Full Example với Error Handling

```csharp
try
{
    var response = await client.PostAsync(endpoint, content);
    
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"API Error: {response.StatusCode}");
        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error details: {errorContent}");
        return;
    }

    var raw = await response.Content.ReadAsStringAsync();
    
    if (string.IsNullOrWhiteSpace(raw))
    {
        Console.WriteLine("Empty response");
        return;
    }

    var parsed = JsonSerializer.Deserialize<OpenRouterResponse>(raw);
    var assistantContent = parsed?.Choices?.FirstOrDefault()?.Message?.Content;

    if (string.IsNullOrWhiteSpace(assistantContent))
    {
        Console.WriteLine("No content in response");
        return;
    }

    var result = JsonSerializer.Deserialize<YourModel>(assistantContent);
    // Process result...
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"JSON Parse Error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected Error: {ex.Message}");
}
```

---

## Best Practices

### 1. Sử dụng `strict: true`
```csharp
json_schema = new
{
    name = "...",
    strict = true,  // Luôn đặt true để đảm bảo output tuân theo schema
    schema = new { ... }
}
```

### 2. Luôn khai báo `required` fields
```csharp
schema = new
{
    type = "object",
    properties = new { ... },
    required = new[] { "field1", "field2" },  // Liệt kê tất cả fields bắt buộc
    additionalProperties = false              // Không cho phép fields ngoài schema
}
```

### 3. Sử dụng `[JsonPropertyName]` attribute
```csharp
public class MyModel
{
    [JsonPropertyName("fieldName")]  // Đảm bảo tên property khớp với schema
    public string FieldName { get; set; }
}
```

### 4. Set timeout phù hợp
```csharp
client.Timeout = TimeSpan.FromMinutes(5);  // Tăng timeout cho request dài
```

### 5. Temperature thấp cho structured output
```csharp
var body = new
{
    model = "misa-ai-1.0",
    temperature = 0.2,  // Temperature thấp giúp output ổn định hơn
    ...
};
```

---

## Troubleshooting

### Lỗi: Response không theo schema

**Nguyên nhân:** `strict` không được set hoặc set `false`

**Giải pháp:**
```csharp
json_schema = new
{
    strict = true,  // Đảm bảo strict = true
    ...
}
```

### Lỗi: JSON Parse Error

**Nguyên nhân:** Property name trong C# model không khớp với schema

**Giải pháp:**
```csharp
// Sử dụng JsonPropertyName
[JsonPropertyName("exactSchemaName")]
public string MyProperty { get; set; }

// Hoặc sử dụng case-insensitive option
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
```

### Lỗi: Timeout

**Nguyên nhân:** Prompt quá dài hoặc schema phức tạp

**Giải pháp:**
```csharp
client.Timeout = TimeSpan.FromMinutes(10);  // Tăng timeout
```

### Lỗi: Empty response

**Nguyên nhân:** Token limit bị vượt quá

**Giải pháp:**
- Giảm độ dài prompt
- Giảm số lượng items trong array schema
- Thêm `max_tokens` parameter

```csharp
var body = new
{
    model = "misa-ai-1.0",
    max_tokens = 4096,  // Giới hạn output là bắt buộc
    ...
};
```

---

## Template Code Hoàn Chỉnh

```csharp
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// Response Models
public class OpenRouterResponse
{
    [JsonPropertyName("choices")]
    public Choice[] Choices { get; set; }
}

public class Choice
{
    [JsonPropertyName("message")]
    public Message Message { get; set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}

// Your Output Model
public class YourOutputModel
{
    [JsonPropertyName("field1")]
    public string Field1 { get; set; }

    [JsonPropertyName("field2")]
    public int Field2 { get; set; }
}

class Program
{
    static async Task Main()
    {
        var apiKey = "YOUR_API_KEY";
        
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromMinutes(5);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("HTTP-Referer", "your-url");

        var body = new
        {
            model = "misa-ai-1.0",
            temperature = 0.2,
            messages = new[]
            {
                new { role = "system", content = "Your system prompt" },
                new { role = "user", content = "Your user message" }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "your_schema",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        properties = new
                        {
                            field1 = new { type = "string" },
                            field2 = new { type = "integer" }
                        },
                        required = new[] { "field1", "field2" },
                        additionalProperties = false
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(body);

        try
        {
            var response = await client.PostAsync(
                "your-url",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var raw = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<OpenRouterResponse>(raw);
            var content = parsed?.Choices?.FirstOrDefault()?.Message?.Content;

            if (!string.IsNullOrWhiteSpace(content))
            {
                var result = JsonSerializer.Deserialize<YourOutputModel>(content);
                Console.WriteLine($"Field1: {result.Field1}");
                Console.WriteLine($"Field2: {result.Field2}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

---

## Tài liệu tham khảo

- [JSON Schema Specification](https://json-schema.org/)
- [System.Text.Json Documentation](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- [OpenAI API Structured Outputs](https://platform.openai.com/docs/guides/structured-outputs)

---

*Tài liệu được tạo cho MISA AI API - Phiên bản 1.0- trên test local url và openrouter*
