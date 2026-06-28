using Microsoft.ML;
using Microsoft.ML.Data;
using SmartUninstaller.Core.Models;
using SmartUninstaller.AI.Models;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.AI.Services;

/// <summary>
/// AI残留识别服务，使用ML.NET进行残留文件预测
/// </summary>
public class AILeftoverDetectionService
{
    private readonly ILogger<AILeftoverDetectionService> _logger;
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<LeftoverData, LeftoverPrediction>? _predictionEngine;

    public AILeftoverDetectionService(ILogger<AILeftoverDetectionService> logger)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 0);
    }

    /// <summary>
    /// 训练AI模型
    /// </summary>
    /// <param name="trainingData">训练数据</param>
    public async Task TrainModelAsync(IEnumerable<LeftoverData> trainingData)
    {
        try
        {
            _logger.LogInformation("开始训练AI模型...");

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(LeftoverData.FilePathLength),
                    nameof(LeftoverData.IsInCommonDirectory),
                    nameof(LeftoverData.HasSoftwareNameInPath),
                    nameof(LeftoverData.FileExtension),
                    nameof(LeftoverData.FileSize),
                    nameof(LeftoverData.CreationTimeHours),
                    nameof(LeftoverData.ModificationTimeHours),
                    nameof(LeftoverData.IsHidden),
                    nameof(LeftoverData.IsSystem),
                    nameof(LeftoverData.IsReadOnly))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: nameof(LeftoverData.IsLeftover),
                    featureColumnName: "Features"));

            _model = pipeline.Fit(dataView);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<LeftoverData, LeftoverPrediction>(_model);

            _logger.LogInformation("AI模型训练完成");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI模型训练失败");
            throw;
        }
    }

    /// <summary>
    /// 预测单个残留
    /// </summary>
    /// <param name="data">残留数据</param>
    /// <returns>预测结果</returns>
    public LeftoverPrediction PredictLeftover(LeftoverData data)
    {
        if (_predictionEngine == null)
            throw new InvalidOperationException("模型尚未训练");

        return _predictionEngine.Predict(data);
    }

    /// <summary>
    /// 分析残留列表
    /// </summary>
    /// <param name="software">关联软件</param>
    /// <param name="leftovers">残留列表</param>
    /// <returns>分析后的残留列表</returns>
    public async Task<IEnumerable<LeftoverInfo>> AnalyzeLeftoversAsync(
        SoftwareInfo software, IEnumerable<LeftoverInfo> leftovers)
    {
        var analyzedLeftovers = new List<LeftoverInfo>();

        foreach (var leftover in leftovers)
        {
            var data = ConvertToLeftoverData(software, leftover);
            var prediction = PredictLeftover(data);

            leftover.ConfidenceScore = prediction.Probability;
            leftover.IsSafeToDelete = prediction.Prediction && prediction.Probability > 0.8;
            leftover.RiskLevel = CalculateRiskLevel(prediction.Probability);
            leftover.RiskDescription = GetRiskDescription(leftover.RiskLevel);

            analyzedLeftovers.Add(leftover);
        }

        return await Task.FromResult(analyzedLeftovers);
    }

    /// <summary>
    /// 保存模型到文件
    /// </summary>
    /// <param name="modelPath">模型保存路径</param>
    public async Task SaveModelAsync(string modelPath)
    {
        if (_model == null) throw new InvalidOperationException("模型尚未训练");
        _mlContext.Model.Save(_model, null, modelPath);
        _logger.LogInformation("模型已保存到: {Path}", modelPath);
        await Task.CompletedTask;
    }

    /// <summary>
    /// 从文件加载模型
    /// </summary>
    /// <param name="modelPath">模型文件路径</param>
    public async Task LoadModelAsync(string modelPath)
    {
        if (!File.Exists(modelPath))
            throw new FileNotFoundException("模型文件不存在", modelPath);

        _model = _mlContext.Model.Load(modelPath, out _);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<LeftoverData, LeftoverPrediction>(_model);
        _logger.LogInformation("模型已加载: {Path}", modelPath);
        await Task.CompletedTask;
    }

    private LeftoverData ConvertToLeftoverData(SoftwareInfo software, LeftoverInfo leftover)
    {
        return new LeftoverData
        {
            FilePathLength = leftover.Path?.Length ?? 0,
            IsInCommonDirectory = IsInCommonDirectory(leftover.Path),
            HasSoftwareNameInPath = leftover.Path?.Contains(software.Name, StringComparison.OrdinalIgnoreCase) == true,
            FileExtension = GetFileExtension(leftover.Path),
            FileSize = leftover.Size,
            CreationTimeHours = leftover.CreatedTime.HasValue
                ? (float)(DateTime.Now - leftover.CreatedTime.Value).TotalHours : 0,
            ModificationTimeHours = leftover.ModifiedTime.HasValue
                ? (float)(DateTime.Now - leftover.ModifiedTime.Value).TotalHours : 0,
            IsHidden = false,
            IsSystem = false,
            IsReadOnly = false,
            IsLeftover = false
        };
    }

    private static bool IsInCommonDirectory(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        string[] commonDirs =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"C:\Windows\Temp"
        ];
        return commonDirs.Any(dir => path.StartsWith(dir, StringComparison.OrdinalIgnoreCase));
    }

    private static float GetFileExtension(string? path)
    {
        if (string.IsNullOrEmpty(path)) return 0;
        return Path.GetExtension(path)?.ToLower() switch
        {
            ".exe" => 1, ".dll" => 2, ".sys" => 3, ".dat" => 4,
            ".log" => 5, ".tmp" => 6, ".bak" => 7, ".old" => 8,
            ".ini" => 9, ".xml" => 10, ".json" => 11, _ => 0
        };
    }

    private static RiskLevel CalculateRiskLevel(float probability)
    {
        return probability switch
        {
            >= 0.9f => RiskLevel.Low,
            >= 0.7f => RiskLevel.Medium,
            >= 0.5f => RiskLevel.High,
            _ => RiskLevel.Critical
        };
    }

    private static string GetRiskDescription(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.Low => "低风险：可以安全删除",
            RiskLevel.Medium => "中等风险：建议谨慎删除",
            RiskLevel.High => "高风险：删除前请确认",
            RiskLevel.Critical => "严重风险：不建议删除",
            _ => "未知风险"
        };
    }
}
