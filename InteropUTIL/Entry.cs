using InputWriter;
using InputWriter.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static Interop.Marshal.Kernel32;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Interop {

    public static class Ext {

        public static IEnumerable<object> GetObjects<T>(this T d) where T : IDataRecord {
            for (int i = 0; i <  d.FieldCount; ++i) {
                yield return d.GetValue(i);
            }
        }
    }

    public class ARTOBJECT {
        public string YEAR {
            get; set;
        }
        public string DESCRIPTION {
            get; set;
        }
    }

    public class Entry {

        public static IEnumerable<WIN32_FILE> Iterate(string dir) {
            return Iterate(dir, -1);
        }

        public static IEnumerable<WIN32_FILE> Iterate(string dir, int depth) {
            WIN32_FILE v;
            IntPtr ptr = FindFirstFile(@"\\?\" + dir + @"\*", out v);
            if (ptr != BAD_HANDLE) {
                do {
                    if ((v.dwFileAttributes & FileAttributes.Directory) != 0) {
                        if (v.cFileName != ".." && v.cFileName != ".") {
                            string subdirectory = dir + (dir.EndsWith(@"\") ? "" : @"\") + v.cFileName;
                            if (depth != 0) {
                                foreach (var file in Iterate(subdirectory, depth - 1)) {
                                    yield return file;
                                };
                            }
                        }
                    } else {
                        v.cAlternate = dir + (dir.EndsWith(@"\") ? "" : @"\") + (v.cFileName);
                        yield return v;
                    }
                } while (FindNextFile(ptr, out v));
                FindClose(ptr);
            }
        }

        public static long DeleteEmptySpace(string dir, out int files, out int folders) {
            WIN32_FILE v;
            IntPtr ptr = FindFirstFile(@"\\?\" + dir + @"\*", out v);
            long size = 0; files = 0; folders = 0;
            if (ptr != BAD_HANDLE) {
                do {
                    if ((v.dwFileAttributes & FileAttributes.Directory) != 0) {
                        if (!(v.cFileName == "." || v.cFileName == "..")) {
                            ++folders;
                            v.cAlternate = dir + (dir.EndsWith(@"\") ? "" : @"\") + (v.cFileName);
                            long subsize; int subfiles, subfolders;
                            string subdirectory = dir + (dir.EndsWith(@"\") ? "" : @"\") + v.cFileName;
                            subsize = DeleteEmptySpace(subdirectory, out subfiles, out subfolders);
                            if (subfiles > 0) {
                                folders += subfolders;
                                files += subfiles;
                            } else {
                                if (subfolders > 0) {
                                    folders += subfolders;
                                } else {
                                    --folders;
                                    if (v.Delete())
                                        Console.WriteLine("Deleted Folder: {0}", v.cFileName);
                                }
                            }
                            size += subsize;
                        }
                    } else {
                        v.cAlternate = dir + (dir.EndsWith(@"\") ? "" : @"\") + (v.cFileName);
                        long filesize = ((long)v.nFileSizeHigh << 32) + v.nFileSizeLow;
                        if (filesize > 0) {
                            ++files;
                        } else {
                            if (v.Delete())
                                Console.WriteLine("Deleted File: {0}", v.cFileName);
                        }
                    }
                } while (FindNextFile(ptr, out v));
                FindClose(ptr);
            }
            return size;
        }

        public static void MoveAll(string src, string dest, Predicate<WIN32_FILE> Conditions, int depth) {
            int Moved = 0;
            foreach (var x in Iterate(src, depth).Where(x => Conditions(x))) {
                bool b = x.MoveTo(dest);
                Console.WriteLine(x + @"->" + b);
                if (!b) {
                    string err = new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error()).Message;
                    Console.WriteLine(err);
                } else
                    ++Moved;
            }
            if (Moved > 0)
                Console.WriteLine(string.Format("Completed. Moved {0} files...", Moved));
        }

        public static void DeleteAll(string src, Predicate<WIN32_FILE> Conditions, int depth) {
            int Deleted = 0;

            foreach (var x in Iterate(src, depth).Where(x => Conditions(x))) {
                bool b = x.Delete();
                Console.WriteLine(x + @"->" + b);
                if (!b) {
                    string err = new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error()).Message;
                    Console.WriteLine(err);
                } else
                    ++Deleted;
            }
            if (Deleted > 0)
                Console.WriteLine(string.Format("Completed. Deleted {0} files...", Deleted));
        }

        public static void console() {
            Character c = '\0';
            Dictionary<Character, int> Variables = new Dictionary<Character, int>();
            ConsoleKeyInfo Info;
            Func<ConsoleKeyInfo> Get = () => {
                var pressed = Console.ReadKey(true);
                c = pressed.KeyChar;
                return pressed;
            };
            CaretWriter caret = new CaretWriter();
            while ((Info = Get()).Key != ConsoleKey.Enter) {
                switch (Info.Key) {
                    case ConsoleKey.Backspace:
                        caret.BackSpace();
                        break;

                    case ConsoleKey.Spacebar:
                        caret.Write(' ');
                        break;

                    case ConsoleKey.LeftArrow:
                        caret.Left();
                        break;

                    case ConsoleKey.RightArrow:
                        caret.Right();
                        break;

                    default:
                        break;
                }
                switch (char.GetUnicodeCategory(c)) {
                    case UnicodeCategory.OtherPunctuation:
                        caret.Write(c);
                        break;

                    default:
                        if ((!char.IsWhiteSpace(c) && !char.IsControl(c))) {
                            caret.Write(c);
                        }
                        break;
                }
            }
            Console.Write('\n');
            StringBuilder build = caret;
            string[] param = build.ToString().Split(':');
            switch (param[0]) {
                case "find":
                    Search s = new Search(param[1]);
                    break;
            }

            //TODO Replace the placeholders for the groups of parenthesis with the values from Par dictionary

            Console.ReadKey();
        }

        public static void Main(string[] args) {
            using (var output = File.CreateText("Output.txt")) {
                //foreach( var File in Iterate( @"F:\" ).Where( x => new[ ] { "tshock" , "terraria" }.Any( y => x.cFileName.ToLower().Contains( y ) ) ) ) {
                string str = "workspace";
                var search = new[] { "*.txt", "*.pdf", "*.cpp", "*.java" };
                foreach (var File in Iterate(@"I:\").Where(x => search.Any(y => x.Extension.Equals(y)))) {
                    output.WriteLine(File.ftLastAccessTime + "::" + File.cAlternate);
                }
            }
        }

        public static void Test() {
            int files, folders;
            long size = DeleteEmptySpace(@"X:\Downloads", out files, out folders);
        }
        /// <summary>
        ///  >>= >=> <=> <=< >= <= !== === == =>> ==> => <<= <> ## /= &= |= *= 
        /// </summary>
        public static void Work() {
            DeleteAll(@"X:\Downloads", x => x.cFileName.ToLower().Contains("sample"), 10);
            MoveAll(@"X:\Downloads", @"X:\Media\Text", x => (new[] { ".txt", ".pdf", ".doc", ".ppt" }).Any(y => x.cFileName.Contains(y)), 3);
            MoveAll(@"X:\Downloads", @"X:\Media\Executables", x => (new[] { ".exe", ".msi" }).Any(y => x.cFileName.Contains(y)), 3);
            MoveAll(@"X:\Downloads", @"X:\Media\Jars", x => x.cFileName.Contains(".jar"), 3);
            MoveAll(@"X:\Downloads", @"X:\Media\Images", x => (new[] { ".gif", ".png", ".jpeg", ".jpg", ".img" }).Any(y => x.cFileName.Contains(y)), 3);
            MoveAll(@"X:\Downloads", @"X:\Media\Archives", x => (new[] { ".rar", ".zip", ".7z" }).Any(y => x.cFileName.Contains(y)), 3);
            MoveAll(@"X:\Downloads", @"X:\Media\Source_Code", x => (new[] { ".java", ".py", ".cs" }).Any(y => x.cFileName.Contains(y)), 3);

            DeleteAll(@"X:\Downloads", x => (new string[] { ".torrent", ".htm", ".php" }).Any(y => x.cFileName.Contains(y)), 0);
            int files, folders;
            long size = DeleteEmptySpace(@"X:\Downloads", out files, out folders);
            Console.ReadKey();
        }
    }
}