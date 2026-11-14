using DBLib;
using DecisionMaking;
using DecisionMaking.DecisionMaking;
using HandStateLib;
using PokerTable.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PokerTable.Model
{
    public class HumanDecisionModel
    {
        #region Methods
        public static bool CheckIfPlayersPlayed(ref ObservableCollection<PlayerViewModel> playersToPlay)
        {
            bool result = false;
            foreach (var item in playersToPlay)
            {
                if (item.Action != null && item.Action != "")
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        public static void GoAllIn(ref ObservableCollection<PlayerViewModel> PlayersToPlay, ref int indexAct, ref double sumbets, ref bool humanPlayed, EnumGameType gameType, HandState hs, double PotSize,ref double BetSizePlayer, ref double bigBling)
        {
            if (gameType == EnumGameType.NLH)
            {
                PlayersToPlay[indexAct].BetSize = PlayersToPlay[indexAct].BetSize + PlayersToPlay[indexAct].Balance;
                PlayersToPlay[indexAct].Balance = 0;
                sumbets += PlayersToPlay[indexAct].BetSize;
            }
            else
            {
                double betSizePlayer = 0;
                var getOppId = GetOpponentID(PlayersToPlay);
                if (getOppId != -1)
                {
                    betSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs, indexAct, getOppId, 1);
                }
                else
                {
                    if (!CheckIfPlayersPlayed(ref PlayersToPlay))
                    {
                        for (int i = 0; i < PlayersToPlay.Count; i++)
                        {
                            if (PlayersToPlay[i].BetSize == bigBling)
                            {
                                betSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs, indexAct, i, 1);
                                break;
                            }
                        }
                    }
                }
                if (PlayersToPlay[indexAct].BetSize + betSizePlayer > PlayersToPlay[indexAct].Balance)
                {
                    betSizePlayer = PlayersToPlay[indexAct].Balance;
                }
                BetSizePlayer = betSizePlayer;
                //PlayersToPlay[indexAct].BetSize = PlayersToPlay[indexAct].BetSize + betSizePlayer;
                //PlayersToPlay[indexAct].Balance -= betSizePlayer;
                //sumbets += PlayersToPlay[indexAct].BetSize;
            }
            //PlayersToPlay[indexAct].Action = "ALL IN";
            //humanPlayed = true;
        }
        public static double GetHighestBet(HandState hs)
        {
            return hs.Bets.Max();
        }
        public static void LimitBet(ref double BetSizePlayer, ObservableCollection<PlayerViewModel> PlayersToPlay, int indexAct)
        {
            if (BetSizePlayer > PlayersToPlay[indexAct].Balance)
            {
                BetSizePlayer = PlayersToPlay[indexAct].Balance;
                PlayersToPlay[indexAct].BetSize = BetSizePlayer;
            }
        }
        public static void GetButtonsContent(double maxBet, EnumPhase phase, double BigBlind, ref string btnBetSize1, ref string btnBetSize2, ref string btnBetSize3, ref string btnBetSize4, ref string btnBetSize5, ref Visibility visibilityForPotButton)
        {
            if (phase == EnumPhase.Preflop && maxBet == BigBlind)
            {
                btnBetSize1 = "2x";
                btnBetSize2 = "2.5x";
                btnBetSize3 = "3x";
                btnBetSize4 = "3.5x";
                btnBetSize5 = "Pot";
                visibilityForPotButton = Visibility.Visible;
            }
            else
            {
                btnBetSize1 = "1/3";
                btnBetSize2 = "1/2";
                btnBetSize3 = "3/4";
                btnBetSize4 = "Pot";
                visibilityForPotButton = Visibility.Collapsed;
            }
        }
        public static double PotSizeRaise(HandState hs, double PotSize)
        {
            double maxBet = GetHighestBet(hs);
            double restBets = hs.Bets.Sum() - maxBet;
            double potSizeRaise = 2 * maxBet + PotSize + restBets;
            return potSizeRaise;
        }
        public static void LimitPotSizeOmaha(double potlimitOmaha, ref double BetSizePlayer, ObservableCollection<PlayerViewModel> PlayersToPlay, int indexAct)
        {
            if (PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize < potlimitOmaha)
            {
                BetSizePlayer = PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize;
            }
            else
            {
                BetSizePlayer = potlimitOmaha;

            }
        }
        /// <summary>
        /// This method serves to set Bet Button for human with values 1 or 3.5
        /// </summary>
        public static void BetBtn4(ref double BetPercent, ref HandState hs, ref EnumGameType gameType, ref EnumPhase phase, ref double BigBlind, ref double BetSizePlayer, ref ObservableCollection<PlayerViewModel> PlayersToPlay, ref int indexAct, ref double PotSize)
        {
            double maxBet = GetHighestBet(hs);
            double restBets = hs.Bets.Sum() - maxBet - PlayersToPlay[indexAct].BetSize;
            if (phase == EnumPhase.Preflop && maxBet <= BigBlind)
            {
                BetPercent = 3.5;
                BetSizePlayer = Math.Round(BetPercent * maxBet, 2);
                if (gameType == EnumGameType.NLH)
                {
                    if (PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize < BetSizePlayer)
                    {
                        LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);

                    }

                }
                else
                {
                    if (PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize < BetSizePlayer)
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);

                    }
                }
                //}
                //if (gameType == EnumGameType.NLH)
                //{

                //    if (true)
                //    {

                //    }
                //    LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);
                //}
                //else
                //{
                //   // BetSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs.Pot + restBets, maxBet, 1);

                //    LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                //}
            }
            else
            {
                var getOppId = GetOpponentID(PlayersToPlay);
                if (getOppId != -1)
                {
                    BetSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs, indexAct, getOppId, 1);
                    if (BetSizePlayer > PlayersToPlay[indexAct].BetSize + PlayersToPlay[indexAct].Balance)
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                    }
                    //BetSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs, indexAct, getOppId, 1);
                }
                else
                {
                    if (gameType == EnumGameType.NLH)
                    {
                        LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);
                    }
                    else
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                        if (BetSizePlayer < PotSizeRaise(hs, PotSize))
                        {

                        }
                        else
                        {
                            BetSizePlayer = Math.Round(BetSizePlayer, 2);

                        }
                        //BetSizePlayer = Math.Round(BetSizePlayer, 2);

                    }
                }
            }
        }
        /// <summary>
        /// This method serves to set Bet Button for human with values 0.75 or 3
        /// </summary>
        public static void BetBtn3(ref double BetPercent, ref HandState hs, ref EnumGameType gameType, ref EnumPhase phase, ref double BigBlind, ref double BetSizePlayer, ref ObservableCollection<PlayerViewModel> PlayersToPlay, ref int indexAct, ref double PotSize)
        {
            double maxBet = GetHighestBet(hs);
            double restBets = hs.Bets.Sum() - maxBet - PlayersToPlay[indexAct].BetSize;

            if (phase == EnumPhase.Preflop && maxBet <= BigBlind)
            {
                BetPercent = 3;
                BetSizePlayer = Math.Round(BetPercent * maxBet, 2);
                if (gameType == EnumGameType.NLH)
                {
                    if (PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize < BetSizePlayer)
                    {
                        LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);

                    }

                }
                else
                {
                    if (PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize < BetSizePlayer)
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);

                    }
                }
            }
            else
            {
                var getOppId = GetOpponentID(PlayersToPlay);
                if (getOppId != -1)
                {
                    BetSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs, indexAct, getOppId, 0.75);
                    if (BetSizePlayer > PlayersToPlay[indexAct].BetSize + PlayersToPlay[indexAct].Balance)
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                    }
                    //BetSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs, indexAct, getOppId, 0.75);
                }
                else
                {
                    if (gameType == EnumGameType.NLH)
                    {
                        LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);
                    }
                    else
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                        if (BetSizePlayer < PotSizeRaise(hs, PotSize))
                        {

                        }
                        else
                        {
                            BetSizePlayer = Math.Round(BetSizePlayer * 0.75, 2);

                        }
                        //BetSizePlayer = Math.Round(BetSizePlayer * 0.75, 2);
                    }
                }
            }
        }
        public static int GetOpponentID(ObservableCollection<PlayerViewModel> PlayersToPlay)
        {
            int index = -1;
            for (int i = 0; i < PlayersToPlay.Count; i++)
            {
                if (PlayersToPlay[i].Action == "RAISE" || PlayersToPlay[i].Action == "ALL IN" || PlayersToPlay[i].Action == "BET")
                {
                    index = i;
                }
            }
            return index;
        }
        public static double GetPotSizeRaisePerc(double pot, double betSize, double perc)
        {
            double potNext = 2 * betSize + pot;
            return betSize + perc * potNext;
        }
        /// <summary>
        /// This method serves to set Bet Button for human with values 0.5 or 2.5
        /// </summary>
        public static void BetBtn2(ref double BetPercent, ref HandState hs, ref EnumGameType gameType, ref EnumPhase phase, ref double BigBlind, ref double BetSizePlayer, ref ObservableCollection<PlayerViewModel> PlayersToPlay, ref int indexAct, ref double PotSize)
        {
            BetPercent = 0;
            double maxBet = GetHighestBet(hs);
            double restBets = hs.Bets.Sum() - maxBet - PlayersToPlay[indexAct].BetSize;
            if (phase == EnumPhase.Preflop && maxBet <= BigBlind)
            {
                BetPercent = 2.5;
                BetSizePlayer = Math.Round(BetPercent * maxBet, 2);
                if (gameType == EnumGameType.NLH)
                {
                    if (PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize < BetSizePlayer)
                    {
                        LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);

                    }

                }
                else
                {
                    if (PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize < BetSizePlayer)
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);

                    }
                }
                //if (gameType == EnumGameType.NLH)
                //{
                //    LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);
                //}
                //else
                //{
                //    LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                //}
            }
            else
            {

                var getOppId = GetOpponentID(PlayersToPlay);
                if (getOppId != -1)
                {
                    BetSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs, indexAct, getOppId, 0.5);
                    if (BetSizePlayer > PlayersToPlay[indexAct].BetSize + PlayersToPlay[indexAct].Balance)
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                    }
                    //BetSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs, indexAct, getOppId, 0.5);
                }
                else
                {
                    if (gameType == EnumGameType.NLH)
                    {
                        LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);
                    }
                    else
                    {

                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                        if (BetSizePlayer < PotSizeRaise(hs, PotSize))
                        {

                        }
                        else
                        {
                            BetSizePlayer = Math.Round(BetSizePlayer * 0.5, 2);

                        }
                        //BetSizePlayer = Math.Round(BetSizePlayer * 0.5, 2);
                    }
                }

            }
        }
        /// <summary>
        /// This method serves to set Bet Button for human with values 0.33 or 2
        /// </summary>
        public static void BetBtn1(ref double BetPercent, ref HandState hs, ref EnumGameType gameType, ref EnumPhase phase, ref double BigBlind, ref double BetSizePlayer, ref ObservableCollection<PlayerViewModel> PlayersToPlay, ref int indexAct, ref double PotSize)
        {
            double maxBet = GetHighestBet(hs);
            double restBets = hs.Bets.Sum() - maxBet - PlayersToPlay[indexAct].BetSize;
            if (phase == EnumPhase.Preflop && maxBet <= BigBlind)
            {
                BetPercent = 2;
                BetSizePlayer = Math.Round(BetPercent * maxBet, 2);
                if (gameType == EnumGameType.NLH)
                {
                    if (PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize < BetSizePlayer)
                    {
                        LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);

                    }

                }
                else
                {
                    if (PlayersToPlay[indexAct].Balance + PlayersToPlay[indexAct].BetSize < BetSizePlayer)
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);

                    }
                }
                //}
                //if (gameType == EnumGameType.NLH)
                //{
                //    LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);
                //}
                //else
                //{
                //    LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                //}
            }
            else
            {
                var getOppId = GetOpponentID(PlayersToPlay);
                if (getOppId != -1)
                {
                    BetSizePlayer = StrategyUtil.GetPotSizeRaisePerc(hs, indexAct, getOppId, 0.33);
                    if (BetSizePlayer > PlayersToPlay[indexAct].BetSize + PlayersToPlay[indexAct].Balance)
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                    }

                }
                else
                {
                    if (gameType == EnumGameType.NLH)
                    {
                        LimitBet(ref BetSizePlayer, PlayersToPlay, indexAct);
                    }
                    else
                    {
                        LimitPotSizeOmaha(PotSizeRaise(hs, PotSize), ref BetSizePlayer, PlayersToPlay, indexAct);
                        if (BetSizePlayer < PotSizeRaise(hs, PotSize))
                        {

                        }
                        else
                        {
                            BetSizePlayer = Math.Round(BetSizePlayer * 0.33, 2);

                        }


                    }
                }

            }
        }
        #endregion
    }
}
