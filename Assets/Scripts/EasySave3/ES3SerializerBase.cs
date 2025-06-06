using System;
using System.IO;
using UnityEngine;

/// <summary>
/// ES3 Save/Load/Delete 공통 로직을 제공하는 제네릭 베이스 클래스
/// </summary>
public abstract class ES3SerializerBase<T>
{
    /// <summary>
    /// Application.persistentDataPath 밑의 상대 폴더 경로. 반드시 오버라이드하세요.
    /// </summary>
    protected abstract string RelativePath { get; }

    /// <summary>
    /// 저장할 파일 이름(.es3 포함)
    /// </summary>
    protected abstract string FileName { get; }

    /// <summary>
    /// ES3 키(key)
    /// </summary>
    protected abstract string Key { get; }

    private readonly string _basePath;
    private readonly ES3Settings _settings;
    private readonly ILogger _logger;

    /// <summary>
    /// DI 생성자: basePath, settings, logger는 필요시 교체 가능합니다.
    /// </summary>
    protected ES3SerializerBase(
        string basePath = null,
        ES3Settings settings = null,
        ILogger logger = null)
    {
        _basePath = basePath ?? Application.persistentDataPath;
        _settings = settings ?? new ES3Settings
        {
            location = ES3.Location.File,
            prettyPrint = true
        };
        _logger = logger ?? Debug.unityLogger;
    }

    /// <summary>
    /// 영구 저장 경로 + 상대폴더 + 파일명
    /// </summary>
    private string FilePath =>
        Path.Combine(_basePath, RelativePath, FileName);

    /// <summary>
    /// 파일 쓰기 전, 폴더가 없으면 생성
    /// </summary>
    private void EnsureDirectory()
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    /// <summary>
    /// 데이터 저장 (성공 여부 리턴)
    /// </summary>
    public bool Save(T data)
    {
        EnsureDirectory();
        try
        {
            ES3.Save(Key, data, FilePath, _settings);
            _logger.Log(LogType.Log, $"[ES3] Saved {typeof(T).Name} → {FilePath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogType.Error,
                $"[ES3] Failed to save {typeof(T).Name} to {FilePath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 데이터 로드 (실패 시 default(T) 리턴)
    /// </summary>
    public T Load()
    {
        EnsureDirectory();
        try
        {
            if (!ES3.FileExists(FilePath, _settings))
            {
                _logger.Log(LogType.Warning,
                    $"[ES3] File not found: {FilePath}");
                return default;
            }
            var data = ES3.Load<T>(Key, FilePath, _settings);
            _logger.Log(LogType.Log,
                $"[ES3] Loaded {typeof(T).Name} ← {FilePath}");
            return data;
        }
        catch (Exception ex)
        {
            _logger.Log(LogType.Error,
                $"[ES3] Failed to load {typeof(T).Name} from {FilePath}: {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// 파일 삭제 (존재하면 삭제 후 true, 없으면 false)
    /// </summary>
    public bool Delete()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                _logger.Log(LogType.Log,
                    $"[ES3] Deleted {typeof(T).Name} → {FilePath}");
                return true;
            }
            _logger.Log(LogType.Log,
                $"[ES3] No {typeof(T).Name} found at {FilePath}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Log(LogType.Error,
                $"[ES3] Failed to delete {typeof(T).Name}: {ex.Message}");
            return false;
        }
    }
}