using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBrowser
{
    public class ChessPlayer
    {
        public int PlayerID { get; set; }
        public string Name { get; set; }
        public int Elo { get; set; }

        public ChessPlayer() { }
    }
}
