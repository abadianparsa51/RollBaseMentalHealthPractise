using System;
using System.Collections.Generic;

namespace Application.Diagnostics.DTOs
{
    public class MlDataExportDto
    {
        public Guid UserId { get; set; }
        public Guid SessionId { get; set; }

        // پاسخ‌های کاربر (مثلاً Q1: "1", Q2: "0", ...)
        public Dictionary<string, string> Answers { get; set; } = new();

        // نتیجه‌ی نهایی تشخیص (Label)
        public string Diagnosis { get; set; } = string.Empty;
    }
}
