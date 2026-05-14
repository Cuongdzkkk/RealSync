# 🤖 AI Guide — RealSync

> Hướng dẫn tích hợp và sử dụng AI module trong hệ thống RealSync.

---

## 1. AI Module Overview

### Hai sub-module chính:

| Module | Mục đích | Tech |
|--------|----------|------|
| **AI Classification** | Phân loại BĐS, scoring, tagging | Python (FastAPI) |
| **Content AI Engine** | Sinh mô tả, SEO content | OpenAI API / Local LLM |

### Architecture
```
Backend (ASP.NET)
    │
    ├── HTTP Request ──→ AI Classification Service (Python FastAPI)
    │                         ├── Property Classifier
    │                         ├── Lead Scorer
    │                         └── Area Analyzer
    │
    └── HTTP Request ──→ Content AI Service (Node.js / Python)
                              ├── Description Generator
                              ├── SEO Content Generator
                              └── Social Post Generator
```

---

## 2. AI Classification

### 2.1 Property Classification

**Input:**
```json
{
  "title": "Bán nhà mặt tiền đường Nguyễn Huệ, Quận 1",
  "description": "Nhà 3 tầng, 4x20m, sổ hồng chính chủ...",
  "price": 15000000000,
  "area": 80,
  "address": "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM"
}
```

**Output:**
```json
{
  "propertyType": {
    "primary": "Nhà mặt tiền",
    "confidence": 0.95
  },
  "listing": {
    "type": "Sale",
    "confidence": 0.98
  },
  "location": {
    "province": "Hồ Chí Minh",
    "district": "Quận 1",
    "ward": "Phường Bến Nghé",
    "confidence": 0.92
  },
  "priceRange": "premium",
  "tags": ["mặt tiền", "sổ hồng", "kinh doanh"],
  "qualityScore": 85
}
```

### 2.2 Lead Scoring

**Input:**
```json
{
  "leadId": "guid",
  "interactions": [
    { "type": "viewed_property", "count": 5, "lastAt": "2026-05-10" },
    { "type": "inquiry", "count": 2, "lastAt": "2026-05-12" },
    { "type": "call", "count": 1, "lastAt": "2026-05-13" }
  ],
  "budget": 5000000000,
  "timeframe": "1_month",
  "source": "website"
}
```

**Output:**
```json
{
  "score": 78,
  "grade": "A",
  "priority": "High",
  "suggestedAction": "Gọi điện follow-up trong 24h",
  "matchedProperties": ["guid1", "guid2"]
}
```

---

## 3. Content AI Engine

### 3.1 Property Description Generator

**Prompt Template:**
```
Bạn là chuyên gia viết nội dung BĐS tại Việt Nam.
Hãy viết mô tả chuyên nghiệp cho BĐS sau:

Loại: {propertyType}
Vị trí: {address}
Diện tích: {area} m²
Giá: {price} VND
Phòng ngủ: {bedrooms}
Phòng tắm: {bathrooms}
Pháp lý: {legalStatus}
Đặc điểm: {features}

Yêu cầu:
- Viết bằng tiếng Việt, giọng chuyên nghiệp
- Độ dài: 150-300 từ
- Nêu bật ưu điểm vị trí, tiện ích
- Có call-to-action cuối bài
- Không phóng đại, giữ thông tin chính xác
```

### 3.2 SEO Content Generator

**Prompt Template:**
```
Tạo meta tags SEO cho BĐS:
Tiêu đề: {title}
Loại: {propertyType}
Khu vực: {district}, {province}

Output JSON:
{
  "metaTitle": "Tối đa 60 ký tự, chứa keyword chính",
  "metaDescription": "Tối đa 160 ký tự, hấp dẫn click",
  "slug": "url-friendly-slug",
  "keywords": ["keyword1", "keyword2"]
}
```

### 3.3 Social Post Generator

**Prompt Template:**
```
Tạo bài đăng Facebook/Zalo cho BĐS:
{propertyInfo}

Yêu cầu:
- Có emoji phù hợp
- Nêu thông tin chính (loại, vị trí, giá, diện tích)
- Có hashtag liên quan
- Call-to-action (liên hệ, inbox)
- Độ dài phù hợp social media (100-200 từ)
```

---

## 4. Integration Pattern

### Backend Integration
```csharp
// Interfaces
public interface IAiClassificationService
{
    Task<PropertyClassification> ClassifyPropertyAsync(PropertyClassifyRequest request);
    Task<LeadScore> ScoreLeadAsync(LeadScoreRequest request);
}

public interface IContentAiService
{
    Task<string> GenerateDescriptionAsync(PropertyDescriptionRequest request);
    Task<SeoContent> GenerateSeoContentAsync(SeoContentRequest request);
    Task<string> GenerateSocialPostAsync(SocialPostRequest request);
}

// Implementation (HTTP client)
public class AiClassificationService : IAiClassificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiClassificationService> _logger;
    
    public async Task<PropertyClassification> ClassifyPropertyAsync(PropertyClassifyRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/classify/property", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PropertyClassification>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI Classification failed for property");
            return PropertyClassification.Default(); // Fallback
        }
    }
}
```

### Background Processing (Hangfire)
```csharp
// Đăng ký job
public class PropertyCreatedHandler
{
    public void Handle(PropertyCreatedEvent @event)
    {
        // Queue AI classification job
        BackgroundJob.Enqueue<IAiClassificationService>(
            service => service.ClassifyPropertyAsync(new PropertyClassifyRequest
            {
                PropertyId = @event.PropertyId
            })
        );
        
        // Queue content generation job
        BackgroundJob.Enqueue<IContentAiService>(
            service => service.GenerateDescriptionAsync(new PropertyDescriptionRequest
            {
                PropertyId = @event.PropertyId
            })
        );
    }
}
```

---

## 5. AI Service API Endpoints

### Classification Service (Python FastAPI)
```
POST /api/classify/property     → Phân loại BĐS
POST /api/score/lead            → Scoring lead
POST /api/extract/features      → Extract features từ text
GET  /api/health                → Health check
```

### Content Service
```
POST /api/content/description   → Sinh mô tả
POST /api/content/seo           → Sinh SEO content
POST /api/content/social        → Sinh social post
GET  /api/content/health        → Health check
```

---

## 6. Rules & Best Practices

1. **Fallback**: Nếu AI service không available, hệ thống vẫn hoạt động bình thường.
2. **Timeout**: AI request timeout 30s, retry 2 lần.
3. **Rate Limit**: Max 100 requests/phút cho classification, 50 cho content.
4. **Cost Tracking**: Log token count và cost cho mỗi AI call.
5. **Human Review**: AI-generated content phải qua human review trước publish.
6. **Caching**: Cache kết quả classification 24h (cùng input → cùng output).
7. **Quality Check**: Confidence score < 70% → flag cho human review.
8. **Vietnamese NLP**: Dùng underthesea cho tokenization tiếng Việt.

---

> Xem thêm: `architecture-guide.md`, `crawler-guide.md`
