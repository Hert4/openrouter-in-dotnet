using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// ndkfj
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

// Define structured output model
public class SalaryBenchmark
{
    [JsonPropertyName("Ky")]
    public string Ky { get; set; }

    [JsonPropertyName("DonViApDung")]
    public string DonViApDung { get; set; }

    [JsonPropertyName("NhomViTri")]
    public string NhomViTri { get; set; }

    [JsonPropertyName("ThamNien")]
    public string ThamNien { get; set; }

    [JsonPropertyName("MucLuongToiThieu")]
    public decimal MucLuongToiThieu { get; set; }

    [JsonPropertyName("MucLuongToiDa")]
    public decimal MucLuongToiDa { get; set; }

    [JsonPropertyName("MucLuongBinhQuan")]
    public decimal MucLuongBinhQuan { get; set; }

    [JsonPropertyName("NguonTrichDan")]
    public string NguonTrichDan { get; set; }
}

// main
class Program
{
    /// <summary>
    /// Đọc tất cả các file markdown từ folder và trả về nội dung gộp lại
    /// </summary>
    /// <param name="markdownFolderPath">Đường dẫn đến folder chứa các file markdown</param>
    /// <returns>Nội dung tất cả các file markdown được gộp lại</returns>
    static string ReadMarkdownFiles(string markdownFolderPath)
    {
        if (!Directory.Exists(markdownFolderPath))
        {
            Console.WriteLine($"Folder không tồn tại: {markdownFolderPath}");
            return string.Empty;
        }

        var markdownFiles = Directory.GetFiles(markdownFolderPath, "*.md");
        
        if (markdownFiles.Length == 0)
        {
            Console.WriteLine($"Không tìm thấy file markdown trong folder: {markdownFolderPath}");
            return string.Empty;
        }

        var contentBuilder = new StringBuilder();
        
        foreach (var filePath in markdownFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var fileContent = File.ReadAllText(filePath, Encoding.UTF8);
            
            contentBuilder.AppendLine($"\n\n===== BÁO CÁO: {fileName} =====\n");
            contentBuilder.AppendLine(fileContent);
            contentBuilder.AppendLine("\n===== HẾT BÁO CÁO =====\n");
            
            Console.WriteLine($"Đã đọc file: {fileName} ({fileContent.Length} ký tự)");
        }

        Console.WriteLine($"Tổng số file markdown đã đọc: {markdownFiles.Length}");
        return contentBuilder.ToString();
    }

    static async Task Main()
    {
        var apiKey = Environment.GetEnvironmentVariable("API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("Chưa có API Key");
            return;
        }

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromMinutes(5);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("HTTP-Referer", "http://test-k8s.misa.local/llm-gateway/v1");
        client.DefaultRequestHeaders.Add("X-Title", "CB Salary Benchmark Tool");

        // ===== ĐỌC NỘI DUNG TỪ FOLDER MARKDOWN =====
        // Có nhiều cách để cấu hình đường dẫn:
        
        // Cách 1: Sử dụng đường dẫn tuyệt đối (absolute path) - Khuyến nghị cho môi trường cố định
        var markdownFolderPath = "/home/misa/CUA/Mem-Agent/markdown";
        
        // Cách 2: Sử dụng đường dẫn tương đối từ thư mục project
        // var markdownFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "markdown");
        
        // Cách 3: Lấy từ environment variable - Linh hoạt cho nhiều môi trường
        // var markdownFolderPath = Environment.GetEnvironmentVariable("MARKDOWN_FOLDER") 
        //     ?? "/home/misa/CUA/Mem-Agent/markdown";
        
        // Cách 4: Đường dẫn tương đối từ bin/Debug/netX.0 (khi chạy với dotnet run)
        // var markdownFolderPath = Path.Combine("..", "..", "..", "..", "markdown");
        
        Console.WriteLine($"Đang đọc markdown từ: {Path.GetFullPath(markdownFolderPath)}");
        var markdownContent = ReadMarkdownFiles(markdownFolderPath);
        Console.WriteLine();

        // ===== SYSTEM PROMPT =====
        var systemPrompt = @"
Bạn là một Chuyên viên Phân tích Lương thưởng (C&B Analyst) giàu kinh nghiệm.

Nhiệm vụ: Thiết lập bảng mặt bằng lương thị trường cho kỳ <10/2025>, áp dụng cho đơn vị <Khối sản xuất>.

Nguồn dữ liệu đầu vào:
- Danh sách vị trí công việc cần phân tích: Sử dụng file ảnh đính kèm.
- Các báo cáo lương thị trường: Sử dụng các file báo cáo đính kèm.

Yêu cầu chi tiết:
- Về vị trí công việc:
Xét các vị trí công việc có trùng tên với vị trí công việc tại công ty hoặc so sánh mô tả công việc xem khớp với mô tả công việc tại vị trí nào của công ty để lấy dữ liệu tại vị trí công việc đó báo cáo
- Về thâm niên:
Phân tích theo các mốc thâm niên sau: dưới 1 năm, 1-3 năm, 3-5 năm, 5-8 năm, trên 8 năm.
Quy tắc khớp thâm niên: 
+ Nếu không có mốc thâm niên trùng với mốc thâm niên tại công ty thì lấy mốc thâm niên gần nhất
+ Nếu mốc thâm niên tại công ty giao giữa 2 mốc thâm niên trên báo cáo thì lấy giá trị trung bình
VD: mốc thâm niên tại công ty là 2-4 năm, mốc thâm niên trên báo cáo là 1-3 năm và 3-5 năm thì lấy giá trị trung bình của 2 mốc trên
+ Đối với những báo cáo không có mốc thâm niên, chỉ có mức level của vị trí thì sẽ map theo mức level dựa vào so sánh mô tả công việc tại mức level trên báo cáo khớp với mô tả công việc theo thâm niên nào của công ty
- Về tính toán: thống kê toàn bộ mức lương trên thị trường mà HR tổng hợp được theo vị trí công việc và thâm niên:
Mức lương Tối thiểu: Lấy giá trị thấp nhất trong tất cả dữ liệu thu thập được.
Mức lương Tối đa: Lấy giá trị cao nhất trong tất cả dữ liệu thu thập được.
Mức lương Bình quân: Lấy giá trị trung vị (median) của tất cả dữ liệu thu thập được.
- Về tiền tệ:
Quy đổi tất cả các mức lương sang đơn vị VND.
Sử dụng tỷ giá bán ra của Vietcombank tại ngày hôm nay để quy đổi.
- Về nguồn: trích nguồn lấy dữ liệu để truy vết lại dữ liệu sau này

Định dạng đầu ra:
Trả về dạng JSON array. Mỗi phần tử là một object với các field: Ky, DonViApDung, NhomViTri, ThamNien, MucLuongToiThieu, MucLuongToiDa, MucLuongBinhQuan, NguonTrichDan.
Tất cả các mức lương phải là số (number), đơn vị VND.
Tại field NguonTrichDan, value là tất cả các tên báo cáo đã được sử dụng để tính toán cho dòng đó.
";

        // ===== TẠO USER MESSAGE VỚI NỘI DUNG MARKDOWN =====
        var userMessage = $@"Dưới đây là các báo cáo lương thị trường được trích xuất từ các file markdown:

{markdownContent}

Dựa trên các báo cáo trên, hãy phân tích và thiết lập bảng mặt bằng lương thị trường.
Tỷ giá USD/VND = 25,000 (nếu cần quy đổi).";

        // ===== REQUEST WITH STRUCTURED OUTPUT =====
        var body = new
        {
            model = "misa-ai-1.0",
            temperature = 0.2,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "salary_benchmark_result",
                    strict = true,
                    schema = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                Ky = new { type = "string" },
                                DonViApDung = new { type = "string" },
                                NhomViTri = new { type = "string" },
                                ThamNien = new { type = "string" },
                                MucLuongToiThieu = new { type = "number" },
                                MucLuongToiDa = new { type = "number" },
                                MucLuongBinhQuan = new { type = "number" },
                                NguonTrichDan = new { type = "string" }
                            },
                            required = new[]
                            {
                                "Ky",
                                "DonViApDung",
                                "NhomViTri",
                                "ThamNien",
                                "MucLuongToiThieu",
                                "MucLuongToiDa",
                                "MucLuongBinhQuan",
                                "NguonTrichDan"
                            },
                            additionalProperties = false
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(body);

        Console.WriteLine("===== REQUEST =====");
        Console.WriteLine(json);
        Console.WriteLine();

        try
        {
            var response = await client.PostAsync(
                "http://test-k8s.misa.local/llm-gateway/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            Console.WriteLine("===== HTTP STATUS =====");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine();

            var raw = await response.Content.ReadAsStringAsync();

            Console.WriteLine("===== RAW RESPONSE =====");
            Console.WriteLine(string.IsNullOrWhiteSpace(raw) ? "(empty)" : raw);
            Console.WriteLine();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ API request failed!");
                return;
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                Console.WriteLine("❌ Empty response");
                return;
            }

            // Parse OpenRouter response
            var parsed = JsonSerializer.Deserialize<OpenRouterResponse>(raw, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var assistantContent = parsed?
                .Choices?
                .FirstOrDefault()?
                .Message?
                .Content;

            Console.WriteLine("===== ASSISTANT CONTENT (RAW) =====");
            Console.WriteLine(assistantContent ?? "(no content)");
            Console.WriteLine();

            // Parse the structured output
            if (!string.IsNullOrWhiteSpace(assistantContent))
            {
                var salaryData = JsonSerializer.Deserialize<SalaryBenchmark[]>(assistantContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Console.WriteLine("===== PARSED SALARY BENCHMARK =====");
                Console.WriteLine($"Tổng số dòng: {salaryData?.Length ?? 0}");
                
                if (salaryData != null)
                {
                    foreach (var item in salaryData)
                    {
                        Console.WriteLine($"\n- Kỳ: {item.Ky}");
                        Console.WriteLine($"  Đơn vị: {item.DonViApDung}");
                        Console.WriteLine($"  Vị trí: {item.NhomViTri}");
                        Console.WriteLine($"  Thâm niên: {item.ThamNien}");
                        Console.WriteLine($"  Lương: {item.MucLuongToiThieu:N0} - {item.MucLuongToiDa:N0} VND (TB: {item.MucLuongBinhQuan:N0})");
                        Console.WriteLine($"  Nguồn: {item.NguonTrichDan}");
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ HTTP Error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"❌ JSON Parse Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}