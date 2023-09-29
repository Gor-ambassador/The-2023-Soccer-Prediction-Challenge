using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.ChartEx;
using System.Data;

namespace Testing
{
    internal class ReportGenerator
    {
        public static void GenerateReport(List<(string, string, string, string, double)> RPSes, List<(string, string, string, string, double)> RMSEes,
            List<(string season, string champ, string home, string away, int scGH, int scGA, double prW, double prD, double prL, int prGH, int prGA)> results)
        {
            if (File.Exists("Report.xlsx"))
                File.Delete("Report.xlsx");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo("Report.xlsx")))
            {
                RPSes = RPSes.OrderBy(x => x.Item5).ToList();

                var sheet = package.Workbook.Worksheets
                    .Add("Matches");

                sheet.Cells["A1"].Value = "Season";
                sheet.Cells["B1"].Value = "Championship";
                sheet.Cells["C1"].Value = "Home";
                sheet.Cells["D1"].Value = "Away";
                sheet.Cells["E1"].Value = "GH";
                sheet.Cells["F1"].Value = "GA";
                sheet.Cells["G1"].Value = "prH";
                sheet.Cells["H1"].Value = "prD";
                sheet.Cells["I1"].Value = "prL";
                sheet.Cells["J1"].Value = "prGH";
                sheet.Cells["K1"].Value = "prGA";

                int row = 2;

                foreach(var result in results.OrderBy(x => x.champ).ThenBy(x => x.season).ThenBy(x => x.home).ThenBy(x => x.away).ToList())
                {
                    sheet.Cells[row, 1].Value = result.season;
                    sheet.Cells[row, 2].Value = result.champ;
                    sheet.Cells[row, 3].Value = result.home;
                    sheet.Cells[row, 4].Value = result.away;
                    sheet.Cells[row, 5].Value = result.scGH;
                    sheet.Cells[row, 6].Value = result.scGA;
                    sheet.Cells[row, 7].Value = result.prW;
                    sheet.Cells[row, 8].Value = result.prD;
                    sheet.Cells[row, 9].Value = result.prL;
                    sheet.Cells[row, 10].Value = result.prGH;
                    sheet.Cells[row, 11].Value = result.prGA;

                    row++;
                }

                sheet = package.Workbook.Worksheets
                    .Add("SortedRPS");

                sheet.Cells["A1"].Value = "Season";
                sheet.Cells["B1"].Value = "Championship";
                sheet.Cells["C1"].Value = "Home";
                sheet.Cells["D1"].Value = "Away";
                sheet.Cells["E1"].Value = "RPS";

                row = 2;
                foreach(var RPS in RPSes)
                {
                    sheet.Cells[row, 1].Value = RPS.Item1;
                    sheet.Cells[row, 2].Value = RPS.Item2;
                    sheet.Cells[row, 3].Value = RPS.Item3;
                    sheet.Cells[row, 4].Value = RPS.Item4;
                    sheet.Cells[row, 5].Value = RPS.Item5;

                    row++;
                }

                var GroupedRPS = RPSes.GroupBy(x => x.Item2);

                sheet.Cells["G1"].Value = "Season";
                sheet.Cells["H1"].Value = "Championship";
                sheet.Cells["I1"].Value = "RPS";

                row = 2;

                var RPSGroups = new List<(string, string, double)>();

                foreach (var group in GroupedRPS)
                {
                    var seasonGrouped = group.GroupBy(x => x.Item1);

                    foreach(var season in seasonGrouped)
                    {
                        RPSGroups.Add((season.Key, group.Key, season.Select(x => x.Item5).Sum() / season.Count()));
                    }
                }

                RPSGroups = RPSGroups.OrderBy(x => x.Item3).ToList();

                foreach(var RPSGroup in RPSGroups)
                {
                    sheet.Cells[row, 7].Value = RPSGroup.Item1;
                    sheet.Cells[row, 8].Value = RPSGroup.Item2;
                    sheet.Cells[row, 9].Value = RPSGroup.Item3;

                    row++;
                }

                RMSEes = RMSEes.OrderBy(x => x.Item5).ToList();
                sheet = package.Workbook.Worksheets
                    .Add("SortedRMSE^2");

                sheet.Cells["A1"].Value = "Season";
                sheet.Cells["B1"].Value = "Championship";
                sheet.Cells["C1"].Value = "Home";
                sheet.Cells["D1"].Value = "Away";
                sheet.Cells["E1"].Value = "RMSE^2";

                row = 2;
                foreach (var RMSE in RMSEes)
                {
                    sheet.Cells[row, 1].Value = RMSE.Item1;
                    sheet.Cells[row, 2].Value = RMSE.Item2;
                    sheet.Cells[row, 3].Value = RMSE.Item3;
                    sheet.Cells[row, 4].Value = RMSE.Item4;
                    sheet.Cells[row, 5].Value = RMSE.Item5;

                    row++;
                }

                var GroupedRMSE = RMSEes.GroupBy(x => x.Item2);

                sheet.Cells["G1"].Value = "Season";
                sheet.Cells["H1"].Value = "Championship";
                sheet.Cells["I1"].Value = "RMSE";

                row = 2;

                var RMSEGroups = new List<(string, string, double)>();

                foreach (var group in GroupedRMSE)
                {
                    var seasonGrouped = group.GroupBy(x => x.Item1);

                    foreach (var season in seasonGrouped)
                    {
                        RMSEGroups.Add((season.Key, group.Key, season.Select(x => x.Item5).Sum() / season.Count()));
                    }
                }

                RMSEGroups = RMSEGroups.OrderBy(x => x.Item3).ToList();

                foreach (var RMSEGroup in RMSEGroups)
                {
                    sheet.Cells[row, 7].Value = RMSEGroup.Item1;
                    sheet.Cells[row, 8].Value = RMSEGroup.Item2;
                    sheet.Cells[row, 9].Value = Math.Sqrt(RMSEGroup.Item3);

                    row++;
                }

                sheet = package.Workbook.Worksheets
                    .Add("Statistics");

                sheet.Cells["A1"].Value = "RPS";
                sheet.Cells["A2"].Value = "Mean";
                sheet.Cells["B2"].Formula = "AVERAGE('SortedRPS'!E2:E" + (RPSes.Count + 1).ToString() + ")";
                sheet.Cells["A3"].Value = "Sample variance";
                sheet.Cells["B3"].Formula = "VAR('SortedRPS'!E2:E" + (RPSes.Count + 1).ToString() + ")";
                sheet.Cells["A4"].Value = "Minimum";
                sheet.Cells["B4"].Formula = "MIN('SortedRPS'!E2:E" + (RPSes.Count + 1).ToString() + ")";
                sheet.Cells["A5"].Value = "25%";
                sheet.Cells["B5"].Formula = "QUARTILE('SortedRPS'!E2:E" + (RPSes.Count + 1).ToString() + ",1)";
                sheet.Cells["A6"].Value = "50%";
                sheet.Cells["B6"].Formula = "QUARTILE('SortedRPS'!E2:E" + (RPSes.Count + 1).ToString() + ",2)";
                sheet.Cells["A7"].Value = "75%";
                sheet.Cells["B7"].Formula = "QUARTILE('SortedRPS'!E2:E" + (RPSes.Count + 1).ToString() + ",3)";
                sheet.Cells["A8"].Value = "Maximum";
                sheet.Cells["B8"].Formula = "MAX('SortedRPS'!E2:E" + (RPSes.Count + 1).ToString() + ")";

                sheet.Cells["D1"].Value = "RMSE^2";
                sheet.Cells["D2"].Value = "Mean";
                sheet.Cells["E2"].Formula = "AVERAGE('SortedRMSE^2'!E2:E" + (RMSEes.Count + 1).ToString() + ")";
                sheet.Cells["D3"].Value = "Sample variance";
                sheet.Cells["E3"].Formula = "VAR('SortedRMSE^2'!E2:E" + (RMSEes.Count + 1).ToString() + ")";
                sheet.Cells["D4"].Value = "Minimum";
                sheet.Cells["E4"].Formula = "MIN('SortedRMSE^2'!E2:E" + (RMSEes.Count + 1).ToString() + ")";
                sheet.Cells["D5"].Value = "25%";
                sheet.Cells["E5"].Formula = "QUARTILE('SortedRMSE^2'!E2:E" + (RMSEes.Count + 1).ToString() + ",1)";
                sheet.Cells["D6"].Value = "50%";
                sheet.Cells["E6"].Formula = "QUARTILE('SortedRMSE^2'!E2:E" + (RMSEes.Count + 1).ToString() + ",2)";
                sheet.Cells["D7"].Value = "75%";
                sheet.Cells["E7"].Formula = "QUARTILE('SortedRMSE^2'!E2:E" + (RMSEes.Count + 1).ToString() + ",3)";
                sheet.Cells["D8"].Value = "Maximum";
                sheet.Cells["E8"].Formula = "MAX('SortedRMSE^2'!E2:E" + (RMSEes.Count + 1).ToString() + ")";

                sheet = package.Workbook.Worksheets.Add("Charts");

                var chart = sheet.Drawings.AddHistogramChart("RPS Distribution");
                chart.Title.Text = "RPS distribution";
                chart.SetPosition(0, 10, 0, 10);
                chart.SetSize(800, 400);
                var hg = chart.Series.Add(package.Workbook.Worksheets[1].Cells[$"E2:E{RMSEes.Count + 1}"], null);
                hg.Binning.Size = 0.01;

                chart = sheet.Drawings.AddHistogramChart("RPS Distribution by championships");
                chart.Title.Text = "RPS distribution by championships";
                chart.SetPosition(22, 10, 0, 10);
                chart.SetSize(800, 400);
                hg = chart.Series.Add(package.Workbook.Worksheets[1].Cells[$"I2:I{RMSEGroups.Count + 1}"], null);
                hg.Binning.Size = 0.005;

                chart = sheet.Drawings.AddHistogramChart("RMSE Distribution by championships");
                chart.Title.Text = "RMSE distribution by championships";
                chart.SetPosition(43, 10, 0, 10);
                chart.SetSize(800, 400);
                hg = chart.Series.Add(package.Workbook.Worksheets[2].Cells[$"I2:I{RMSEGroups.Count + 1}"], null);
                hg.Binning.Size = 0.05;

                package.Save();
            }
        }
    }
}
