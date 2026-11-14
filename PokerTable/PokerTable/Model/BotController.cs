using DBLib;
using DecisionMaking;
using HandStateLib;
using PokerTable.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerTable.Model
{
    public class BotController
    {
        public static HandState GetHandState(ref int numberOfPlayers, ref int indexAct, ref ObservableCollection<PlayerViewModel> playersToPlay, ref EnumPhase phase, ref ObservableCollection<string> cardsOnTable, ref EnumGameType gameType, ref double potSize, ref string handId)
        {
            HandState hs = new HandState(numberOfPlayers);
            try
            {
                int playerIndex = indexAct;

                hs.Bets = new List<double>();
                hs.Names = new List<string>();
                hs.Stacks = new List<double>();
                hs.InGame = new List<bool>();
                foreach (var item in playersToPlay)
                {
                    hs.Bets.Add(item.BetSize);
                    hs.Names.Add(item.Name);
                    hs.Stacks.Add(item.Balance);
                    hs.InGame.Add(item.InGame);
                }
                hs.Cards[0] = playersToPlay[playerIndex].Cards.Substring(0, 2);
                hs.Cards[1] = playersToPlay[playerIndex].Cards.Substring(2, 2);
                if (phase == EnumPhase.Preflop)
                {
                    hs.Cards[2] = "";
                    hs.Cards[3] = "";
                    hs.Cards[4] = "";
                    hs.Cards[5] = "";
                    hs.Cards[6] = "";
                }
                else if (phase == EnumPhase.Flop)
                {
                    hs.Cards[2] = cardsOnTable[0];
                    hs.Cards[3] = cardsOnTable[1];
                    hs.Cards[4] = cardsOnTable[2];
                }
                else if (phase == EnumPhase.Turn)
                {
                    hs.Cards[2] = cardsOnTable[0];
                    hs.Cards[3] = cardsOnTable[1];
                    hs.Cards[4] = cardsOnTable[2];
                    hs.Cards[5] = cardsOnTable[3];
                }
                else
                {
                    hs.Cards[2] = cardsOnTable[0];
                    hs.Cards[3] = cardsOnTable[1];
                    hs.Cards[4] = cardsOnTable[2];
                    hs.Cards[5] = cardsOnTable[3];
                    hs.Cards[6] = cardsOnTable[4];
                }
                if (gameType == EnumGameType.PLO4)
                {

                    hs.OmahaCards = new List<string>() { playersToPlay[playerIndex].Cards.Substring(0, 2).ToLower(),
                                                         playersToPlay[playerIndex].Cards.Substring(2, 2).ToLower(),
                                                         playersToPlay[playerIndex].Cards.Substring(4, 2).ToLower(),
                                                         playersToPlay[playerIndex].Cards.Substring(6, 2).ToLower() };


                }
                for (int i = 0; i < playersToPlay.Count; i++)
                {
                    if (playersToPlay[i].IsDealer == true)
                    {
                        hs.DealerID = i;
                    }
                }
                hs.HandID = handId;
                hs.OriginalNames = new List<string>(hs.Names);
                hs.Pot = potSize;
            }
            catch (Exception ex)
            {
                Singleton.Instance.Log("Exception in GetHandState from BotController: " + " " + ex.ToString());
            }
            return hs;
        }
    }
}
