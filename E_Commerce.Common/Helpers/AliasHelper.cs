using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace E_Commerce.Common.Helpers
{
    /// <summary>
    /// Helper để tạo URL-friendly alias từ tiếng Việt
    /// </summary>
    public static class AliasHelper
    {
        /// <summary>
        /// Chuyển đổi chuỗi tiếng Việt thành alias URL-friendly
        /// Ví dụ: "Áo thun nam" -> "ao-thun-nam"
        /// </summary>
        public static string GenerateAlias(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Chuyển về lowercase
            string alias = input.ToLowerInvariant().Trim();

            // Loại bỏ dấu tiếng Việt
            alias = RemoveVietnameseDiacritics(alias);

            // Thay thế khoảng trắng và ký tự đặc biệt bằng dấu gạch ngang
            alias = Regex.Replace(alias, @"[^a-z0-9]+", "-");

            // Loại bỏ nhiều dấu gạch ngang liên tiếp
            alias = Regex.Replace(alias, @"-+", "-");

            // Loại bỏ dấu gạch ngang ở đầu và cuối
            alias = alias.Trim('-');

            // Giới hạn độ dài (tùy chọn, ví dụ 255 ký tự)
            if (alias.Length > 255)
            {
                alias = alias.Substring(0, 255).TrimEnd('-');
            }

            return alias;
        }

        /// <summary>
        /// Loại bỏ dấu tiếng Việt
        /// </summary>
        private static string RemoveVietnameseDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Tạo alias unique bằng cách thêm số vào cuối nếu đã tồn tại
        /// </summary>
        public static string GenerateUniqueAlias(string baseAlias, Func<string, bool> existsChecker)
        {
            if (string.IsNullOrWhiteSpace(baseAlias))
                return string.Empty;

            string alias = GenerateAlias(baseAlias);
            string uniqueAlias = alias;
            int counter = 1;

            while (existsChecker(uniqueAlias))
            {
                uniqueAlias = $"{alias}-{counter}";
                counter++;
            }

            return uniqueAlias;
        }
    }
}

