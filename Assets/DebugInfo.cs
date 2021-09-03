using UnityEngine;
using UnityEngine.UI;

public class DebugInfo : MonoBehaviour
{
    private static readonly string[] DIRECTIONS = new string[] { "North (+Z)", "East (+X)", "South (-Z)", "West (-X)", };

    Text text;

    public Player player;

    private void Start()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {

        int dir = (int)Mathf.Round(player.transform.localEulerAngles.y / 90) % 4;
        text.text = string.Format("X:{0:0.##}\nY:{1:0.##}\nZ:{2:0.##}\nFacing:{3}\nChunks loaded: {4}\nPooled chunks remaining: {5}\nLast chunk update time: {6:0.##}s",
            player.transform.position.x, player.transform.position.y, player.transform.position.z,
            DIRECTIONS[dir],
            World.instance.NumLoadedChunks,
            World.instance.ChunkPoolCount,
            World.instance.lastChunkUpdateTime);

    }
}
