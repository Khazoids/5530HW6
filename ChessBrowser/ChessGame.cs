using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBrowser
{
    public class ChessGame
    {  
        public string Event { get; set; }
        public string Round { get; set; }
        public int WhitePlayer { get; set; }
        public int BlackPlayer { get; set; }
        public char Result { get; set; }
        public string Moves { get; set; }


        public ChessGame() { }
    }
}
