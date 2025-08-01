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
    }
}