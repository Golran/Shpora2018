using Shpora.WordSearcher.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Shpora.WordSearcher
{
    public class WordSearcher
    {
        private const string voidSpace = "00000000000\r\n00000000000\r\n00000000000\r\n00000000000\r\n00000000000";
        private const string voidLine = "00000000000";
        private const string voidLetter = "0000000 0000000 0000000 0000000 0000000 0000000 0000000";
        private const int stepsForMoveVertical = 30;
        private const int stepsForMoveHorizontal = 11;
        public TypeMove LastTypeMove { get; private set; }
        public int AllowedNumberRepetitions { get; private set; }
        public int NumberRepetitions { get; private set; }
        public bool StartTurn { get; private set; }
        public int NumberVoidTurn { get; private set; }
        public int GlobalIterationIndex { get; private set; }
        public List<string> WordsOnOneTurn { get; private set; }
        public List<string> SearchedWord { get; private set; }
        public int LengthOneTurn { get; private set; }
        public int NumberMoveOnRight { get; private set; }
        public int NumberMoveOnLeft { get; private set; }
        public string VisibleArea { get; private set; }
        public StateMachine StateMachine { get; private set; }

        public WordSearcher()
        {
            StateMachine = StateMachine.SearchFirsWord;
            SearchedWord = new List<string>();
            WordsOnOneTurn = new List<string>();
            StartTurn = true;
            AllowedNumberRepetitions = 4;
        }


        public void GameRun()
        {
            var gameSession = new GameSession();
            VisibleArea = gameSession.InitializingGamingSession();
            while (VisibleArea == "")
                VisibleArea = gameSession.InitializingGamingSession();
            var timeToFinish = gameSession.TimeToFinish * 1000 - 18000;
            var stopWatch = new Stopwatch();
            stopWatch.Restart();
            while (stopWatch.ElapsedMilliseconds<timeToFinish && NumberRepetitions <= AllowedNumberRepetitions && NumberVoidTurn < 2)
            {
                GlobalSearchWord(gameSession);
            }
            SearchedWord.AddRange(WordsOnOneTurn);
            Console.WriteLine(SearchedWord.Count);
            var words = SearchedWord.OrderBy(word => word.Length).ToList();
            gameSession.DeliveryFoundWords(words);
            EndGame(gameSession, stopWatch);
        }

        private void GlobalSearchWord(GameSession gameSession)
        {
            switch (StateMachine)
            {
                case StateMachine.SearchFirsWord:
                    SearchFirsWord(gameSession, TypeMove.Down);
                    StateMachine = StateMachine.VisibleAreaAnalysis;
                    break;
                case StateMachine.VisibleAreaAnalysis:
                    StateMachine = VisibleAreaAnalysis(gameSession);
                    break;
                case StateMachine.MoveRight:
                    MoveInDirection(gameSession, 7, TypeMove.Right);
                    NumberMoveOnRight += 7;
                    StateMachine = StateMachine.GoToTopPartOfWord;
                    break;
                case StateMachine.MoveLeft:
                    MoveInDirection(gameSession, 7, TypeMove.Left);
                    NumberMoveOnLeft += 7;
                    StateMachine = StateMachine.GoToTopPartOfWord;
                    break;
                case StateMachine.GoToTopPartOfWord:
                    MoveToTopPartWord(gameSession);
                    StateMachine = StateMachine.GoToBeginningOfWord;
                    break;
                case StateMachine.GoToBeginningOfWord:
                    MoveToBeginningWord(gameSession);
                    StateMachine = StateMachine.RecognizeWord;
                    break;
                case StateMachine.RecognizeWord:
                    StateMachine = RecognizeWord(gameSession);
                    break;
                case StateMachine.SearchWordOnFirstTurn:
                    SearchWordOnOneTurn(gameSession, TypeMove.Down);
                    StateMachine = StateMachine.VisibleAreaAnalysis;
                    break;
                case StateMachine.SearchWordOnOtherTurns:
                    SearchWordOnTor(gameSession, TypeMove.Down);
                    StateMachine = GlobalIterationIndex >= LengthOneTurn-8 ? StateMachine.GoToNextTurnover : StateMachine.VisibleAreaAnalysis;
                    break;
                case StateMachine.GoToNextTurnover:
                    GoToNextTurnover(gameSession);
                    StateMachine = StateMachine.SearchWordOnOtherTurns;
                    break;
            }
        }


        private void EndGame(GameSession bot, Stopwatch stopwatch)
        {
            var gameStat = bot.GetStats();
            bot.EndGameSession();
            var str = @"В этой игре набрано {0} очков! Выполнено {1} шагов! А так же найдено {2} слов!";
            var message = string.Format(str, gameStat.Points, gameStat.Moves, gameStat.Words);
            Console.WriteLine(message);
            Console.WriteLine(stopwatch.Elapsed);
            stopwatch.Stop();
        }

        private void MoveToBeginningWord(GameSession gameSession)
        {
            MoveToStart(gameSession);
            VisibleArea = gameSession.Move(TypeMove.Down);
            VisibleArea = gameSession.Move(TypeMove.Down);
            MoveToStart(gameSession);
        }

        private void MoveToStart(GameSession gameSession)
        {
            var visibleArea = VisibleArea.Replace("\r\n", " ").Split()
                .Select(line => line.Substring(0, 4))
                .Take(4)
                .ToArray();
            while (!visibleArea.All(line => line == "0000"))
            {
                VisibleArea = gameSession.Move(TypeMove.Left);
                NumberMoveOnLeft++;
                visibleArea = VisibleArea.Replace("\r\n", " ").Split()
                .Select(line => line.Substring(0, 4))
                .Take(4)
                .ToArray();
            }
        }

        private StateMachine RecognizeWord(GameSession gameSession)
        {
            var typeMove = TypeMove.Up;
            LastTypeMove = TypeMove.Up;
            var visibleLetter = "";
            var searchedLetter = 'ъ';
            var word = "";
            while (visibleLetter != voidLetter && searchedLetter != ' ')
            {
                switch (typeMove)
                {
                    case TypeMove.Right:
                        MoveInDirection(gameSession, 8, TypeMove.Right);
                        NumberMoveOnRight += 8;
                        if (LastTypeMove == TypeMove.Up)
                            typeMove = TypeMove.Down;
                        else typeMove = TypeMove.Up;
                        LastTypeMove = typeMove;
                        break;
                    case TypeMove.Down:
                        searchedLetter = ' ';
                        visibleLetter = RecognizeLetter(gameSession,typeMove);
                        typeMove = TypeMove.Right;
                        break;
                    case TypeMove.Up:
                        searchedLetter = ' ';
                        visibleLetter = RecognizeLetter(gameSession,typeMove);
                        typeMove = TypeMove.Right;
                        break;
                }
                if (gameSession.Alphabet.ContainsKey(visibleLetter) && typeMove == TypeMove.Right)
                {
                    searchedLetter = gameSession.Alphabet[visibleLetter];
                    word += searchedLetter;
                }
            }
            EndSearchWord(gameSession, word);
            return StateMachine = StartTurn ? StateMachine.SearchWordOnFirstTurn : StateMachine.SearchWordOnOtherTurns;
        }


        private void EndSearchWord(GameSession gameSession, string word)
        {
            if (LastTypeMove == TypeMove.Up)
                MoveInDirection(gameSession, 7, TypeMove.Down);
            else MoveInDirection(gameSession, 5, TypeMove.Down);
            LengthOneTurn = StartTurn ? GlobalIterationIndex : LengthOneTurn;
            if ((SearchedWord.Contains(word) || WordsOnOneTurn.Contains(word)) && word != "")
            {
                NumberRepetitions++;
                StartTurn = false;
                Console.WriteLine("Найденно повторное слово: " + word);
            }
            else if (word != "")
            {
                WordsOnOneTurn.Add(word);
                Console.WriteLine("Найденно слово: " + word);
            }
            var moves = NumberMoveOnRight - NumberMoveOnLeft;
            if (moves >= 0)
                MoveInDirection(gameSession, moves, TypeMove.Left);
            else
            {
                moves = Math.Abs(moves);
                MoveInDirection(gameSession, moves, TypeMove.Right);
            }
            NumberMoveOnRight = 0;
            NumberMoveOnLeft = 0;

        }

        private string RecognizeLetter(GameSession gameSession, TypeMove typeMove)
        {
            var letter = "";
            var visiblePartLetter1 = VisibleArea.Replace("\r\n", " ").Split()
              .Select(line => line.Substring(4))
              .ToArray();
            MoveInDirection(gameSession, 2, typeMove);
            var visiblePartLetter2 = VisibleArea.Replace("\r\n", " ").Split()
                .Select(line => line.Substring(4));
            visiblePartLetter2 = typeMove == TypeMove.Down ? visiblePartLetter2.Skip(3).ToArray():
                visiblePartLetter2.Take(2).ToArray();
            var visibleLetter = typeMove == TypeMove.Down ? visiblePartLetter1.Concat(visiblePartLetter2).ToArray():
                visiblePartLetter2.Concat(visiblePartLetter1).ToArray();
            letter += visibleLetter[0];
            for (int i = 1; i < visibleLetter.Length; i++)
            {
                letter += ' ' + visibleLetter[i];
            }
            return letter;
        }

        private void MoveInDirection(GameSession gameSession, int countMove, TypeMove typeMove)
        {
            if (typeMove == TypeMove.Down)
                GlobalIterationIndex += countMove;
            if (typeMove == TypeMove.Up)
                GlobalIterationIndex -= countMove;
            for (int i = 0; i < countMove; i++)
                VisibleArea = gameSession.Move(typeMove);
        }

        private StateMachine VisibleAreaAnalysis(GameSession gameSession)
        {
            var visibleArea = VisibleArea.Replace("\r\n", " ").Split();
            StateMachine resultState;
            if (visibleArea.Take(visibleArea.Length - 1).All(line => line == voidLine))
            {
                MoveInDirection(gameSession, 4, TypeMove.Down);
            }
            visibleArea = VisibleArea.Replace("\r\n", " ").Split();
            var leftPartArea = visibleArea.Select(line => line.Substring(0, 4)).ToArray();
            var rightPartArea = visibleArea.Select(line => line.Substring(7)).ToArray();
            if (rightPartArea.All(line => line == "0000"))
                resultState = StateMachine.MoveLeft;
            else if (leftPartArea.All(line => line == "0000"))
                resultState = StateMachine.MoveRight;
            else resultState = StateMachine.GoToTopPartOfWord;
            return resultState;
        }

        private void MoveToTopPartWord(GameSession gameSession)
        {
            var resultRec = RecognizePosition();
            var typeMove = resultRec.Item1;
            var countVoidLine = resultRec.Item2;
            if (countVoidLine == 0)
            {
                var visibleArea = VisibleArea.Replace("\r\n", " ").Split();
                while (visibleArea[0] != voidLine)
                {
                    MoveInDirection(gameSession, 1, TypeMove.Up);
                    visibleArea = VisibleArea.Replace("\r\n", " ").Split();
                }
                MoveInDirection(gameSession, 1, TypeMove.Down);
            }
            else MoveInDirection(gameSession, countVoidLine, typeMove);
        }

        private Tuple<TypeMove, int> RecognizePosition()
        {
            var visibleArea = VisibleArea.Replace("\r\n", " ").Split();
            var countVoidLine = 0;
            TypeMove typeMove;
            if (visibleArea[0] == voidLine)
            {
                typeMove = TypeMove.Down;
                countVoidLine++;
                for (int i = 1; i < visibleArea.Length; i++)
                {
                    if (visibleArea[i] == voidLine) countVoidLine++;
                    else break;
                }
            }
            else if (visibleArea[visibleArea.Length - 1] == voidLine)
            {
                typeMove = TypeMove.Up;
                countVoidLine++;
                for (int i = visibleArea.Length - 2; i > 0; i--)
                {
                    if (visibleArea[i] == voidLine) countVoidLine++;
                    else break;
                }
                countVoidLine += 2;
            }
            else typeMove = TypeMove.Up;
            return Tuple.Create(typeMove, countVoidLine);
        }

        private void SearchWordOnOneTurn(GameSession gameSession, TypeMove typeVertical)
        {
            while (VisibleArea == voidSpace)
            {
                VisibleArea = gameSession.Move(typeVertical);
                GlobalIterationIndex++;
            }
        }

        private void SearchWordOnTor(GameSession gameSession, TypeMove typeVertical)
        {
            for (; GlobalIterationIndex < LengthOneTurn-7; GlobalIterationIndex++)
            {
                if (VisibleArea != voidSpace)
                    break;
                VisibleArea = gameSession.Move(typeVertical);
            }
        }

        private void SearchFirsWord(GameSession gameSession, TypeMove typeVertical)
        {
            var typeMove = typeVertical;
            var steps = stepsForMoveVertical;
            while (VisibleArea == voidSpace)
            {
                steps--;
                switch (typeMove)
                {
                    case TypeMove.Down:
                        VisibleArea = gameSession.Move(typeVertical);
                        if (steps <= 1)
                        {
                            typeMove = TypeMove.Right;
                            steps = stepsForMoveHorizontal;
                        }
                        break;
                    case TypeMove.Right:
                        VisibleArea = gameSession.Move(TypeMove.Right);
                        if (steps <= 1)
                        {
                            typeMove = typeVertical;
                            steps = stepsForMoveVertical;
                        }
                        break;
                }
            }
        }

        private void GoToNextTurnover(GameSession gameSession)
        {
            Console.WriteLine("Переход на следующий оборот");
            GlobalIterationIndex = 0;
            NumberRepetitions = 0;
            if (WordsOnOneTurn.Count == 0)
                NumberVoidTurn++;
            SearchedWord.AddRange(WordsOnOneTurn);
            var lenghtMiddleWord= WordsOnOneTurn.Count > 2 ? WordsOnOneTurn.OrderByDescending(word => word.Length)
                .ToArray()[WordsOnOneTurn.Count / 2 ].Length : 3;
            MoveInDirection(gameSession, (lenghtMiddleWord + 1)* 8, TypeMove.Right);
            AllowedNumberRepetitions = WordsOnOneTurn.Count > 10 ?  WordsOnOneTurn.Count / 2  : 5;
            WordsOnOneTurn = new List<string>();
        }
    }
}
