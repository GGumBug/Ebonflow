using System.IO;
using UnityEngine;

namespace StageEditor
{
    public class StageSaveLoad : ES3SerializerBase<StageData>
    {
        protected override string RelativePath => "Stage";

        public StageSaveLoad(
            string basePath = null,
            ES3Settings settings = null,
            ILogger logger = null)
            : base(basePath, settings, logger)
        { }

        public int GetFileCount()
        {
            // 1) 폴더 경로 계산
            var folderPath = Path.Combine(basePath, RelativePath);

            // 2) 폴더가 없으면 0 리턴
            if (!Directory.Exists(folderPath))
                return 0;

            // 3) 폴더 내 모든 파일 조회 후 개수 반환
            //    필요하다면 "*.json" 등의 패턴을 줄 수도 있습니다.
            var files = Directory.GetFiles(folderPath);
            return files.Length;
        }
    }
}