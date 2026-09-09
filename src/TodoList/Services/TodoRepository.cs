using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using TodoList.Models;

namespace TodoList.Services;

/// <summary>
/// 基于 JSON 文件的 Todo 持久化仓库（线程安全）。
/// </summary>
public class TodoRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 中文原样保存，不转义
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;
    private readonly object _syncRoot = new();

    public string FilePath => _filePath;

    public TodoRepository(string filePath)
    {
        _filePath = filePath;
    }

    public List<TodoItem> Load()
    {
        lock (_syncRoot)
        {
            if (!File.Exists(_filePath))
                return new List<TodoItem>();

            try
            {
                var json = File.ReadAllText(_filePath, Encoding.UTF8);
                return JsonSerializer.Deserialize<List<TodoItem>>(json, SerializerOptions)
                       ?? new List<TodoItem>();
            }
            catch (JsonException)
            {
                // JSON 损坏时先备份原文件，再从空列表开始，避免下次保存覆盖丢失
                try
                {
                    File.Copy(_filePath, _filePath + ".corrupted.bak", overwrite: true);
                }
                catch
                {
                    // 备份失败不阻塞启动
                }

                return new List<TodoItem>();
            }
        }
    }

    public void Save(IReadOnlyList<TodoItem> items)
    {
        lock (_syncRoot)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(items, SerializerOptions);
            File.WriteAllText(_filePath, json, Encoding.UTF8);
        }
    }
}
