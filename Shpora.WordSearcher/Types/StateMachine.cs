using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shpora.WordSearcher.Types
{
    public enum StateMachine
    {
        SearchFirsWord, VisibleAreaAnalysis, MoveRight, MoveLeft,
        GoToTopPartOfWord, GoToBeginningOfWord, RecognizeWord,
        SearchWordOnFirstTurn, SearchWordOnOtherTurns, GoToNextTurnover
    }
}
