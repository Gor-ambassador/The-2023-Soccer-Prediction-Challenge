using Newtonsoft.Json;
using ScoreRegression;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Wolfram.NETLink;

namespace Testing
{
    internal class Program
    {
        /// <summary>
        /// Method for determining the probabilities of outcomes based on the parameters of the Poisson distribution
        /// </summary>
        /// <param name="lyamb1">Parameter of Home team</param>
        /// <param name="lyamb2">Parameter of Away team</param>
        /// <param name="pWin"></param>
        /// <param name="pDraw"></param>
        /// <param name="pLose"></param>
        static void findProb(double lyamb1, double lyamb2, out double pWin, out double pDraw, out double pLose)
        {
            pWin = 0;
            pDraw = 0;
            pLose = 0;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    double stepResult = PoissonOne(lyamb1, i) * PoissonOne(lyamb2, j);
                    
                    if (i < j)
                        pLose += stepResult;
                    else if (i == j)
                        pDraw += stepResult;
                    else
                        pWin += stepResult;
                }
            }
        }

        /// <summary>
        /// The probability that the distribution will take the value k for a given parameter
        /// </summary>
        /// <param name="lyamb"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        static double PoissonOne(double lyamb, double k)
        {
            double fact = 1;
            for (int i = 2; i <= k; i++) fact *= i;

            return Math.Exp(-lyamb) * Math.Pow(lyamb, k) / fact;
        }

        /// <summary>
        /// Determining the most probable Poisson distribution value
        /// </summary>
        /// <param name="lyamb"></param>
        /// <returns></returns>
        static double MaxPoissonOne(double lyamb)
        {
            double now = PoissonOne(lyamb, 0);
            double next;
            int k = 0;
            while ((next = PoissonOne(lyamb, k + 1)) > now)
            {
                now = next;
                k++;
            }
            return k;
        }

        /// <summary>
        /// Class defining a specific game
        /// </summary>
        class Game
        {
            public string home;
            public string away;
            public int gh;
            public int ga;

            public Game(string home, string away, string result)
            {
                this.home = home;
                this.away = away;
                var res = result.Split(" - ").Select(x => int.Parse(x)).ToList();

                this.gh = res[0];
                this.ga = res[1];
            }

            /// <summary>
            /// Recalculation of Elo ratings based on the result of a given match
            /// </summary>
            /// <param name="elo1"></param>
            /// <param name="elo2"></param>
            /// <param name="newElo1"></param>
            /// <param name="newElo2"></param>
            public void NewElo(int elo1, int elo2, out int newElo1, out int newElo2)
            {
                double G = 0, K = 40;
                switch (Math.Abs(gh - ga))
                {
                    case 0:
                        G = 1;
                        break;
                    case 1:
                        G = 1;
                        break;
                    case 2:
                        G = 1.5;
                        break;
                    default:
                        G = (11 + Math.Abs(gh - ga)) / 8;
                        break;
                }

                double drh = elo1 - elo2 + 100;
                double dr = elo2 - elo1 - 100;

                double Weh = 1 / (Math.Pow(10, -drh / 400) + 1);
                double We = 1 / (Math.Pow(10, -dr / 400) + 1);

                double Wh = 0.5;
                double W = 0.5;

                if (gh > ga)
                {
                    Wh = 1;
                    W = 0;
                }
                else if (gh < ga)
                {
                    W = 1;
                    Wh = 0;
                }

                double eloNew1 = Convert.ToDouble(elo1) + K * G * (Wh - Weh);
                double eloNew2 = Convert.ToDouble(elo2) + K * G * (W - We);

                newElo1 = Convert.ToInt32(Math.Round(eloNew1));
                newElo2 = Convert.ToInt32(Math.Round(eloNew2));
            }
        }

        public class Team
        {
            public string Name;
            public int wins = 0;
            public int draws = 0;
            public int loses = 0;
            public int scores = 0;
            public int Elo = 0;
            public double gh = 0;
            public double gch = 0;
            public double ga = 0;
            public double gca = 0;

            public Team(string name, int Elo)
            {
                Name = name;
                this.Elo = Elo;
            }

            public Team(string line)
            {
                List<string> infoes = line.Split(';').ToList();

                if (infoes.Count > 1)
                {
                    Name = infoes[0];
                    gh = Convert.ToDouble(infoes[1]);
                    gch = Convert.ToDouble(infoes[2]);
                    ga = Convert.ToDouble(infoes[3]);
                    gca = Convert.ToDouble(infoes[4]);
                }
                else
                {
                    Name = line;
                    gh = gch = ga = gca = 0;
                }
            }
        }

        static Random rand = new Random();
        static double sumRPS = 0;
        static double sumRMSE = 0;
        static int matchCount = 0;
        static Dictionary<string, string> champsNames = new();

        static List<(string, string, string, string, double)> RPSes = new List<(string, string, string, string, double)>();
        static List<(string, string, string, string, double)> RMSEes = new List<(string, string, string, string, double)>();

        static List<(string season, string champ, string home, string away, int scGH, int scGA, double prW, double prD, double prL, int prGH, int prGA)> results = new();

        static async Task Main(string[] args)
        {
            Dictionary<string, string[]> champs = new();
            List<ConfigElem> elems;
            
            using(StreamReader sr = new StreamReader("config.json"))
            {
                elems = JsonConvert.DeserializeObject<List<ConfigElem>>(sr.ReadToEnd());
            }

            champs = elems.ToDictionary(x => x.ChampId, x => x.ChampSeasons);
            champsNames = elems.ToDictionary(x => x.ChampId, x => x.ChampName);

            List<(string id, string season)> tasks = new List<(string id, string season)>();

            foreach (var id in champs.Keys)
            {
                tasks.AddRange(champs[id].Select(x => (id, x)));
            }

            var start = tasks.AsParallel().Select(GetRPSRMSE).ToList();
            await Task.WhenAll(start);

            ReportGenerator.GenerateReport(RPSes, RMSEes, results);
        }

        static async Task GetRPSRMSE((string id, string season) inData)
        {
            List<Game> games = new List<Game>();
            Dictionary<Team, int> EloRating = new Dictionary<Team, int>();
            ResponseData data;

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                var response = await client.GetAsync($"https://www.fotmob.com/api/leagues?id={inData.id}&ccode3=RUS&season={inData.season.Replace("-", "%2F")}");
                string json = await response.Content.ReadAsStringAsync();

                data = JsonConvert.DeserializeObject<ResponseData>(json);
                foreach (var match in data.matches.allMatches.Where(x => x.status.finished && !x.status.cancelled))
                {
                    try
                    {
                        if (match.status.scoreStr == null)
                        {
                            int a = 0;
                        }
                        var nGame = new Game(match.home.name, match.away.name, match.status.scoreStr);
                        games.Add(nGame);
                        if (!EloRating.Keys.Any(x => x.Name == nGame.home))
                        {
                            EloRating.Add(new Team(nGame.home), 1600);
                        }
                        if (!EloRating.Keys.Any(x => x.Name == nGame.away))
                        {
                            EloRating.Add(new Team(nGame.away), 1600);
                        }
                    }
                    catch (Exception ex)
                    {
                        int a = 0;
                    }
                }
            }

            var testSet = games.Skip(games.Count * 4 / 5).ToList();
            var trainingSet = games.Take(games.Count / 4 * 5).ToList();

            for (int i = 0; i < 1; i++)
                foreach (Game game in trainingSet)
                {
                    try
                    {
                        int elo1, elo2;
                        game.NewElo(EloRating[EloRating.Keys.First(x => x.Name == game.home)], EloRating[EloRating.Keys.First(x => x.Name == game.away)],
                            out elo1, out elo2);
                        EloRating[EloRating.Keys.First(x => x.Name == game.home)] = elo1;
                        EloRating[EloRating.Keys.First(x => x.Name == game.away)] = elo2;
                    }
                    catch (Exception ex)
                    {
                    }
                }

            foreach (Team Team in EloRating.Keys)
            {
                double gamesHome = games.Where(x => x.home == Team.Name).Count();
                double gamesAway = games.Where(x => x.away == Team.Name).Count();

                if(gamesHome == 0 || gamesAway == 0)
                {
                    int a = 0;
                }

                Team.gh = games.Where(x => x.home == Team.Name).Select(x => x.gh).Average();
                Team.gch = games.Where(x => x.home == Team.Name).Select(x => x.ga).Average();
                Team.ga = games.Where(x => x.away == Team.Name).Select(x => x.ga).Average();
                Team.gca = games.Where(x => x.away == Team.Name).Select(x => x.gh).Average();
            }

            List<Team> TeamsNew = EloRating.Keys.ToList();
            var ratings = EloRating.ToDictionary(x => x.Key.Name, x => x.Value);

            List<double> one = new List<double>();
            List<double> delta = new List<double>();
            List<double> goalsDelta = new List<double>();

            for (int i = 0; i < trainingSet.Count; i++)
            {
                Team home = TeamsNew.First(x => x.Name == trainingSet[i].home);
                Team away = TeamsNew.First(x => x.Name == trainingSet[i].away);

                one.Add(1);
                delta.Add(Convert.ToDouble(ratings[trainingSet[i].home] + 100) - Convert.ToDouble(ratings[trainingSet[i].away]));
                goalsDelta.Add(trainingSet[i].gh - (home.gh + away.gca) / 2);

                one.Add(1);
                delta.Add(-Convert.ToDouble(ratings[trainingSet[i].home] + 100) + Convert.ToDouble(ratings[trainingSet[i].away]));
                goalsDelta.Add(trainingSet[i].ga - (home.gch + away.ga) / 2);
            }

            Matrix F = new Matrix(new List<List<double>> { one, delta }).Transpose();

            Matrix M = F.Transpose().Multiplicate(F);

            Matrix M1 = M.Reverce();

            Matrix Y = new Matrix(new List<List<double>> { goalsDelta });

            Matrix alpha = M1.Multiplicate(F.Transpose()).Multiplicate(Y.Transpose());

            Console.WriteLine($"{data.details.name} {data.details.selectedSeason}: {alpha.data[1][0]} * x + {alpha.data[0][0]}");

            double RPS = 0, RMSE = 0;

            int gamesCount = 0;

            for (int i = 0; i < testSet.Count; i++)
            {
                try
                {
                    Team home = TeamsNew.First(x => x.Name == testSet[i].home);
                    Team away = TeamsNew.First(x => x.Name == testSet[i].away);
                    double lyamb1 = (home.gh + away.gca) / 2 + alpha.data[1][0] * ((100 + ratings[home.Name] - ratings[away.Name])) + alpha.data[0][0];
                    double lyamb2 = (home.gch + away.ga) / 2 - alpha.data[1][0] * ((100 + ratings[home.Name] - ratings[away.Name])) + alpha.data[0][0];

                    if (lyamb1 < 0)
                        lyamb1 = 0;
                    if (lyamb2 < 0)
                        lyamb2 = 0;

                    int goalH = Convert.ToInt32(Math.Round(MaxPoissonOne(lyamb1)));
                    int goalA = Convert.ToInt32(Math.Round(MaxPoissonOne(lyamb2)));

                    double pWin, pLose, pDraw, RPSNow, RMSENow;

                    findProb(lyamb1, lyamb2, out pWin, out pDraw, out pLose);

                    pLose = 1 - pWin - pDraw;

                    results.Add((inData.season, champsNames[inData.id], games[i].home, games[i].away, games[i].gh, games[i].ga, pWin, pDraw, pLose, goalH, goalA));

                    if (testSet[i].gh > testSet[i].ga)
                    {
                        RPSNow = ((pWin - 1) * (pWin - 1) + pLose * pLose) / 2;
                    }
                    else if (testSet[i].gh == testSet[i].ga)
                    {
                        RPSNow = (pWin * pWin + pLose * pLose) / 2;
                    }
                    else
                    {
                        RPSNow = ((1 - pLose) * (1 - pLose) + pWin * pWin) / 2;
                    }

                    RMSENow = (games[i].gh - goalH) * (games[i].gh - goalH) + (games[i].ga - goalA) * (games[i].ga - goalA);

                    RPSes.Add((inData.season, champsNames[inData.id], games[i].home, games[i].away, RPSNow));
                    RMSEes.Add((inData.season, champsNames[inData.id], games[i].home, games[i].away, RMSENow));

                    RPS += RPSNow;
                    RMSE += RMSENow;

                    gamesCount++;
                }
                catch { }
            }

            sumRPS += RPS;
            sumRMSE += RMSE;
            matchCount += gamesCount;
            RPS /= gamesCount;
            RMSE = Math.Sqrt(RMSE / gamesCount);
        }
    }
}