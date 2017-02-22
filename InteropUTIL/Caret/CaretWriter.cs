using InputWriter.Type;
using System;
using System.Text;

namespace InputWriter {

    public class CaretWriter {

        /// <summary>
        /// Handler for the caret position
        /// </summary>
        private struct Position {
            private int x_o, y_o, x, y;

            public int X {
                get { return x; }
                set {
                    //Cursor Updating
                    if( x + value >= 0 )
                        x = value;
                    Console.CursorLeft = x;
                }
            }
            public int Y {
                get { return y; }
                set {
                    //Cursor Updating
                    if( y + value >= 0 )
                        y = value;
                    Console.CursorTop = y;
                }
            }

            /// <summary>
            /// Create a new position handler.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public Position( int x , int y ) {
                this.x = this.x_o = x;
                this.y = this.y_o = y;
            }
        }

        private struct CaretPosition {
            private int x,y;

            public int X {
                get { return x; }
                set { x = value; }
            }
            public int Y {
                get { return y; }
                set { y = value; }
            }

            public CaretPosition( int x , int y ) {
                this.x = x;
                this.y = y;
            }

            public static CaretPosition operator -( Position A , CaretPosition B ) {
                return new CaretPosition( A.X - B.X , A.X - B.Y );
            }

            public static CaretPosition operator +( Position A , CaretPosition B ) {
                return new CaretPosition( A.X + B.X , A.X + B.Y );
            }

            public static CaretPosition operator -( CaretPosition A , Position B ) {
                return new CaretPosition( A.X - B.X , A.X - B.Y );
            }

            public static CaretPosition operator +( CaretPosition A , Position B ) {
                return new CaretPosition( A.X + B.X , A.X + B.Y );
            }

            public static implicit operator CaretPosition( Position A ) {
                return new CaretPosition( A.X , A.Y );
            }
        }

        /// <summary>
        /// This is where the current screen cursor is located.
        /// </summary>
        private static Position Cursor = new Position( Console.CursorLeft , Console.CursorTop );

        /// <summary>
        /// This is where it writes characters
        /// </summary>
        private StringBuilder Builder;

        /// <summary>
        /// The start of the builder
        /// </summary>
        private CaretPosition Start;

        /// <summary>
        /// The end of the builder
        /// </summary>
        private CaretPosition End;

        private CaretWriter old;

        /// <summary>
        /// CaretWriter. For specialized input. Still under testing/writing. Takes a StringBuilder as a parameter.
        /// </summary>
        /// <param name="b"></param>
        public CaretWriter() {
            this.old = null;
            this.Builder = new StringBuilder();
            this.Start = new CaretPosition( Cursor.X , Cursor.Y );
            this.End = this.Start;
        }

        public CaretWriter( CaretWriter caret ) {
            this.old = caret;
            this.Builder = new StringBuilder();
            this.Start = new CaretPosition( caret.End.X , caret.End.Y );
            this.End = this.Start;
        }

        /// <summary>
        /// Deletes one character and moves the caret to the left once
        /// </summary>
        public void BackSpace() {
            try {
                var span = Cursor - Start;
                this.Builder.Remove( span.X - 1 , 1 );
                this.End.X -= 1;
            } catch( Exception ) {
                //Do Nothing
            } finally {
                Cursor.X -= 1;
                Console.Write( ' ' );
                Cursor.X += 0;
            }
        }

        /// <summary>
        /// Moves the cursor down and to the left creating a newline but, without adding a newline character.
        /// </summary>
        public void NewLine() {
            Cursor.Y += 1;
            Cursor.X = 0;
        }

        public void Last() {
            Cursor.Y -= 1;
            Cursor.X = End.X;
        }

        /// <summary>
        /// Moves the caret to the left once
        /// </summary>
        public void Left() {
            try {
                if( Cursor.X > this.Start.X )
                    Cursor.X -= 1;
            } catch( Exception ) {
                return;
            }
        }

        /// <summary>
        /// Moves the caret to the right once, if it is within the builder length
        /// </summary>
        public void Right() {
            try {
                if( Cursor.X < this.End.X )
                    Cursor.X += 1;
            } catch( Exception ) {
                return;
            }
        }

        /// <summary>
        /// Write this character to the screen and moves the caret to the right.
        /// </summary>
        /// <param name="c"></param>
        public void Write( Character c ) {
            if( Cursor.X >= this.End.X ) {
                this.Builder.Append( c );
                this.End.X += 1;
            } else {
                var span = Cursor - Start;
                this.Builder.Remove( span.X , 1 ).Insert( span.X , c );
            }
            Console.Out.Write( c );
            Cursor.X += 1;
        }

        public CaretWriter EndOfCaret() {
            this.End = Cursor;
            return this.old;
        }

        public static implicit operator StringBuilder( CaretWriter A ) {
            return A.Builder;
        }

        public override string ToString() {
            return this.Builder.Replace( ' ' , '\b' ).ToString();
        }
    }
}