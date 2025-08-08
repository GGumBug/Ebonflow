using System;

public class DataContext<T>
{
    protected readonly string fileName;
    protected T data;
    protected readonly ES3SerializerBase<T> dataSaveLoad;
    private readonly Func<T> defaultFactory;

    public DataContext(string fileName, ES3SerializerBase<T> serializer, Func<T> defaultFactory)
    {
        this.fileName = fileName;
        dataSaveLoad = serializer;
        this.defaultFactory = defaultFactory;
        Load();
    }

    /// <summary>파일에서 로드, 없으면 팩토리로 기본값 생성</summary>
    public virtual void Load()
    {
        data = dataSaveLoad.Load(fileName);
        if (data == null)
            data = defaultFactory();
    }

    /// <summary>현재 데이터 저장</summary>
    public void Save()
    {
        dataSaveLoad.Save(data, fileName);
    }

    /// <summary>기본값 또는 전달된 팩토리로 리셋 + 저장</summary>
    public void Reset(Func<T> factory = null)
    {
        data = (factory ?? defaultFactory)();
        Save();
    }

    /// <summary>내부 데이터 직접 접근</summary>
    public T Data => data;
}
