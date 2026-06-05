using System;
using System.Collections.Generic;
using System.IO;

namespace FantasyFootballFixer
{
    public class Player
    {
        public string Name;
        public string Team;
        public double Price;
        public string Position;

        public double Goals;
        public double Assists;
        public double xG;
        public double xA;
        public double Tackles;
        public double Saves;
        public double GoalsConceded;

        private double normalisedScore = -1;

        private Dictionary<string, Dictionary<string, double>> maxStatsRef;

        public void SetMaxStats(Dictionary<string, Dictionary<string, double>> m)
        {
            maxStatsRef = m;
            normalisedScore = -1;
        }

        public void ResetScore()
        {
            normalisedScore = -1;
        }

        public Player(string name, string team, double price, string position)
        {
            Name = name;
            Team = team;
            Price = price;
            Position = position;
        }

        public static class WeightManager
        {
            public static Dictionary<string, Dictionary<string, double>> PositionWeights = new Dictionary<string, Dictionary<string, double>>()
            {
                ["FW"] = new Dictionary<string, double> { ["Goals"] = 0.4, ["Assists"] = 0.15, ["xG"] = 0.25, ["xA"] = 0.1, ["Tackles"] = 0.1, ["Saves"] = 0, ["GoalsConceded"] = 0 },
                ["MF"] = new Dictionary<string, double> { ["Goals"] = 0.25, ["Assists"] = 0.3, ["xG"] = 0.15, ["xA"] = 0.2, ["Tackles"] = 0.1, ["Saves"] = 0, ["GoalsConceded"] = 0 },
                ["DF"] = new Dictionary<string, double> { ["Goals"] = 0.1, ["Assists"] = 0.15, ["xG"] = 0.05, ["xA"] = 0.05, ["Tackles"] = 0.35, ["Saves"] = 0, ["GoalsConceded"] = 0.0 },
                ["GK"] = new Dictionary<string, double> { ["Goals"] = 0, ["Assists"] = 0, ["xG"] = 0, ["xA"] = 0, ["Tackles"] = 0, ["Saves"] = 0.4, ["GoalsConceded"] = 0.6 }
            };

            public static void UpdateWeight(string pos, string stat, double newWeight)
            {
                pos = pos.ToUpper();
                if (PositionWeights.ContainsKey(pos) && PositionWeights[pos].ContainsKey(stat))
                    PositionWeights[pos][stat] = newWeight;
            }

            public static Dictionary<string, double> GetWeightsForPosition(string pos)
            {
                pos = pos.ToUpper();
                if (PositionWeights.ContainsKey(pos))
                    return PositionWeights[pos];
                return new Dictionary<string, double>();
            }
        }

        public class Shortlist
        {
            private LinkedList<Player> players = new LinkedList<Player>();

            public void Add(Player p)
            {
                if (!players.Contains(p))
                    players.AddLast(p);
            }

            public void Remove(Player p)
            {
                players.Remove(p);
            }

            public void Print()
            {
                Console.WriteLine("=== Shortlist ===");
                foreach (var p in players)
                    Console.WriteLine(p);
                Console.WriteLine("================");
            }

            public void Export(string file)
            {
                using (StreamWriter sw = new StreamWriter(file))
                {
                    foreach (var p in players)
                    {
                        sw.WriteLine(p);
                    }
                }
                Console.WriteLine("Team exported to " + file);
            }


            public LinkedList<Player> GetPlayers()
            {
                return players;
            }
        }

        public double GetScore()
        {
            if (Goals == 0 && Assists == 0 && xG == 0 && xA == 0 && Tackles == 0 && Saves == 0 && GoalsConceded == 0)
            {
                normalisedScore = 0;
                return normalisedScore;
            }

            if (normalisedScore >= 0)
            {
                return normalisedScore;
            }

            double score = 0;
            string pos = Position.ToUpper();
            Dictionary<string, double> weights = WeightManager.GetWeightsForPosition(pos);

            if (maxStatsRef == null || !maxStatsRef.ContainsKey(pos))
            {
                Console.WriteLine($"Warning: maxStatsRef not set for {Name} ({pos}). Scores may be incorrect.");
                foreach (var stat in weights.Keys)
                {
                    double value = 0;
                    switch (stat)
                    {
                        case "Goals": value = Goals; break;
                        case "Assists": value = Assists; break;
                        case "xG": value = xG; break;
                        case "xA": value = xA; break;
                        case "Tackles": value = Tackles; break;
                        case "Saves": value = Saves; break;
                        case "GoalsConceded": value = GoalsConceded; break; 
                    }

                    score += value * weights[stat] * 10;
                }
                normalisedScore = Math.Min(100, Math.Max(0, (int)Math.Round(score)));
                return normalisedScore;
            }

            foreach (var stat in weights.Keys)
            {
                double value = 0;
                double max;

                if (maxStatsRef[pos].ContainsKey(stat))
                {
                    max = maxStatsRef[pos][stat];
                }
                else
                {
                    max = 1;
                }

                if (max == 0) max = 1;  

                switch (stat)
                {
                    case "Goals": value = Goals; break;
                    case "Assists": value = Assists; break;
                    case "xG": value = xG; break;
                    case "xA": value = xA; break;
                    case "Tackles": value = Tackles; break;
                    case "Saves": value = Saves; break;
                    case "GoalsConceded":
                        if (GoalsConceded > 0)
                        {
                            value = Math.Max(0, max - GoalsConceded);
                        }
                        else
                        {
                            value = 0;
                        }

                        break;
                }

                double normalised;

                if (max > 0)
                {
                    normalised = Math.Min(100, (value / max) * 100);
                }
                else
                {
                    normalised = 0;
                }

                score += normalised * weights[stat];
            }

            normalisedScore = Math.Min(100, Math.Max(0, (int)Math.Round(score)));
            return normalisedScore;
        }


        public override string ToString()
{
    return Name + " (" + Team + ") - " + Position + 
           " | Price: £" + Price + "m" + 
           " | Score: " + GetScore();
}

    }

    public class Team
    {
        public List<Player> Players = new List<Player>();
        public double Budget = 100;

        public void AddPlayer(Player p)
        {
            if (Players.Count < 15 && TotalCost() + p.Price <= Budget)
            {
                Players.Add(p);
            }
        }

        public double TotalCost()
        {
            double total = 0;

            for (int i = 0; i < Players.Count; i++)
            {
                total += Players[i].Price;
            }

            return total;
        }

        public double AverageScore()
        {
            if (Players.Count == 0) return 0;

            double total = 0;

            for (int i = 0; i < Players.Count; i++)
            {
                total += Players[i].GetScore();
            }

            return total / Players.Count;
        }
    }

    public static class DataManager
    {
        private static double SafeParse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            value = value.Replace("%", "");

            double result = 0;
            double.TryParse(value, out result);
            return result;
        }

        public static List<Player> LoadPlayers(string path)
        {
            List<Player> players = new List<Player>();

            if (!File.Exists(path))
            {
                Console.WriteLine("No file found, please try again.");
            }

            string[] lines = File.ReadAllLines(path);

            if (lines.Length < 2)
            {
                return players;
            }

            string[] headers = lines[0].Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                string[] row = lines[i].Split(',');

                if (row.Length < headers.Length)
                {
                    continue;
                }

                string team = row[0];
                string name = row[1];
                string position = row[3];

                position = position.ToUpper().Trim(); 
                if (position.Contains("FW")) position = "FW";
                else if (position.Contains("MF")) position = "MF";
                else if (position.Contains("DF")) position = "DF";
                else if (position.Contains("GK")) position = "GK";

                double goals = SafeParse(row[Array.IndexOf(headers, "GoalsPer90")]);
                double assists = SafeParse(row[Array.IndexOf(headers, "AssistsPer90")]);
                double xg = SafeParse(row[Array.IndexOf(headers, "xG")]);
                double xa = SafeParse(row[Array.IndexOf(headers, "xA")]);
                double tackles = SafeParse(row[Array.IndexOf(headers, "Tackles")]);
                double saves = SafeParse(row[Array.IndexOf(headers, "Save%")]) / 100.0;
                double conceded = SafeParse(row[Array.IndexOf(headers, "GoalsConceded")]);

                string pos = position.ToUpper();

                double basePrice = 0;
                int count = 0;

                if (pos.Contains("GK")) { basePrice += 4.0; count++; }
                if (pos.Contains("DF")) { basePrice += 4.0; count++; }
                if (pos.Contains("MF")) { basePrice += 4.0; count++; }
                if (pos.Contains("FW")) { basePrice += 4.0; count++; }

                if (count > 0) basePrice /= count;
                else basePrice = 5.0;

                double price = basePrice;

                if (pos.Contains("FW"))
                    price += (goals * 0.3) + (assists * 0.15) + (xg * 0.2);
                else if (pos.Contains("MF"))
                    price += (goals * 0.2) + (assists * 0.2) + (xg * 0.15) + (xa * 0.15);
                else if (pos.Contains("DF"))
                    price += (goals * 0.1) + (assists * 0.1) + (tackles * 0.05);
                else if (pos.Contains("GK"))
                    price += (saves * 0.2) - (conceded * 0.1);

                price = Math.Round(Math.Max(4.0, Math.Min(price, 12.0)), 1);

                Player p = new Player(name, team, price, position);
                p.Goals = goals;
                p.Assists = assists;
                p.xG = xg;
                p.xA = xa;
                p.Tackles = tackles;
                p.Saves = saves;
                p.GoalsConceded = conceded;

                players.Add(p);

            }

            return players;
        }
    }

}
