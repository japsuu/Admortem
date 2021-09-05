namespace UnityEngine
{
    //[CreateAssetMenu(fileName = "New Tile Animation", menuName = "Tiles/Tile Animation")]
    [System.Serializable]
    public class TileAnimation
    {
        public float MinSpeed;
        public float MaxSpeed;

        public Sprite[] Frames;

        [HideInInspector] public bool expandedInInspector;
    }
}