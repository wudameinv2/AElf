using AElf.Types;

namespace AElf.OS.BlockSync.Dto
{
    public class SyncBlockDto
    {
        public Hash SyncBlockHash { get; set; }

        public long SyncBlockHeight { get; set; }

        public string SuggestedPeerPubKey { get; set; }
        
        public int BatchRequestBlockCount { get; set; }
    }
}