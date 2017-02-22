using CSSQL.Marshal;
using System;
using System.IO;
using static CSSQL.Marshal.Kernel32;

namespace CSSQL.FileSystem {

    public static class Utils {

        public static bool Create( string destDirectory ) {
            if( destDirectory == null || destDirectory.Length == 0 ) {
                throw new ArgumentException( "Bad path argument" );
            }
            string[] path = destDirectory.Split('\\','/');
            if( path.Length < 2 ) {
                throw new ArgumentException( "Path too short" );
            } else {
                string folder = path[0];

                for( int i = 1; i < path.Length; i++ ) {
                    folder += Path.DirectorySeparatorChar + path[ i ];
                    Kernel32.CreateDirectory( folder , IntPtr.Zero );
                }
                return true;
            }
        }

        public static WIN32_FILE GetFile( string path ) {
            WIN32_FILE v;
            IntPtr ptr = FindFirstFile( @"\\?\" + path, out v );
            if( ptr == BAD_HANDLE ) {
                return default( WIN32_FILE );
            }
            return v;
        }
    }

    public class FileSize {

        //Kibibytes, Mebibytes, Gibibytes, Tebibytes, Pebibytes;
        public enum Prefix { Bytes = 0, KiB = 1, MiB, GiB, TiB, PiB };

        private static double[] Factors = {Math.Pow(2,0),Math.Pow( 2 , 10 ) , Math.Pow( 2 , 20 ) , Math.Pow( 2 , 30 ) , Math.Pow( 2 , 40 ), Math.Pow(2,50) };
        public ulong Raw { get; set; }
        public string Readable { get; set; }

        public FileSize( ulong s ) {
            this.Raw = s;
            foreach( double F in Factors ) {
                if( this.Raw / F < ( 1024 ) ) {
                    this.Readable = string.Format( "{0}{1}" , Math.Round( this.Raw / F , 3 ) , Enum.GetName( typeof( Prefix ) , ( Prefix )( Math.Log( F ) / Math.Log( 2 ) / 10 ) ) );
                    break;
                }
            }
        }

        public FileSize( double s , Prefix e ) : this( ( ulong )( Factors[ ( int )e - 1 ] * s ) ) {
        }

        public override string ToString() {
            return this.Readable;
        }

        public static explicit operator FileSize( ulong v ) {
            return new FileSize( v );
        }

        public static implicit operator ulong( FileSize f ) {
            return f.Raw;
        }
    }
}