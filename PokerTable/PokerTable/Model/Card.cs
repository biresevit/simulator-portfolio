using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PokerTable.Model
{
    public class Card
    {
        public static Random r = new Random();
        public static List<string> Deck()
        {
            List<string> cards = new List<string>();
            string[] arrayCards = { "2s", "2d", "2h", "2c", "3s", "3d", "3h", "3c", "4s", "4d", "4h", "4c", "5s", "5d", "5h", "5c", "6s", "6d", "6h", "6c", "7s", "7d", "7h", "7c", "8s", "8d", "8h", "8c", "9s", "9d", "9h", "9c", "Ts", "Td", "Th", "Tc", "Js", "Jd", "Jh", "Jc", "Qs", "Qd", "Qh", "Qc", "Ks", "Kd", "Kh", "Kc", "As", "Ad", "Ah", "Ac" };
            foreach (string item in arrayCards)
            {
                cards.Add(item);
            }
            return cards;
        }
        public static List<string> RandomCards(int numCards,ref List<string> CardsDeck)
        {
            List<string> listOfRandomCards = new List<string>();
            string card = "";
            for (int i = 0; i < numCards; i++)
            {
                int random = r.Next(0, CardsDeck.Count);
                card = CardsDeck[random];
                listOfRandomCards.Add(card);
                CardsDeck.Remove(card);
            }
            return listOfRandomCards;
        }
    }
}
