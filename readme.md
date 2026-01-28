
# Demo OpenRouter + .NET

## Giới thiệu

Dự án này minh họa cách sử dụng **OpenRouter Chat Completions API** trong ứng dụng **.NET (C#)** để:

* Gửi prompt phân tích lương (C&B / Salary Benchmark)
* Ép model trả về dữ liệu theo **JSON Schema**
* **Chỉ lấy và log nội dung `assistant.content`**
* Phù hợp cho backend xử lý tiếp (parse JSON, lưu DB, export Excel, …)

Không log metadata dư thừa như `usage`, `cost`, `id`. 


---

## Yêu cầu trước khi chạy

### Tạo API Key OpenRouter

Đăng ký và tạo API key tại OpenRouter nếu dùng.

### Set biến môi trường

#### macOS / Linux

> set any api, example using openrouter

```bash
export OPENROUTER_API_KEY="your_api_key_here"
```

#### Windows (PowerShell)

```powershell
setx OPENROUTER_API_KEY "your_api_key_here"
```

---

## Usage

### Run code

```shell
dotnet run
```

---

## 📤 Expected Result

Chương trình **chỉ log ra nội dung phản hồi của assistant**, đúng format JSON đã ép:

```shell
===== ASSISTANT CONTENT =====
[
  {
    "DonViApDung": "Khối sản xuất",
    "Ky": "10/2025",
    "MucLuongBinhQuan": 15000000,
    "MucLuongToiDa": 30000000,
    "MucLuongToiThieu": 8000000,
    "NguonTrichDan": "Báo cáo lương thị trường 1, Báo cáo lương thị trường 2",
    "NhomViTri": "Lập trình viên",
    "ThamNien": "1-3 năm"
  },
  {
    "DonViApDung": "Khối sản xuất",
    "Ky": "10/2025",
    "MucLuongBinhQuan": 20000000,
    "MucLuongToiDa": 40000000,
    "MucLuongToiThieu": 10000000,
    "NguonTrichDan": "Báo cáo lương thị trường 3, Báo cáo lương thị trường 4",
    "NhomViTri": "Lập trình viên",
    "ThamNien": "3-5 năm"
  },
  {
    "DonViApDung": "Khối sản xuất",
    "Ky": "10/2025",
    "MucLuongBinhQuan": 25000000,
    "MucLuongToiDa": 50000000,
    "MucLuongToiThieu": 12000000,
    "NguonTrichDan": "Báo cáo lương thị trường 5, Báo cáo lương thị trường 6",
    "NhomViTri": "Lập trình viên",
    "ThamNien": "5-8 năm"
  },
  {
    "DonViApDung": "Khối sản xuất",
    "Ky": "10/2025",
    "MucLuongBinhQuan": 30000000,
    "MucLuongToiDa": 60000000,
    "MucLuongToiThieu": 15000000,
    "NguonTrichDan": "Báo cáo lương thị trường 7, Báo cáo lương thị trường 8",
    "NhomViTri": "Lập trình viên",
    "ThamNien": "trên 8 năm"
  }
]
```

---

## Cách hoạt động

1. Gửi request tới openrouter/local-test-api `/chat/completions`
2. Ép response theo `json_schema`
3. Parse response:

   ```
   choices[0].message.content
   ```
4. Log ra **duy nhất JSON kết quả**


---


