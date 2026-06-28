namespace SmartUninstaller.Shared.Extensions;

/// <summary>
/// 字符串扩展方法
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 截断字符串到指定长度，超出部分用省略号替代
    /// </summary>
    /// <param name="value">原始字符串</param>
    /// <param name="maxLength">最大长度</param>
    /// <returns>截断后的字符串</returns>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength), "...");
    }

    /// <summary>
    /// 对字符串进行掩码处理
    /// </summary>
    /// <param name="value">原始字符串</param>
    /// <param name="visibleChars">可见字符数</param>
    /// <returns>掩码后的字符串</returns>
    public static string Mask(this string value, int visibleChars = 3)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= visibleChars) return value;
        return string.Concat(value.AsSpan(0, visibleChars), new string('*', value.Length - visibleChars));
    }

    /// <summary>
    /// 判断字符串是否为空或空白
    /// </summary>
    /// <param name="value">字符串值</param>
    /// <returns>是否为空或空白</returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}
