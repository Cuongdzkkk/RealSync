# 🤖 AI Classification Prompt Template

> Dùng prompt này khi cần agent tạo hoặc tích hợp AI classification.

---

## Prompt Template

```
[AI] Tạo/Tích hợp AI Classification cho "{TaskName}"

Yêu cầu:
1. Input schema (dữ liệu đầu vào)
2. Output schema (kết quả phân loại)
3. Classification logic (rule-based hoặc ML)
4. API endpoint (FastAPI / Express)
5. Integration với Backend (HTTP client + Hangfire job)
6. Fallback khi AI service down
7. Caching strategy
8. Error handling + logging

Tuân thủ AI Rules trong SKILL.md
```

---

## Ví dụ: Property Auto-Classification

```
[AI] Tạo service phân loại BĐS tự động

Khi BĐS mới được tạo hoặc crawl về, tự động:
1. Phân loại loại BĐS (nhà, đất, căn hộ...)
2. Trích xuất thông tin vị trí
3. Phân loại phân khúc giá
4. Tạo tags tự động
5. Tính quality score

Input:
{
  "title": "string",
  "description": "string",
  "price": number,
  "area": number,
  "address": "string"
}

Expected Output:
{
  "propertyType": { "value": "string", "confidence": number },
  "location": { "province": "string", "district": "string" },
  "priceSegment": "budget | mid | premium | luxury",
  "tags": ["string"],
  "qualityScore": number (0-100)
}
```

---

## Python FastAPI Example

```python
# ai/classification/main.py
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import re
from typing import Optional

app = FastAPI(title="RealSync AI Classification")

class PropertyClassifyRequest(BaseModel):
    title: str
    description: Optional[str] = ""
    price: float
    area: float
    address: Optional[str] = ""

class ClassificationResult(BaseModel):
    property_type: dict
    location: dict
    price_segment: str
    tags: list[str]
    quality_score: int

PROPERTY_TYPE_KEYWORDS = {
    "Nhà mặt tiền": ["mặt tiền", "mặt phố", "nhà phố"],
    "Nhà hẻm": ["hẻm", "kiệt", "ngõ"],
    "Căn hộ": ["căn hộ", "chung cư", "apartment"],
    "Đất nền": ["đất nền", "đất thổ cư", "lô đất"],
    "Biệt thự": ["biệt thự", "villa"],
    "Văn phòng": ["văn phòng", "office"],
}

PRICE_SEGMENTS = {
    "budget": (0, 2_000_000_000),
    "mid": (2_000_000_000, 5_000_000_000),
    "premium": (5_000_000_000, 15_000_000_000),
    "luxury": (15_000_000_000, float("inf")),
}

@app.post("/api/classify/property", response_model=ClassificationResult)
async def classify_property(request: PropertyClassifyRequest):
    text = f"{request.title} {request.description}".lower()

    # 1. Classify property type
    property_type = classify_type(text)

    # 2. Extract location
    location = extract_location(request.address or request.title)

    # 3. Price segment
    price_segment = get_price_segment(request.price)

    # 4. Auto tags
    tags = extract_tags(text)

    # 5. Quality score
    quality_score = calculate_quality_score(request)

    return ClassificationResult(
        property_type=property_type,
        location=location,
        price_segment=price_segment,
        tags=tags,
        quality_score=quality_score,
    )

def classify_type(text: str) -> dict:
    for ptype, keywords in PROPERTY_TYPE_KEYWORDS.items():
        for kw in keywords:
            if kw in text:
                return {"value": ptype, "confidence": 0.85}
    return {"value": "Khác", "confidence": 0.5}

def get_price_segment(price: float) -> str:
    for segment, (low, high) in PRICE_SEGMENTS.items():
        if low <= price < high:
            return segment
    return "unknown"

def extract_location(address: str) -> dict:
    # Simplified - production should use NLP
    provinces = ["Hồ Chí Minh", "Hà Nội", "Đà Nẵng", "Bình Dương"]
    result = {"province": "", "district": ""}
    for p in provinces:
        if p.lower() in address.lower():
            result["province"] = p
            break
    return result

def extract_tags(text: str) -> list[str]:
    tag_keywords = {
        "sổ hồng": "sổ hồng",
        "sổ đỏ": "sổ đỏ",
        "chính chủ": "chính chủ",
        "mặt tiền": "mặt tiền",
        "kinh doanh": "kinh doanh",
        "gần trường": "gần trường",
        "gần chợ": "gần chợ",
    }
    return [tag for kw, tag in tag_keywords.items() if kw in text]

def calculate_quality_score(req: PropertyClassifyRequest) -> int:
    score = 50
    if req.title and len(req.title) > 20: score += 10
    if req.description and len(req.description) > 100: score += 15
    if req.price > 0: score += 10
    if req.area > 0: score += 10
    if req.address: score += 5
    return min(score, 100)

@app.get("/api/health")
async def health():
    return {"status": "ok", "service": "ai-classification"}
```

---

## Backend Integration Example

```csharp
// RealSync.Services/Implementations/AiClassificationService.cs
public class AiClassificationService : IAiClassificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiClassificationService> _logger;
    private readonly ICacheService _cacheService;

    public async Task<PropertyClassification?> ClassifyPropertyAsync(
        PropertyClassifyRequest request)
    {
        // Check cache
        var cacheKey = $"ai:classify:{request.GetHashCode()}";
        var cached = await _cacheService.GetAsync<PropertyClassification>(cacheKey);
        if (cached != null) return cached;

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/classify/property", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI Classification returned {StatusCode}", 
                    response.StatusCode);
                return null; // Fallback: return null, let caller handle
            }

            var result = await response.Content
                .ReadFromJsonAsync<PropertyClassification>();

            // Cache for 24h
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(24));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI Classification service error");
            return null; // Graceful fallback
        }
    }
}
```
