## 1. Mô tả chung

Mục tiêu của thí nghiệm là **đánh giá khả năng xử lý ngữ cảnh dài** của hai mô hình:

* **Gemini-3-Flash**
* **MISA-AI-1.0-Plus**
* **MISA-AI-1.0**
---

## 2. Thông tin tập dữ liệu

### Ngữ cảnh:
- Adecco.md
- Bao-cao-luong-IT_ITVIEC.md
- Manpower_Vietnam-Salary-Guide-2023.md
- Vietnam IT Salary Guide 2023.md

Tham khảo:

- 🤗 **Gemini-3-Flash Report Dataset**  
  https://huggingface.co/datasets/beyoru/Gemini-3-flash-preview-report

- 🤗 **MISA-AI-1.0-Plus Report Dataset**  
  https://huggingface.co/datasets/beyoru/misa-ai-plus-report

- 🤗 **MISA-AI-1.0 Report Dataset**  
  https://huggingface.co/datasets/beyoru/misa-ai-flash-report

Rubric: LLM-as-a-Judge for correctness only
---

## 3. So sánh kết quả

| Model           | Correct | Total | Accuracy   |
| --------------- | ------- | ----- | ---------- |
| Gemini-3-Flash  | 387     | 400   | **96.75%** |
| MISA-AI-1.0-Plus| 383     | 400   | **95.75%** |
| MISA-AI-1.0     | 354     | 400   | **88.50%** |

* **Gemini-3-Flash** đạt độ chính xác cao hơn **~1%** so với **MISA-AI-1.0-Plus**.
* Cả 3 mô hình đều thể hiện **hiệu năng rất tốt** trong bài toán xử lý ngữ cảnh dài.
* Sự chênh lệch không lớn, cho thấy **MISA-AI-1.0-Plus/MISA-AI-1.0** vẫn có thể sử dụng như là một AI tổng hợp báo cáo thay cho **Gemini-3-Flash**.

---

**Kết luận chung**:
Cả 3 mô hình đều đạt hiệu quả cao cho task, trong đó **Gemini-3-Flash cho kết quả tốt nhất về mặt độ chính xác** trên tập dữ liệu thử nghiệm này.


