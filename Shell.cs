using System;
using System.IO;

namespace OPSYS
{
    class Shell
    {
        private DirectoryInfo currentDir;

        public Shell()
        {
            currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        }

        public void Run()
        {
            while (true)
            {
                Console.Write($"{currentDir.FullName}> ");
                string input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                    continue;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLower();

                try
                {
                    switch (command)
                    {
                        case "cd":
                            Cd(parts);
                            break;

                        case "dir":
                            Dir();
                            break;

                        case "md":
                            Md(parts);
                            break;

                        case "rd":
                            Rd(parts);
                            break;

                        case "copy":
                            Copy(parts);
                            break;

                        case "del":
                            Del(parts);
                            break;

                        case "type":
                            Type(parts);
                            break;

                        case "help":
                            Help();
                            break;

                        case "exit":
                            return;

                        default:
                            Console.WriteLine("Invalid command. Type 'help' for commands.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        /* ================= Commands ================= */

        private void Cd(string[] parts)
        {
            // cd
            if (parts.Length == 1)
            {
                Console.WriteLine(currentDir.FullName);
                return;
            }

            // cd ..
            if (parts[1] == "..")
            {
                if (currentDir.Parent != null)
                    currentDir = currentDir.Parent;
                return;
            }

            // cd folder
            string path = Path.Combine(currentDir.FullName, parts[1]);
            if (Directory.Exists(path))
            {
                currentDir = new DirectoryInfo(path);
            }
            else
            {
                Console.WriteLine("The system cannot find the path specified.");
            }
        }

        private void Dir()
        {
            foreach (var dir in currentDir.GetDirectories())
                Console.WriteLine("<DIR> " + dir.Name);

            foreach (var file in currentDir.GetFiles())
                Console.WriteLine("      " + file.Name);
        }

        private void Md(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: md <directory>");
                return;
            }

            string path = Path.Combine(currentDir.FullName, parts[1]);
            Directory.CreateDirectory(path);
            Console.WriteLine("Directory created successfully.");
        }

        private void Rd(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: rd <directory>");
                return;
            }

            string path = Path.Combine(currentDir.FullName, parts[1]);

            if (!Directory.Exists(path))
            {
                Console.WriteLine("The system cannot find the directory specified.");
                return;
            }

            if (Directory.GetFileSystemEntries(path).Length > 0)
            {
                Console.WriteLine("The directory is not empty.");
                return;
            }

            Directory.Delete(path);
            Console.WriteLine("Directory removed successfully.");
        }

        private void Copy(string[] parts)
        {
            if (parts.Length < 3)
            {
                Console.WriteLine("Usage: copy <source> <destination>");
                return;
            }

            string src = Path.Combine(currentDir.FullName, parts[1]);
            string dest = Path.Combine(currentDir.FullName, parts[2]);

            if (!File.Exists(src))
            {
                Console.WriteLine("The system cannot find the file specified.");
                return;
            }

            File.Copy(src, dest, true);
            Console.WriteLine("File copied successfully.");
        }

        private void Del(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: del <file>");
                return;
            }

            string path = Path.Combine(currentDir.FullName, parts[1]);

            if (!File.Exists(path))
            {
                Console.WriteLine("The system cannot find the file specified.");
                return;
            }

            File.Delete(path);
            Console.WriteLine("File deleted successfully.");
        }

        private void Type(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: type <file>");
                return;
            }

            string path = Path.Combine(currentDir.FullName, parts[1]);

            if (!File.Exists(path))
            {
                Console.WriteLine("The system cannot find the file specified.");
                return;
            }

            foreach (string line in File.ReadAllLines(path))
                Console.WriteLine(line);
        }

        private void Help()
        {
            Console.WriteLine("Supported commands:");
            Console.WriteLine("cd <dir>    Change directory");
            Console.WriteLine("dir         List directory contents");
            Console.WriteLine("md <dir>    Create directory");
            Console.WriteLine("rd <dir>    Remove empty directory");
            Console.WriteLine("copy s d    Copy file");
            Console.WriteLine("del <file>  Delete file");
            Console.WriteLine("type <file> Display file content");
            Console.WriteLine("exit        Exit shell");
        }
    }
}
