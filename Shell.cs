using System;
using System.IO;
using System.Linq;

namespace MiniFatFs
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
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                input = input.Trim();

                // handle echo specially (because it may contain spaces/quotes)
                if (input.StartsWith("echo", StringComparison.OrdinalIgnoreCase))
                {
                    try { Echo(input); }
                    catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
                    continue;
                }

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
                        case "ls":
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

                        case "clear":
                            Clear();
                            break;

                        case "mv":
                            Mv(parts);
                            break;

                        case "touch":
                            Touch(parts);
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

            // support absolute paths too
            string target = parts[1];
            string path = Path.IsPathRooted(target)
                ? target
                : Path.Combine(currentDir.FullName, target);

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

        /* ================= Added Commands ================= */

        private void Clear()
        {
            // some environments may not allow Console.Clear()
            try
            {
                Console.Clear();
            }
            catch
            {
                Console.WriteLine(new string('\n', 60));
            }
        }

        private void Mv(string[] parts)
        {
            // mv <src> <dst>
            if (parts.Length < 3)
            {
                Console.WriteLine("Usage: mv <source> <destination>");
                return;
            }

            string src = Path.Combine(currentDir.FullName, parts[1]);
            string dst = Path.Combine(currentDir.FullName, parts[2]);

            if (!File.Exists(src))
            {
                Console.WriteLine("The system cannot find the file specified.");
                return;
            }

            // if destination exists, overwrite (delete then move)
            if (File.Exists(dst))
                File.Delete(dst);

            File.Move(src, dst);
            Console.WriteLine("File moved successfully.");
        }

        private void Touch(string[] parts)
        {
            // touch <file>
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: touch <file>");
                return;
            }

            string path = Path.Combine(currentDir.FullName, parts[1]);

            if (!File.Exists(path))
            {
                // create empty file
                using (File.Create(path)) { }
                Console.WriteLine("File created successfully.");
            }
            else
            {
                // update timestamps
                File.SetLastWriteTime(path, DateTime.Now);
                Console.WriteLine("File timestamp updated.");
            }
        }

        private void Echo(string rawInput)
        {
            // echo "text" <file>
            // echo "text" <file> --append
            // We parse by quotes to allow spaces inside text
            string s = rawInput.Trim();

            // remove leading "echo"
            if (s.Length < 4) { Console.WriteLine("Usage: echo \"text\" <file> [--append]"); return; }
            s = s.Substring(4).Trim();

            if (!s.StartsWith("\""))
            {
                Console.WriteLine("Usage: echo \"text\" <file> [--append]");
                return;
            }

            int secondQuote = s.IndexOf('"', 1);
            if (secondQuote == -1)
            {
                Console.WriteLine("Error: missing closing quote.");
                return;
            }

            string text = s.Substring(1, secondQuote - 1);
            string rest = s.Substring(secondQuote + 1).Trim(); // should contain <file> [--append]

            if (string.IsNullOrWhiteSpace(rest))
            {
                Console.WriteLine("Usage: echo \"text\" <file> [--append]");
                return;
            }

            string[] tokens = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string fileName = tokens[0];
            bool append = tokens.Skip(1).Any(t => t.Equals("--append", StringComparison.OrdinalIgnoreCase));

            string path = Path.Combine(currentDir.FullName, fileName);

            if (append)
                File.AppendAllText(path, text + Environment.NewLine);
            else
                File.WriteAllText(path, text + Environment.NewLine);

            Console.WriteLine(append ? "Text appended successfully." : "Text written successfully.");
        }

        private void Help()
        {
            Console.WriteLine("Supported commands:");
            Console.WriteLine("cd <dir>              Change directory");
            Console.WriteLine("dir                   List directory contents");
            Console.WriteLine("md <dir>              Create directory");
            Console.WriteLine("rd <dir>              Remove empty directory");
            Console.WriteLine("copy <src> <dst>       Copy file");
            Console.WriteLine("del <file>            Delete file");
            Console.WriteLine("type <file>           Display file content");
            Console.WriteLine("clear                 Clear the screen");
            Console.WriteLine("mv <src> <dst>         Move/Rename file");
            Console.WriteLine("touch <file>          Create empty file (or update timestamp)");
            Console.WriteLine("echo \"text\" <file>      Write text to file (overwrite)");
            Console.WriteLine("echo \"text\" <file> --append  Append text to file");
            Console.WriteLine("exit                  Exit shell");
        }
    }
}
