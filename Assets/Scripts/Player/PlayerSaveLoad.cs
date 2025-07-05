using UnityEngine;

public class PlayerSaveLoad : ES3SerializerBase<PlayerSaveData>
{
    protected override string RelativePath => "Player";

}
