using UnityEngine;

public class PlayerSaveLoad : ES3SerializerBase<PlayerSaveData>
{
    protected override string RelativePath => "Player";

    public PlayerSaveLoad(
            string basePath = null,
            ES3Settings settings = null,
            ILogger logger = null)
            : base(basePath, settings, logger)
        { }
}
