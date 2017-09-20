using Interop.FileSystem;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Interop.Marshal {

    public static class Kernel32 {

        [StructLayout( LayoutKind.Sequential , CharSet = CharSet.Unicode )]
        public struct WIN32_FILE {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh; //changed all to uint, otherwise you run into unexpected overflow
            public uint nFileSizeLow;  //|
            public uint dwReserved0;   //|
            public uint dwReserved1;   //v

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_PATH)]
            public string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_ALTERNATE)]
            public string cAlternate;

            public ulong FileSize() {
                return ( ( ulong )nFileSizeHigh << 32 ) | nFileSizeLow;
            }

            public string Extension { get { return FileExt( this.cFileName ); } }

            public bool MoveTo( string dir ) {
                var path =  dir + ( dir.EndsWith( @"\" ) ? "" : @"\" );
                if( MoveFile( @"\\?\" + cAlternate , ( @"\\?\" + path + cFileName ) ) ) {
                    return true;
                } else {
                    if( PathFileExists( path ) ) {
                        var f = Utils.GetFile( path + cFileName );
                        f.cAlternate = path + cFileName;
                        if( f.FileSize() > this.FileSize() ) {
                            f.Delete();
                            return this.MoveTo( dir );
                        } else {
                            return this.Delete();
                        }
                    } else {
                        Utils.Create( path );
                        return this.MoveTo( dir );
                    }
                }
            }

            public bool Delete() {
                return DeleteFileW( cAlternate ) || RemoveDirectory( cAlternate );
            }

            private static string FileExt( string f ) {
                var b = new StringBuilder();
                foreach( char c in f.Reverse().TakeWhile( x => x != '.' ) ) {
                    b.Insert( 0 , c );
                }
                return b.Length != f.Length ? b.Insert( 0 , "*." ).ToString() : "NONE";
            }

            public override string ToString() {
                return string.Format( "[Name={0},Size={1}]" , this.cFileName , ( FileSize )this.FileSize() );
            }
        }

        [DllImport( "kernel32.dll" , SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool DeleteFileW( [MarshalAs( UnmanagedType.LPWStr )]string lpFileName );

        [DllImport( "kernel32.dll" , CharSet = CharSet.Unicode , SetLastError = true )]
        private static extern bool RemoveDirectory( string lpPathName );

        [StructLayout( LayoutKind.Sequential )]
        public struct FILETIME {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        };

        public const int MAX_PATH = 260;
        public const int MAX_ALTERNATE = 14;
        public static IntPtr BAD_HANDLE = new IntPtr( -1 );

        [DllImport( "shlwapi.dll" , EntryPoint = "PathFileExistsW" , SetLastError = true , CharSet = CharSet.Unicode )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool PathFileExists( [MarshalAs( UnmanagedType.LPTStr )]string pszPath );

        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool CreateDirectory( string lpPathName , IntPtr lpSecurityAttributes );

        [DllImport( "kernel32.dll" )]
        public static extern bool MoveFile( string lpExistingFileName , string lpNewFileName );

        [DllImport( "kernel32.dll" , CharSet = CharSet.Unicode )]
        public static extern IntPtr FindFirstFile( string lpFileName , out WIN32_FILE lpFindFileData );

        [DllImport( "kernel32.dll" , CharSet = CharSet.Unicode )]
        public static extern bool FindNextFile( IntPtr hFindFile , out WIN32_FILE lpFindFileData );

        [DllImport( "kernel32.dll" , SetLastError = true )]
        public static extern bool FindClose( IntPtr hFindFile );
    }
}