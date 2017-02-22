namespace InputWriter.Type {

    public struct Character {
        private char C;

        public Character( int c ) {
            C = ( char )c;
        }

        public Character( char c ) {
            C = c;
        }

        public static implicit operator Character( int c ) {
            return new Character( ( char )c );
        }

        public static implicit operator Character( char c ) {
            return new Character( c );
        }

        public static implicit operator int( Character c ) {
            return c.C;
        }

        public static implicit operator char( Character c ) {
            return c.C;
        }

        public static bool operator ==( Character a , int b ) {
            return a.C == b;
        }

        public static bool operator !=( Character a , int b ) {
            return !( a == b );
        }

        public static bool operator ==( int a , Character b ) {
            return ( b == a );
        }

        public static bool operator !=( int a , Character b ) {
            return !( b == a );
        }

        public override string ToString() {
            return string.Format( "char='{0}',int={1}" , char.ToString( this.C ) , ( int )this.C );
        }
    }
}