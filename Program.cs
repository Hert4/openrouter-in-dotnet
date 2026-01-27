using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// ================= MODELS =================
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

// ================= PROGRAM =================
class Program
{
    static async Task Main()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("❌ Chưa có OPENROUTER_API_KEY");
            return;
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai");
        client.DefaultRequestHeaders.Add("X-Title", "CB Salary Benchmark Tool");

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
Trả về dạng list dictionary string object cho code backend c# xử lý. List có dạng theo từng dòng. Ở mỗi phần tử dạng dictionary {} với các key sau Kỳ, Đơn vị áp dụng, Nhóm vị trí, Thâm niên, Mức lương Tối thiểu, Mức lương Tối đa, Mức lương Bình quân, Nguồn trích dẫn và value là giá trị tương ứng
Tại key “Nguồn trích dẫn”, value là tất cả các tên báo cáo đã được sử dụng để tính toán cho dòng đó.
";

        // ===== REQUEST BODY =====
        var body = new
        {
            model = "meta-llama/llama-3.3-70b-instruct",
            temperature = 0.2,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new
                {
                    role = "tool",
                    content = "Lương lập trình viên backend 100$–200$ cho 1–2 năm, trên 2 năm gấp đôi."
                },
                new
                {
                    role = "user",
                    content = "Hãy thực hiện phân tích và trả về bảng mặt bằng lương theo đúng yêu cầu. Chỉ phản hồi text thuần không được trả về dạng JSON có cấu trúc"
                }
            },

            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "salary_benchmark_result",
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
                            }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(body);

        var response = await client.PostAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var raw = await response.Content.ReadAsStringAsync();

        // ===== PARSE & LOG ONLY ASSISTANT CONTENT =====
        var parsed = JsonSerializer.Deserialize<OpenRouterResponse>(raw);

        var assistantContent = parsed?
            .Choices?
            .FirstOrDefault()?
            .Message?
            .Content;

        Console.WriteLine("===== ASSISTANT CONTENT =====");
        Console.WriteLine(assistantContent);
    }
}
