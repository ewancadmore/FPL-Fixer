using System;
using System.Collections.Generic;
using System.Linq;
using static FantasyFootballFixer.Player;

namespace FantasyFootballFixer
{
    class Launcher
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path to the CSV file: ");
            string path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path)) path = "premier_final_finished.csv";

            List<Player> players = DataManager.LoadPlayers(path);

            var maxStats = Launcher.CalculateMaxStats(players);

            foreach (var p in players)
                p.SetMaxStats(maxStats);

            RunMenu(players);
        }

        static void RunMenu(List<Player> players)
        {
            while (true)
            {
                Console.WriteLine("=== Fantasy Football Fixer ===");
                Console.WriteLine("1. View all players");
                Console.WriteLine("2. Filter players");
                Console.WriteLine("3. Sort players");
                Console.WriteLine("4. Manually build a team");
                Console.WriteLine("5. Automatically build a team");
                Console.WriteLine("6. Search for a player");
                Console.WriteLine("7. Customise weights for score");
                Console.WriteLine("8. Exit");
                Console.Write("Choose an option: ");

                string input = Console.ReadLine();

                if (input == "1")
                {
                    ViewPlayers(players);
                }
                else if (input == "2")
                {
                    FilterPlayers(players);
                }
                else if (input == "3")
                {
                    SortPlayers(players);
                }
                else if (input == "4")
                {
                    BuildTeam(players);
                }
                else if (input == "5")
                {
                    AutoBuildTeam(players);
                }
                else if (input == "6")
                {
                    SearchPlayers(players);
                }
                else if (input == "7")
                {
                    CustomiseWeights(players);
                }
                else if (input == "8")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid option.");
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        public static void CustomiseWeights(List<Player>players)
        {
            Console.Write("Enter position to update (GK, DF, MF, FW): ");
            string pos = Console.ReadLine().ToUpper();

            if (!WeightManager.PositionWeights.ContainsKey(pos))
            {
                Console.WriteLine("Invalid position.");
                return;
            }

            foreach (var stat in WeightManager.PositionWeights[pos].Keys.ToList())
            {
                Console.Write($"Enter new weight for {stat} (current {WeightManager.PositionWeights[pos][stat]}): ");
                if (double.TryParse(Console.ReadLine(), out double w))
                {
                    WeightManager.UpdateWeight(pos, stat, w);
                }
            }

            foreach (var p in players)
            {
                p.ResetScore();
            }

            Console.WriteLine("Weights updated!");
        }

        static void ViewPlayers(List<Player> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                Console.WriteLine(players[i]);
            }
        }

        static void FilterPlayers(List<Player> players)
        {
            Dictionary<string, double> filters = new Dictionary<string, double>();
            List<Player> result = new List<Player>();

            List<string> validStats = new List<string> { "goals", "assists", "tackles", "xg", "xa", "saves", "goalsconceded", "price" };

            Console.WriteLine("Enter filters (type 'done' to finish)");
            Console.WriteLine("Important: Please note that g + a are recorded as per 90 stats (e.g. 0.5 not 5).");

            while (true)
            {
                Console.Write("Stat: ");
                string stat = Console.ReadLine().ToLower();

                if (stat == "done") break;

                if (!validStats.Contains(stat))
                {
                    Console.WriteLine("Invalid stat. Try again.");
                    continue;
                }

                Console.Write("Minimum value: ");
                double min = 0;
                double.TryParse(Console.ReadLine(), out min);

                filters[stat] = min;
            }

            for (int i = 0; i < players.Count; i++)
            {
                Player p = players[i];
                bool ok = true;

                foreach (var f in filters)
                {
                    double value = GetStat(p, f.Key);

                    if (value == -1)
                    {
                        Console.WriteLine($"Invalid value: {f.Key}");
                        ok = false;
                        break;
                    }

                    if (value < f.Value)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    result.Add(p);
                }
            }

            Console.WriteLine("Found " + result.Count + " players");

            for (int i = 0; i < result.Count; i++)
            {
                Console.WriteLine(result[i]);
            }
        }

        static string NormaliseString(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("à", "a").Replace("è", "e").Replace("ì", "i").Replace("ò", "o").Replace("ù", "u")
                .Replace("â", "a").Replace("ê", "e").Replace("î", "i").Replace("ô", "o").Replace("û", "u")
                .Replace("ä", "a").Replace("ë", "e").Replace("ï", "i").Replace("ö", "o").Replace("ü", "u")
                .Replace("ã", "a").Replace("õ", "o").Replace("ñ", "n").Replace("ç", "c")
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
                .Replace("À", "A").Replace("È", "E").Replace("Ì", "I").Replace("Ò", "O").Replace("Ù", "U")
                .Replace("Â", "A").Replace("Ê", "E").Replace("Î", "I").Replace("Ô", "O").Replace("Û", "U")
                .Replace("Ä", "A").Replace("Ë", "E").Replace("Ï", "I").Replace("Ö", "O").Replace("Ü", "U")
                .Replace("Ã", "A").Replace("Õ", "O").Replace("Ñ", "N").Replace("Ç", "C")
                .Replace("š", "s").Replace("ž", "z").Replace("Š", "S").Replace("Ž", "Z")
                .ToLower();
        }

        static double GetStat(Player p, string stat)
        {
            if (stat == "goals") return p.Goals;
            if (stat == "assists") return p.Assists;
            if (stat == "tackles") return p.Tackles;
            if (stat == "xg") return p.xG;
            if (stat == "xa") return p.xA;
            if (stat == "saves") return p.Saves;
            if (stat == "goalsconceded") return p.GoalsConceded;
            if (stat == "price") return p.Price;
            return -1;
        }

        static void SortPlayers(List<Player> players)
        {
            Console.WriteLine("\nChoose a sorting option:");
            Console.WriteLine("1. Sort by Score");
            Console.WriteLine("2. Sort by Price");
            Console.WriteLine("3. Sort by Goals");
            Console.WriteLine("4. Sort by Assists");
            Console.WriteLine("5. Sort by Saves");
            Console.WriteLine("6. Sort Alphabetically (A–Z)");

            Console.Write("Enter choice: ");
            string choice = Console.ReadLine();
            Console.WriteLine();

            Console.WriteLine("What sorting algorithm do you want to use:");
            Console.WriteLine("1. Bubble Sort");
            Console.WriteLine("2. Merge Sort");
            Console.Write("Enter algorithm: ");
            string chosensort = Console.ReadLine();

            if (chosensort == "1")
            {
                BubbleSort(players, choice);
            }
            else if (chosensort == "2")
            {
                MergeSort(players, choice);
            }
            else
            {
                Console.WriteLine("No algorithm selected.");
                return;
            }

            Console.WriteLine("Players sorted.\n");

            for (int i = 0; i < players.Count && i < 20; i++)
            {
                Console.WriteLine(players[i]);
            }
        }

        static bool ComesBefore(Player a, Player b, string choice)
        {
            if (choice == "1") return a.GetScore() > b.GetScore();      
            if (choice == "2") return a.Price < b.Price;                
            if (choice == "3") return a.Goals > b.Goals;                
            if (choice == "4") return a.Assists > b.Assists;            
            if (choice == "5") return a.Saves > b.Saves;                
            if (choice == "6") return string.Compare(a.Name, b.Name) < 0; 
            return true;
        }

        static void BubbleSort(List<Player> players, string choice)
        {
            for (int i = 0; i < players.Count - 1; i++)
            {
                for (int j = 0; j < players.Count - i - 1; j++)
                {
                    if (!ComesBefore(players[j], players[j + 1], choice))
                    {
                        Player temp = players[j];
                        players[j] = players[j + 1];
                        players[j + 1] = temp;
                    }
                }
            }
        }

        static void MergeSort(List<Player> players, string choice)
        {
            if (players.Count <= 1) return;

            int mid = players.Count / 2;
            List<Player> left = players.GetRange(0, mid);
            List<Player> right = players.GetRange(mid, players.Count - mid);

            MergeSort(left, choice);
            MergeSort(right, choice);

            Merge(players, left, right, choice);
        }

        static void Merge(List<Player> players, List<Player> left, List<Player> right, string choice)
        {
            int i = 0, j = 0, k = 0;

            while (i < left.Count && j < right.Count)
            {
                if (ComesBefore(left[i], right[j], choice))
                {
                    players[k++] = left[i++];
                }
                else
                {
                    players[k++] = right[j++];
                }
            }

            while (i < left.Count) players[k++] = left[i++];
            while (j < right.Count) players[k++] = right[j++];
        }

        static void SearchPlayers(List<Player> players)
    {
        Console.Write("Enter a name to search: ");
            string name = NormaliseString(Console.ReadLine());

        List<Player> matches = new List<Player>();

            for (int i = 0; i < players.Count; i++)
            {
                string normalizedPlayerName = NormaliseString(players[i].Name);
                if (normalizedPlayerName.Contains(name))
                {
                    matches.Add(players[i]);
                }
            }

            if (matches.Count == 0)
        {
            Console.WriteLine("No players found.");
        }
        else
        {
            Console.WriteLine("Found " + matches.Count + " players:");
            for (int i = 0; i < matches.Count; i++)
            {
                Console.WriteLine(matches[i]);
            }
        }
    }

        static Dictionary<string, Dictionary<string, double>> CalculateMaxStats(List<Player> players)
        {
            Dictionary<string, Dictionary<string, double>> maxStats = new Dictionary<string, Dictionary<string, double>>();

            string[] positions = { "GK", "DF", "MF", "FW" };
            string[] stats = { "Goals", "Assists", "xG", "xA", "Tackles", "Saves", "GoalsConceded" };

            foreach (string pos in positions)
            {
                maxStats[pos] = new Dictionary<string, double>();
                foreach (string stat in stats)
                {
                    double max = 0;
                    foreach (Player p in players)
                    {
                        if (!p.Position.ToUpper().Contains(pos)) continue;

                        double value = 0;
                        if (stat == "Goals") value = p.Goals;
                        else if (stat == "Assists") value = p.Assists;
                        else if (stat == "xG") value = p.xG;
                        else if (stat == "xA") value = p.xA;
                        else if (stat == "Tackles") value = p.Tackles;
                        else if (stat == "Saves") value = p.Saves;
                        else if (stat == "GoalsConceded") value = p.GoalsConceded;

                        if (value > max) max = value;
                    }
                    maxStats[pos][stat] = Math.Max(max, 1);
                }
            }

            return maxStats;
        }


        static void AutoBuildTeam(List<Player> players)
        {
            Team t = new Team();
            int gkCount = 0, dfCount = 0, mfCount = 0, fwCount = 0;

            List<Player> gks = players.Where(p => p.Position.ToUpper().Contains("GK")).OrderByDescending(p => p.GetScore()).ToList();
            List<Player> dfs = players.Where(p => p.Position.ToUpper().Contains("DF")).OrderByDescending(p => p.GetScore()).ToList();
            List<Player> mfs = players.Where(p => p.Position.ToUpper().Contains("MF")).OrderByDescending(p => p.GetScore()).ToList();
            List<Player> fws = players.Where(p => p.Position.ToUpper().Contains("FW")).OrderByDescending(p => p.GetScore()).ToList();

            List<Player> SelectBestPlayers(List<Player> options, int maxCount, double allocatedBudget)
            {
                int n = options.Count;
                double[][] ValueperCost = new double[n + 1][];
                for (int i = 0; i <= n; i++)
                {
                    ValueperCost[i] = new double[(int)allocatedBudget + 1];
                }

                List<Player>[][] selected = new List<Player>[n + 1][];
                for (int i = 0; i <= n; i++)
                {
                    selected[i] = new List<Player>[(int)allocatedBudget + 1];
                    for (int j = 0; j <= (int)allocatedBudget; j++) selected[i][j] = new List<Player>();
                }

                for (int i = 1; i <= n; i++)
                {
                    Player p = options[i - 1];
                    for (int j = 0; j <= (int)allocatedBudget; j++)
                    {
                        if (p.Price <= j && selected[i - 1][j - (int)p.Price].Count < maxCount)
                        {
                            double newScore = ValueperCost[i - 1][j - (int)p.Price] + p.GetScore();
                            if (newScore > ValueperCost[i - 1][j])
                            {
                                ValueperCost[i][j] = newScore;
                                selected[i][j] = new List<Player>(selected[i - 1][j - (int)p.Price]) { p };
                            }
                            else
                            {
                                ValueperCost[i][j] = ValueperCost[i - 1][j];
                                selected[i][j] = new List<Player>(selected[i - 1][j]);
                            }
                        }
                        else
                        {
                            ValueperCost[i][j] = ValueperCost[i - 1][j];
                            selected[i][j] = new List<Player>(selected[i - 1][j]);
                        }
                    }
                }

                return selected[n][(int)allocatedBudget];
            }

            void AddPlayers(List<Player> candidates, ref int count, int max, double allocatedBudget)
            {
                List<Player> optimal = SelectBestPlayers(candidates, max, allocatedBudget);
                foreach (var p in optimal)
                {
                    if (count >= max || t.Players.Count >= 15) break;
                    t.AddPlayer(p);
                    count++;
                }

                if (count < max)
                {
                    List<Player> remaining = candidates.Where(p => !optimal.Contains(p)).OrderByDescending(p => p.GetScore()).ToList();
                    foreach (var p in remaining)
                    {
                        if (count >= max || t.Players.Count >= 15 || t.TotalCost() + p.Price > t.Budget) break;
                        t.AddPlayer(p);
                        count++;
                    }
                }
            }

            double gkBudget = 10;
            double dfBudget = 30;
            double mfBudget = 30;
            double fwBudget = 30;

            AddPlayers(gks, ref gkCount, 2, gkBudget);
            AddPlayers(dfs, ref dfCount, 5, dfBudget);
            AddPlayers(mfs, ref mfCount, 5, mfBudget);
            AddPlayers(fws, ref fwCount, 3, fwBudget);

            Console.WriteLine("Team successfully built.");
            foreach (var p in t.Players)
                Console.WriteLine(p.Name + " " + p.Position + " £" + p.Price + " Score: " + p.GetScore());

            Console.WriteLine("Total cost: " + t.TotalCost());
            Console.WriteLine("Average score: " + t.AverageScore());

            Console.Write("Do you want to save this team? (y/n): ");
            string save = Console.ReadLine().ToLower();

            if (save == "y")
            {
                Console.Write("Enter file name: ");
                string file = Console.ReadLine();

                try
                {
                    using (StreamWriter sw = new StreamWriter(file))
                    {
                        foreach (var p in t.Players)
                        {
                            sw.WriteLine(p);
                        }
                    }
                    Console.WriteLine("Saved successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error saving file " + ex.Message);
                }
            }
        }


        static void BuildTeam(List<Player> players)
        {
            Shortlist shortlist = new Shortlist();
            Team team = new Team();
            bool building = true;

            int gkCount = 0, dfCount = 0, mfCount = 0, fwCount = 0;
            int totalPlayers = 0;

            while (building)
            {
                Console.WriteLine("\nChoose an option:");
                Console.WriteLine("1. Add player to shortlist");
                Console.WriteLine("2. Remove player from shortlist");
                Console.WriteLine("3. Display shortlist");
                Console.WriteLine("4. Save shortlist");
                Console.WriteLine("5. Finalise team from shortlist");
                Console.WriteLine("6. Exit");
                Console.Write("Enter choice: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        Console.Write("Enter player name to add: ");
                        string addName = NormaliseString(Console.ReadLine());
                        Player addPlayer = players.FirstOrDefault(p => NormaliseString(p.Name).Contains(addName));
                        if (addPlayer != null)
                        {
                            string pos = addPlayer.Position.ToUpper();
                            bool canAdd = true;
                            string reason = "";

                            if (totalPlayers >= 15)
                            {
                                canAdd = false;
                                reason = "Shortlist already has 15 players (max allowed).";
                            }
                            else if (pos.Contains("GK") && gkCount >= 2)
                            {
                                canAdd = false;
                                reason = "Already have 2 GK in shortlist.";
                            }
                            else if (pos.Contains("DF") && dfCount >= 5)
                            {
                                canAdd = false;
                                reason = "Already have 5 DF in shortlist.";
                            }
                            else if (pos.Contains("MF") && mfCount >= 5)
                            {
                                canAdd = false;
                                reason = "Already have 5 MF in shortlist.";
                            }
                            else if (pos.Contains("FW") && fwCount >= 3)
                            {
                                canAdd = false;
                                reason = "Already have 3 FW in shortlist.";
                            }

                            if (canAdd)
                            {
                                shortlist.Add(addPlayer);
                                totalPlayers++;
                                if (pos.Contains("GK")) gkCount++;
                                else if (pos.Contains("DF")) dfCount++;
                                else if (pos.Contains("MF")) mfCount++;
                                else if (pos.Contains("FW")) fwCount++;
                                Console.WriteLine($"Player {addPlayer.Name} added to shortlist.");
                            }
                            else
                            {
                                Console.WriteLine($"Cannot add {addPlayer.Name}: {reason}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Player not found.");
                        }
                        break;

                    case "2":
                        Console.Write("Enter player name to remove: ");
                        string nametoRemove = NormaliseString(Console.ReadLine()); 
                        Player removePlayer = shortlist.GetPlayers().FirstOrDefault(p => NormaliseString(p.Name).Contains(nametoRemove));
                        if (removePlayer != null)
                        {
                            shortlist.Remove(removePlayer);
                            totalPlayers--;
                            string pos = removePlayer.Position.ToUpper();
                            if (pos.Contains("GK")) gkCount--;
                            else if (pos.Contains("DF")) dfCount--;
                            else if (pos.Contains("MF")) mfCount--;
                            else if (pos.Contains("FW")) fwCount--;
                            Console.WriteLine($"Player {removePlayer.Name} removed from shortlist.");
                        }
                        else
                        {
                            Console.WriteLine("Player not found in shortlist.");
                        }
                        break;

                    case "3":
                        shortlist.Print();
                        break;

                    case "4":
                        Console.Write("Enter filename to save shortlist: ");
                        string file = Console.ReadLine();
                        shortlist.Export(file);
                        break;

                    case "5":
                        int finalGkCount = 0, finalDfCount = 0, finalMfCount = 0, finalFwCount = 0;
                        foreach (var p in shortlist.GetPlayers())
                        {
                            string pos = p.Position.ToUpper();
                            if (team.TotalCost() + p.Price > team.Budget)
                                continue;

                            if (pos.Contains("GK") && finalGkCount < 2)
                            {
                                team.AddPlayer(p);
                                finalGkCount++;
                            }
                            else if (pos.Contains("DF") && finalDfCount < 5)
                            {
                                team.AddPlayer(p);
                                finalDfCount++;
                            }
                            else if (pos.Contains("MF") && finalMfCount < 5)
                            {
                                team.AddPlayer(p);
                                finalMfCount++;
                            }
                            else if (pos.Contains("FW") && finalFwCount < 3)
                            {
                                team.AddPlayer(p);
                                finalFwCount++;
                            }
                        }

                        Console.WriteLine("\n=== Final Team ===");
                        foreach (var p in team.Players)
                            Console.WriteLine(p);
                        Console.WriteLine($"Total cost: {team.TotalCost()}");
                        Console.WriteLine($"Average score: {team.AverageScore()}");
                        building = false;
                        break;

                    case "6":
                        building = false;
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
    }
}
