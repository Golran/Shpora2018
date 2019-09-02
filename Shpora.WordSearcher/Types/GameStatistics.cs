using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shpora.WordSearcher.Types
{
    public class GameStatistics
    {
        public int Points { get; set; }
        public int Moves { get; set; }
        public int Words { get; set; }

        public GameStatistics(int points,int moves, int words )
        {
            Points = points;
            Moves = moves;
            Words = words;
        }
    }
}
