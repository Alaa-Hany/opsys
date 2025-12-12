using System;
using System.Linq;

namespace MiniFatFs
{
    public class SuperblockManager
    {
        private readonly VirtualDisk _disk;

        public SuperblockManager(VirtualDisk disk)
        {
            _disk = disk ?? throw new ArgumentNullException(nameof(disk));
        }

        public byte[] ReadSuperblock()
        {
            return _disk.ReadCluster(FsConstants.SUPERBLOCK_CLUSTER);
        }

        public void WriteSuperblock(byte[] data)
        {
            if (data.Length != FsConstants.CLUSTER_SIZE)
            {
                throw new ArgumentException($"Superblock data must be exactly {FsConstants.CLUSTER_SIZE} bytes.");
            }
            _disk.WriteCluster(FsConstants.SUPERBLOCK_CLUSTER, data);
        }
        
        public void FormatSuperblock()
        {
            byte[] zeroData = new byte[FsConstants.CLUSTER_SIZE]; 
            WriteSuperblock(zeroData);
            Console.WriteLine("Superblock (Cluster 0) formatted to all zeros.");
        }
    }
}