using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace App.Web.Infrastructure;

public enum ToastLevel { Success, Warning, Error, Info }

public static class TempDataToastExtensions
{
    private const string Key = "__Toasts__";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private record ToastItem(ToastLevel Level, string Message);

    public static void AddToast(this ITempDataDictionary tempData, ToastLevel level, string message)
    {
        var list = GetToasts(tempData);
        list.Add(new ToastItem(level, message));
        tempData[Key] = JsonSerializer.Serialize(list, JsonOpts);
    }

    public static void Success(this ITempDataDictionary t, string m) => t.AddToast(ToastLevel.Success, m);
    public static void Warning(this ITempDataDictionary t, string m) => t.AddToast(ToastLevel.Warning, m);
    public static void Error(this ITempDataDictionary t, string m)   => t.AddToast(ToastLevel.Error, m);
    public static void Info(this ITempDataDictionary t, string m)    => t.AddToast(ToastLevel.Info, m);

    private static List<ToastItem> GetToasts(ITempDataDictionary tempData)
    {
        if (tempData.TryGetValue(Key, out var raw) && raw is string json && !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var existing = JsonSerializer.Deserialize<List<ToastItem>>(json, JsonOpts);
                if (existing != null) return existing;
            }
            catch { /* that's all right */ }
        }
        return new List<ToastItem>();
    }
}