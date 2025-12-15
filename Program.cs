using System;
using System.Linq;
using System.Collections.Generic;
using MiniFatFs;

class Program
{


    static void FormatFat(FatTableManager manager)
    {
        int[] freshFat = new int[FsConstants.FAT_ARRAY_SIZE];

        // 1. Mark all as free initially
        for (int i = 0; i < FsConstants.FAT_ARRAY_SIZE; i++)
            freshFat[i] = FsConstants.FAT_ENTRY_FREE;

        // 2. Mark reserved clusters as EOF
        freshFat[FsConstants.SUPERBLOCK_CLUSTER] = FsConstants.FAT_ENTRY_EOF;
        for (int i = FsConstants.FAT_START_CLUSTER; i <= FsConstants.FAT_END_CLUSTER; i++)
            freshFat[i] = FsConstants.FAT_ENTRY_EOF;

        // 3. Mark Root Directory Cluster as allocated
        freshFat[FsConstants.ROOT_DIR_FIRST_CLUSTER] = FsConstants.FAT_ENTRY_EOF;

        manager.WriteAllFat(freshFat);
        manager.FlushFatToDisk();

        Console.WriteLine("\nFAT formatted successfully.");
    }


    static void RunTask4()
    {
        string diskPath = "fatty.bin";
        var disk = new VirtualDisk();

        try
        {
            disk.Initialize(diskPath);

            var fatManager = new FatTableManager(disk);
            FormatFat(fatManager);

            var directoryManager = new DirectoryManager(disk, fatManager);

            Console.WriteLine("\n--- Test 4.1: Standard Entry Management ---");

            int fileStartCluster = fatManager.AllocateChain(1);
            int fileSize = FsConstants.CLUSTER_SIZE;

            var newEntry = new DirectoryEntry("TestFile.txt", 0x20, fileStartCluster, fileSize);

            directoryManager.AddDirectoryEntry(
                FsConstants.ROOT_DIR_FIRST_CLUSTER,
                newEntry
            );

            var foundEntry = directoryManager.FindDirectoryEntry(
                FsConstants.ROOT_DIR_FIRST_CLUSTER,
                "testFILE.TXT"
            );

            if (foundEntry != null)
                Console.WriteLine("\tFindDirectoryEntry SUCCESS");
            else
                Console.WriteLine("\tFindDirectoryEntry FAILED");

            Console.WriteLine("\n--- Test 4.2: Removal Test ---");

            directoryManager.RemoveDirectoryEntry(
                FsConstants.ROOT_DIR_FIRST_CLUSTER,
                foundEntry
            );

            int fatValue = fatManager.GetFatEntry(fileStartCluster);
            Console.WriteLine(
                fatValue == FsConstants.FAT_ENTRY_FREE
                ? "\tCluster freed successfully"
                : "\tCluster NOT freed"
            );

            Console.WriteLine("\n--- Test 4.3: Directory Extension Test ---");

            for (int i = 0; i <= FsConstants.MAX_ENTRIES_PER_CLUSTER; i++)
            {
                var dummy = new DirectoryEntry(
                    $"FILE{i:D2}.TXT",
                    0x20,
                    FsConstants.CONTENT_START_CLUSTER + 1,
                    100
                );

                directoryManager.AddDirectoryEntry(
                    FsConstants.ROOT_DIR_FIRST_CLUSTER,
                    dummy
                );
            }

            Console.WriteLine("\nTask 4 finished successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Task 4 Error: " + ex.Message);
        }
        finally
        {
            disk.CloseDisk();
        }
    }


    static void RunTask6()
    {
        Shell shell = new Shell();
        shell.Run();
    }



    static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\n====== Mini FAT OS ======");
            Console.WriteLine("1 - Run Task 4 (FAT & Directory)");
            Console.WriteLine("2 - Run Task 6 (Shell)");
            Console.WriteLine("0 - Exit");
            Console.Write("Choose: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    RunTask4();
                    break;

                case "2":
                    RunTask6();
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
    }
}
