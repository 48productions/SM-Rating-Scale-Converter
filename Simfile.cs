using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SM_Rating_Scale_Converter
{
    enum StepDifficulty
    {
        Beginner, //SM Difficulties
        Easy,
        Medium,
        Hard,
        Expert,
        BASIC, //DWI Difficulties
        ANOTHER,
        MANIAC,
        SMANIAC,
        Edit, //Edit (default-ish)
    }

    enum RatingScaleChange
    {
        ToOld,
        ToX,
        NoChange,
    }

    class Simfile
    {
        public Simfile()
        {
            StepCharts = new List<StepChart>();
            ScaleChange = RatingScaleChange.NoChange;
        }

        public string Name { get; set; }
        public List<StepChart> StepCharts { get; set; }

        public RatingScaleChange ScaleChange { get; set; }

        public string Path { get; set; }


        //Parse a .SM simfile for stepcharts
        internal static Simfile ParseSimfileSM(string path)
        {
            Console.WriteLine("Parsing SM simfile: " + path);
            Simfile simfile = new Simfile();
            simfile.Path = path;

            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i <= lines.Length - 1; i++)
            {
                string line = lines[i];

                //Console.WriteLine(line);

                if (line.Contains("#TITLE:"))
                {
                    simfile.Name = line.Substring(7, line.Length - 8); //Read everything after "#TITLE:" to the end of the line (minus the ;, so len = - 7 - 1)
                    Console.WriteLine("Found name: " + simfile.Name);
                } else if (line.Contains("#NOTES:"))
                {
                    Console.WriteLine("Found notes section:");
                    string difficultyLine = lines[i + 3].Trim().TrimEnd(':'); //Trim off leading spaces and trailing colons
                    Console.WriteLine("Difficulty: " + difficultyLine);

                    StepDifficulty difficulty;
                    if (!Enum.TryParse(difficultyLine, true, out difficulty)) difficulty = StepDifficulty.Edit; //Try parsing the difficulty line - if there's a valid difficulty there, use it. If not, call it an Edit difficulty.

                    string ratingLine = lines[i + 4].Trim().TrimEnd(':'); //Trim off leading spaces and trailing colons here too
                    Console.WriteLine("Rating: " + ratingLine);

                    simfile.StepCharts.Add(new StepChart(difficulty, Int32.Parse(ratingLine)));
                }
            }
            return simfile;
        }



        //Parse a .DWI simfile for stepcharts
        internal static Simfile ParseSimfileDWI(string path)
        {
            Console.WriteLine("Parsing DWI simfile: " + path);
            Simfile simfile = new Simfile();
            simfile.Path = path;

            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i <= lines.Length - 1; i++)
            {
                string line = lines[i];

                //Console.WriteLine(line);

                if (line.Contains("#TITLE:"))
                {
                    simfile.Name = line.Substring(7, line.Length - 8); //Read everything after "#TITLE:" to the end of the line (minus the ;, so len = - 7 - 1)
                    Console.WriteLine("Found name: " + simfile.Name);
                }
                else if (line.Contains("#SINGLE:") || line.Contains("#DOUBLE:") || line.Contains("#COUPLE:") || line.Contains("#SOLO:"))
                {
                    Console.WriteLine("Found notes section:");
                    string[] dwiParams = line.Split(':'); //Split the string at : to get the parameters DWI uses for chart data
                    string difficultyLine = dwiParams[1]; //Parameter 1 is difficulty (basic, maniac, etc)
                    Console.WriteLine("Difficulty: " + difficultyLine);

                    StepDifficulty difficulty;
                    if (!Enum.TryParse(difficultyLine, true, out difficulty)) difficulty = StepDifficulty.Edit; //Try parsing the difficulty line - if there's a valid difficulty there, use it. If not, call it an Edit difficulty.

                    string ratingLine = dwiParams[2]; //Paramter 2 is the rating
                    Console.WriteLine("Rating: " + ratingLine);

                    simfile.StepCharts.Add(new StepChart(difficulty, Int32.Parse(ratingLine)));
                }
            }

            return simfile;
        }



        //Modify a SM format simfile with new step ratings
        internal static void SaveSimfileSM(Simfile simfile)
        {
            Console.WriteLine("Writing SM " + simfile.Path);
            string[] lines = File.ReadAllLines(simfile.Path);

            int chartIndex = 0;

            for (int i = 0; i <= lines.Length - 1; i++)
            {
                //Console.WriteLine(line);

                if (lines[i].Contains("#NOTES:"))
                {
                    lines[i + 4] = "     " + simfile.StepCharts[chartIndex].Rating + ":"; //Write the new rating line
                    Console.WriteLine("New chart rating: " + simfile.StepCharts[chartIndex].Rating);

                    chartIndex++; //Move onto the next chart index for the next chart
                }
            }

            File.WriteAllLines(simfile.Path, lines);
        }



        //Modify a DWI format simfile with new step ratings
        internal static void SaveSimfileDWI(Simfile simfile)
        {
            Console.WriteLine("Writing DWI " + simfile.Path);
            string[] lines = File.ReadAllLines(simfile.Path);

            int chartIndex = 0;

            for (int i = 0; i <= lines.Length - 1; i++)
            {
                //Console.WriteLine(line);

                if (lines[i].Contains("#SINGLE:") || lines[i].Contains("#DOUBLE:") || lines[i].Contains("#COUPLE:") || lines[i].Contains("#SOLO:"))
                {
                    string[] dwiParams = lines[i].Split(':'); //Split the string at : to get the parameters DWI uses for chart data

                    dwiParams[2] = simfile.StepCharts[chartIndex].Rating.ToString();
                    Console.WriteLine("New chart rating: " + simfile.StepCharts[chartIndex].Rating.ToString());

                    lines[i] = String.Join(":", dwiParams); //CTRL + Z STRING.SPLIT to get the original line back with the new difficulties

                    chartIndex++; //Move onto the next chart index for the next chart
                }
            }

            File.WriteAllLines(simfile.Path, lines);
        }




        //Convinience function to get a color brush based on a difficulty
        internal static Brush GetDifficultyColor(StepDifficulty chartDifficulty)
        {
            switch (chartDifficulty)
            {
                case StepDifficulty.Beginner:
                    return Brushes.MediumBlue;
                case StepDifficulty.Easy:
                case StepDifficulty.BASIC:
                    return Brushes.DarkGreen;
                case StepDifficulty.Medium:
                case StepDifficulty.ANOTHER:
                    return Brushes.DarkGoldenrod;
                case StepDifficulty.Hard:
                case StepDifficulty.MANIAC:
                    return Brushes.DarkRed;
                case StepDifficulty.Expert:
                case StepDifficulty.SMANIAC:
                    return Brushes.Purple;
                default:
                    return Brushes.DarkSlateGray;
            }
        }

        //Doesn't really fit in this class but w/e lol, returns a color brush for the background based on whether the background should be a light or dark line
        internal static Brush GetBGColor(bool useDarkLine)
        {
            return useDarkLine ? Brushes.Silver : Brushes.LightGray;
        }
    }

    class StepChart
    {
        public StepChart(StepDifficulty difficulty, int rating)
        {
            Rating = rating;
            ChartDifficulty = difficulty;
        }
        public int Rating { get; set; }
        public StepDifficulty ChartDifficulty { get; set; }
    }
}
