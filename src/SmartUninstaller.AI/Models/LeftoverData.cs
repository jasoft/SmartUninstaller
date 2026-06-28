using Microsoft.ML.Data;

namespace SmartUninstaller.AI.Models;

/// <summary>
/// 残留数据模型，用于ML.NET训练和预测
/// </summary>
public class LeftoverData
{
    /// <summary>文件路径长度</summary>
    [LoadColumn(0)]
    public float FilePathLength { get; set; }

    /// <summary>是否在常见目录中</summary>
    [LoadColumn(1)]
    public bool IsInCommonDirectory { get; set; }

    /// <summary>路径中是否包含软件名称</summary>
    [LoadColumn(2)]
    public bool HasSoftwareNameInPath { get; set; }

    /// <summary>文件扩展名类型</summary>
    [LoadColumn(3)]
    public float FileExtension { get; set; }

    /// <summary>文件大小</summary>
    [LoadColumn(4)]
    public float FileSize { get; set; }

    /// <summary>创建时间距今小时数</summary>
    [LoadColumn(5)]
    public float CreationTimeHours { get; set; }

    /// <summary>修改时间距今小时数</summary>
    [LoadColumn(6)]
    public float ModificationTimeHours { get; set; }

    /// <summary>是否为隐藏文件</summary>
    [LoadColumn(7)]
    public bool IsHidden { get; set; }

    /// <summary>是否为系统文件</summary>
    [LoadColumn(8)]
    public bool IsSystem { get; set; }

    /// <summary>是否为只读文件</summary>
    [LoadColumn(9)]
    public bool IsReadOnly { get; set; }

    /// <summary>是否为残留（标签列）</summary>
    [LoadColumn(10)]
    [ColumnName("Label")]
    public bool IsLeftover { get; set; }
}

/// <summary>
/// 残留预测结果模型
/// </summary>
public class LeftoverPrediction
{
    /// <summary>预测结果</summary>
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    /// <summary>概率</summary>
    public float Probability { get; set; }

    /// <summary>评分</summary>
    public float Score { get; set; }
}
