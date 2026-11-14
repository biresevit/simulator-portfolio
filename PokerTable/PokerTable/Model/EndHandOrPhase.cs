using DBLib;
using DecisionMaking;
using HandStateLib;
using PokerTable.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PokerTable.Model
{
    public class EndHandOrPhase
    {
        #region Methods
        /// <summary>
        /// This method checks if is All In / 1 or 0 player have balance
        /// </summary>
        /// <param name="hs">current handstate from table</param>
        /// <param name="didPlayersPlay">list did players play, foreach player move shows true, else it's false</param>
        public static bool AllIn(HandState hs, List<bool> didPlayersPlay)
        {
            bool isBetSmaller = false;
            int inGameCount = 0;
            if (AllPlayersPlayed(hs, didPlayersPlay))
            {
                for (int i = 0; i < hs.InGame.Count; i++)
                {
                    if (hs.InGame[i])
                    {
                        inGameCount += 1;
                    }
                }
                int playersHaveBalance = 0;
                double biggestBet = hs.Bets.Max();
                if (inGameCount >= 2)
                {
                    for (int j = 0; j < hs.InGame.Count; j++)
                    {
                        if (hs.InGame[j] && hs.Stacks[j] > 0)
                        {
                            playersHaveBalance++;
                            if (hs.Bets[j] < biggestBet)
                            {
                                isBetSmaller = true;
                                break;
                            }
                        }
                    }
                    if (playersHaveBalance == 1 || playersHaveBalance == 0)
                    {
                        if (isBetSmaller)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// This method checks if is All players from didPlayersPlay list played
        /// </summary>
        /// <param name="hs">current handstate from table</param>
        /// <param name="didPlayersPlay">list did players play, foreach player move shows true, else it's false</param>
        public static bool AllPlayersPlayed(HandState hs, List<bool> didPlayersPlay)
        {
            for (int i = 0; i < hs.InGame.Count; i++)
            {
                if (hs.InGame[i] && !didPlayersPlay[i])
                {

                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// This method checks if current phase is over
        /// </summary>
        public static bool IsPhaseFinished(HandState hs, bool phaseOver, List<bool> didPlayersPlay, bool useSleep, int sleepTime)
        {
            int numInGame = hs.InGame.Count(o => o == true);

            if (numInGame == 1)
            {
                return true;
            }
            List<double> allBets = new List<double>();
            for (int i = 0; i < hs.Bets.Count; i++)
            {
                if (hs.InGame[i] && (hs.Stacks[i] > 0 || (hs.Stacks[i] == 0 && hs.Bets[i] > 0)))
                {
                    allBets.Add(hs.Bets[i]);
                }
            }
            bool areAllBetssame = allBets.Distinct().ToList().Count == 1;
            if (areAllBetssame && AllPlayersPlayed(hs, didPlayersPlay))
            {
                phaseOver = true;
                return true;
            }
            if (useSleep)
            {
                Thread.Sleep(sleepTime);
            }
            return false;
        }
        public static void PotSizeToWinners(List<string> winnersList, ObservableCollection<PlayerViewModel> playersToPlay, double potSize, EnumGameType gameType)
        {
            if (winnersList.Count == 1)
            {
                for (int i = 0; i < playersToPlay.Count; i++)
                {
                    playersToPlay[i].BetSize = 0;
                    if (gameType == EnumGameType.NLH)
                    {
                        if (playersToPlay[i].Cards == winnersList[0])
                        {
                            playersToPlay[i].IsWinner = true;
                            playersToPlay[i].Balance += potSize;
                            break;
                        }
                    }
                    else
                    {
                        if (playersToPlay[i].Name == winnersList[0])
                        {
                            playersToPlay[i].IsWinner = true;
                            playersToPlay[i].Balance += potSize;
                            break;
                        }
                    }

                }
                potSize = 0;
            }
            else
            {
                for (int i = 0; i < playersToPlay.Count; i++)
                {
                    if (gameType == EnumGameType.NLH)
                    {
                        foreach (var item in winnersList)
                        {
                            if (playersToPlay[i].Cards == item)
                            {
                                playersToPlay[i].IsWinner = true;

                                playersToPlay[i].Balance += potSize / winnersList.Count;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in winnersList)
                        {
                            if (playersToPlay[i].Name == item)
                            {
                                playersToPlay[i].IsWinner = true;

                                playersToPlay[i].Balance += potSize / winnersList.Count;
                                break;
                            }
                        }
                    }
                }
                potSize = 0;
            }
        }
        #endregion
    }
}
