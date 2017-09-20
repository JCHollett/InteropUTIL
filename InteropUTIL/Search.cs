using System;
using System.Collections.Generic;
using static Interop.Marshal.Kernel32;

namespace Interop {

    /// <summary>
    /// Work in progress, unfinished code
    /// </summary>
    public class Search {
        private List<Predicate<WIN32_FILE>> conditions;

        private string srch;

        public Search( string x ) {
            this.conditions = new List<Predicate<WIN32_FILE>>();
            var bytes = x.ToCharArray();
            foreach( var indice in GetIndices( bytes ) ) {
                Console.WriteLine( string.Format( "[{0},{1}]:{2}" , indice.Item1 , indice.Item2 , new string( bytes , indice.Item1 , indice.Item2 - indice.Item1 ) ) );
            }
            Console.WriteLine( this.srch );
        }

        public static Queue<Tuple<int , int>> GetIndices( char[ ] charz ) {
            Queue<Tuple<int,int>> q = new Queue<Tuple<int, int>>();
            int i = 0;
            int j = 0;
            bool flipflop = true;
            if( charz.Length >= 0 ) {
                while( i < charz.Length && j < charz.Length ) {
                    switch( flipflop ) {
                        case true:
                            if( charz[ i ] == '-' ) {
                                flipflop = !flipflop;
                                j = i + 1;
                            } else {
                                ++i;
                            }
                            break;

                        case false:
                            if( charz[ j ] == '-' ) {
                                flipflop = !flipflop;
                                q.Enqueue( new Tuple<int , int>( i , j - 1 ) );
                                i = j;
                            } else {
                                j++;
                            }
                            break;
                    }
                }
                if( i > 0 && j > i ) {
                    q.Enqueue( new Tuple<int , int>( i , j ) );
                }
                return q;
            } else {
                return q;
            }
        }
    }
}