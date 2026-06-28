using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartUninstaller.UI.Converters;

/// <summary>
/// 布尔值到可见性转换器
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// 将布尔值转换为可见性
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // 如果参数为"Inverse"，则反转逻辑
            if (parameter?.ToString() == "Inverse")
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    /// <summary>
    /// 将可见性转换为布尔值
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            var result = visibility == Visibility.Visible;
            if (parameter?.ToString() == "Inverse")
                return !result;
            return result;
        }
        return false;
    }
}

/// <summary>
/// 文件大小格式化转换器
/// </summary>
public class FileSizeConverter : IValueConverter
{
    /// <summary>
    /// 将字节数转换为可读的文件大小字符串
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            string[] suffixes = new[] { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {suffixes[order]}";
        }
        return "0 B";
    }

    /// <summary>
    /// 反向转换（不支持）
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// 风险等级到颜色转换器
/// </summary>
public class RiskLevelToColorConverter : IValueConverter
{
    /// <summary>
    /// 将风险等级转换为颜色字符串
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Core.Models.RiskLevel risk)
        {
            return risk switch
            {
                Core.Models.RiskLevel.Low => "#4CAF50",
                Core.Models.RiskLevel.Medium => "#FF9800",
                Core.Models.RiskLevel.High => "#F44336",
                Core.Models.RiskLevel.Critical => "#9C27B0",
                _ => "#757575"
            };
        }
        return "#757575";
    }

    /// <summary>
    /// 反向转换（不支持）
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
