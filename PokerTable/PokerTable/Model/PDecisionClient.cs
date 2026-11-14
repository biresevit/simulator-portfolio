using DBLib;
using DecisionMaking;
using HandStateLib;
using PokerSuite.Shared.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PokerTable.Model
{
    /// <summary>
    /// Talks to the PDecision REST API like a real bot client: every bot gets its decisions from POST /api/decision and
    /// sends its own view of the table to POST /api/handhistory after every change. It also records the true order of
    /// actions per hand (expected.jsonl) to check the hand histories PDecision saves.
    /// </summary>
    public class PDecisionClient
    {
        private static readonly HttpClient http = new()
        {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("PDECISION_URL") ?? "http://127.0.0.1:5005"),
            Timeout = TimeSpan.FromSeconds(60),
        };

        private readonly object sync = new();
        private readonly Dictionary<string, string> lastHandPerBot = new();
        private readonly Dictionary<string, string> lastFramePerBot = new();
        private readonly List<object> currentActions = new();
        private string currentHandId;

        // one table id per simulator session, so PDecision starts with fresh per table state
        public string TableId { get; } = "sim-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        public string ExpectedFile { get; }
        public string SkippedFile { get; }

        public int Decisions { get; private set; }
        public int DecisionErrors { get; private set; }
        public int Frames { get; private set; }
        public int FramesSkipped { get; private set; }
        public int FrameErrors { get; private set; }
        public int HandsFinished { get; private set; }
        public string LastError { get; private set; } = "";

        public PDecisionClient()
        {
            string dir = Path.Combine(Path.GetTempPath(), "pokertable_rest");
            Directory.CreateDirectory(dir);
            ExpectedFile = Path.Combine(dir, TableId + "_expected.jsonl");
            SkippedFile = Path.Combine(dir, TableId + "_skipped.txt");
        }

        public string Status =>
            $"PDecision REST {http.BaseAddress}: {Decisions} decisions ({DecisionErrors} errors), {Frames} hand-history frames " +
            $"({FramesSkipped} skipped, {FrameErrors} errors), {HandsFinished} hands" + (LastError == "" ? "" : $" | last error: {LastError}");

        /// <summary>
        /// Decision for the bot at seat 0 of the rotated hand state, or null if PDecision did not answer with a decision.
        /// </summary>
        public Decision GetDecision(HandState rotated, double bigBlind, double smallBlind, EnumGameType gameType)
        {
            Fill(rotated, bigBlind, smallBlind, gameType);
            rotated.IsMyTurn = true;
            var (status, body) = Post("/api/decision", rotated);
            lock (sync)
            {
                Decisions++;
                if (status != 200)
                {
                    DecisionErrors++;
                    LastError = $"decision HTTP {status}: {Short(body)}";
                    Singleton.Instance.Log($"PDecision REST {LastError}", LogLevel.Error);
                    return null;
                }
            }
            return Decision.Deserialize(body);
        }

        /// <summary>
        /// Sends one bot's view of the table to the hand-history feed.
        /// </summary>
        public void PostFrame(HandState rotated, double bigBlind, double smallBlind, EnumGameType gameType)
        {
            Fill(rotated, bigBlind, smallBlind, gameType);
            rotated.IsMyTurn = false;
            string bot = rotated.OriginalNames[0];
            lock (sync)
            {
                // the first frame of each hand per bot starts the hand
                rotated.IsNewHand = !lastHandPerBot.TryGetValue(bot, out string lastHand) || lastHand != rotated.HandID;
                lastHandPerBot[bot] = rotated.HandID;
            }

            string frame = HandState.Serialize(rotated);
            var (status, body) = Post("/api/handhistory", rotated);
            lock (sync)
            {
                lastFramePerBot.TryGetValue(bot, out string previous);
                lastFramePerBot[bot] = frame;
                Frames++;
                if (status != 200)
                {
                    FrameErrors++;
                    LastError = $"handhistory HTTP {status}: {Short(body)}";
                    Singleton.Instance.Log($"PDecision REST {LastError}", LogLevel.Error);
                }
                else if (body.Contains("\"accepted\":false"))
                {
                    FramesSkipped++;
                    // the frame and the bot's previous frame, to find out what PDecision found inconsistent
                    File.AppendAllText(SkippedFile, $"=== {bot} hand {rotated.HandID}: {body}{Environment.NewLine}previous: {previous}{Environment.NewLine}current:  {frame}{Environment.NewLine}");
                }
            }
        }

        /// <summary>
        /// Records an action as it happened on the table (folds / checks / calls / bets / raises), in order.
        /// </summary>
        public void RecordAction(string handId, string player, string action, string phase)
        {
            lock (sync)
            {
                if (currentHandId != handId)
                {
                    currentActions.Clear();
                    currentHandId = handId;
                }
                currentActions.Add(new[] { player, action, phase });
            }
        }

        /// <summary>
        /// Writes the recorded actions, players and board of the finished hand to expected.jsonl.
        /// </summary>
        public void FinishHand(string handId, IEnumerable<string> players, IEnumerable<string> board)
        {
            lock (sync)
            {
                if (currentHandId != handId || currentActions.Count == 0)
                {
                    return;
                }
                var record = new
                {
                    table = TableId,
                    hand_id = handId,
                    players = players.Where(p => !string.IsNullOrEmpty(p)).OrderBy(p => p).ToList(),
                    actions = currentActions.ToList(),
                    board = board.Where(c => !string.IsNullOrEmpty(c)).Select(c => c.ToLower()).ToList(),
                };
                File.AppendAllText(ExpectedFile, JsonSerializer.Serialize(record) + Environment.NewLine);
                currentActions.Clear();
                currentHandId = null;
                HandsFinished++;
            }
        }

        private void Fill(HandState hs, double bigBlind, double smallBlind, EnumGameType gameType)
        {
            hs.TableID = TableId;
            hs.TableName = TableId;
            hs.BBSize = bigBlind;
            hs.SBSize = smallBlind;
            hs.Straddle = 0;
            hs.Ante = 0;
            hs.TableType = enTableType.Cash;
            hs.GameType = gameType switch
            {
                EnumGameType.PLO4 => enGameType.PLO4,
                EnumGameType.PLO5 => enGameType.PLO5,
                _ => enGameType.NLH,
            };
            // the table shows "Ts"; clients send cards in lower case as in the integration guide
            hs.Cards = hs.Cards.Select(c => (c ?? "").ToLower()).ToList();
            hs.OmahaCards = hs.OmahaCards?.Select(c => (c ?? "").ToLower()).ToList() ?? new List<string>();
            hs.Showdowns = hs.Showdowns?.Select(c => (c ?? "").ToLower()).ToList() ?? new List<string>();
            if (gameType == EnumGameType.NLH)
            {
                hs.OmahaCards = new List<string>();
            }
            else
            {
                hs.Cards[0] = "";
                hs.Cards[1] = "";
            }
        }

        private static (int, string) Post(string path, HandState hs)
        {
            try
            {
                using var content = new StringContent(HandState.Serialize(hs), Encoding.UTF8, "application/json");
                using var reply = http.PostAsync(path, content).GetAwaiter().GetResult();
                return ((int)reply.StatusCode, reply.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                return (0, ex.Message);
            }
        }

        private static string Short(string text)
        {
            return text.Length > 160 ? text.Substring(0, 160) : text;
        }
    }
}
