using System;
using System.Linq;
using System.Collections.Generic;
using MiniFatFs;

class Program
{
    static void FormatFat(FatTableManager manager)
    {
        int[] freshFat = new int[FsConstants.FAT_ARRAY_SIZE];
        
        freshFat[FsConstants.SUPERBLOCK_CLUSTER] = FsConstants.FAT_ENTRY_EOF; 
        for (int i = FsConstants.FAT_START_CLUSTER; i <= FsConstants.FAT_END_CLUSTER; i++)
        {
            freshFat[i] = FsConstants.FAT_ENTRY_EOF; 
        }

        manager.WriteAllFat(freshFat);
        manager.FlushFatToDisk();
        Console.WriteLine("\nFAT formatted and flushed (Clusters 0-4 marked as EOF, others free).");
    }

    static void Main(string[] args)
    {
        string diskPath = "بطه بلدي.bin";
        var disk = new VirtualDisk();
        
        try
        {
            disk.Initialize(diskPath);
            
            var fatManager = new FatTableManager(disk);
            FormatFat(fatManager); 

            Console.WriteLine("\n--- Test 1: Allocate 3 Clusters ---");
            int required = 3;
            int startCluster = fatManager.AllocateChain(required);
            
            Console.WriteLine($"Allocated Chain Start: {startCluster}"); 
            List<int> chain = fatManager.FollowChain(startCluster);
            
            Console.WriteLine($"Chain: {string.Join(" -> ", chain)}");
            bool allocationSuccess = chain.Count == required && 
                                     chain[0] == 5 && 
                                     fatManager.GetFatEntry(5) == 6 &&
                                     fatManager.GetFatEntry(6) == 7 &&
                                     fatManager.GetFatEntry(7) == FsConstants.FAT_ENTRY_EOF;

            Console.WriteLine(allocationSuccess ? "Allocation and FollowChain SUCCESS." : "Allocation and FollowChain FAILED.");

            Console.WriteLine("\n--- Test 2: Free the Chain ---");
            fatManager.FreeChain(startCluster);
            
            bool freeSuccess = fatManager.GetFatEntry(5) == FsConstants.FAT_ENTRY_FREE && 
                               fatManager.GetFatEntry(6) == FsConstants.FAT_ENTRY_FREE &&
                               fatManager.GetFatEntry(7) == FsConstants.FAT_ENTRY_FREE;
            
            Console.WriteLine(freeSuccess ? "FreeChain SUCCESS." : "FreeChain FAILED.");

            fatManager.FlushFatToDisk();
            
            Console.WriteLine("\n--- Test 3: Reload and Verify Persistence ---");
            var freshFatManager = new FatTableManager(disk);
            
            bool persistenceSuccess = freshFatManager.GetFatEntry(5) == FsConstants.FAT_ENTRY_FREE;
            
            Console.WriteLine(persistenceSuccess 
                ? "Persistence Check SUCCESS (Cluster 5 is still free after reload)." 
                : "Persistence Check FAILED.");
            
            Console.WriteLine("\nTask 3 Implementation complete and tested.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred: {ex.Message}");
        }
        finally
        {
            // بطه بلدي
            disk.CloseDisk();
        }
    }
}