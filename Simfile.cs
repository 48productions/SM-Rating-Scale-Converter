using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SM_Rating_Scale_Converter
{
    public enum StepDifficulty
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

    public enum RatingScaleChange
    {
        ToOld,
        ToX,
        NoChange,
    }

    public class Simfile
    {
        public Simfile()
        {
            StepCharts = new List<StepChart>();
            ScaleChange = RatingScaleChange.NoChange;
        }

        public string Name { get; set; }
        public string Artist { get; set; }
        public string NameTranslit { get; set; }
        public string ArtistTranslit { get; set; }
        public string BPM { get; set; }
        public List<StepChart> StepCharts { get; set; }

        public RatingScaleChange ScaleChange { get; set; }

        public string Path { get; set; }


        //Parse a .SM simfile for stepcharts
        public static Simfile ParseSimfileSM(string path)
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
                }
                else if (line.Contains("#ARTIST:"))
                {
                    simfile.Artist = line.Substring(8, line.Length - 9);

                }
                else if (line.Contains("#TITLETRANSLIT:"))
                {
                    simfile.NameTranslit = line.Substring(15, line.Length - 16);

                }
                else if (line.Contains("#ARTISTTRANSLIT:"))
                {
                    simfile.ArtistTranslit = line.Substring(16, line.Length - 17);
                }
                else if (line.Contains("#BPMS:"))
                {
                    // Parse BPMs from an SM-formatted BPM string: #BPMS:x=y,x=y,...; Position=value,
                    int bpmPos = 0;
                    int bpmEndPos = 0;
                    float bpmLow = -1;
                    float bpmHigh = 0;
                    while (true)
                    {
                        bpmPos = line.IndexOf('=', bpmPos); // Find the next = in the BPM list for the start of this BPM entry, starting from where the last one was found
                        if (bpmPos < 0)
                        { // If IndexOf returns -1, there's no more BPMs on this line.
                            if (line.IndexOf(';') == -1) // If an end ; isn't on this line, the BPM segment may continue onto the next line...
                            {
                                i++; // So start looking on the next line and continue searching.
                                line = lines[i];
                                bpmPos = 0; bpmEndPos = 0;
                                continue;
                            } else { break; }
                        }
                        bpmEndPos = line.IndexOfAny(new char[] { ',', ';' }, bpmPos) - 1; // Find the next , or ; in the BPM list for the end of this BPM
                        if (bpmEndPos < 0) { bpmEndPos = line.Length - 1; } // SM sometimes places ; on a newline, so if we fail to find a ;, it's probably just the last BPM in the chain

                        // Parse the BPM out of the string, if it's outside our found max/min values, record it
                        float bpm;
                        try
                        {
                            Console.WriteLine(line.Substring(bpmPos + 1, bpmEndPos - bpmPos));
                            bpm = (float)Double.Parse(line.Substring(bpmPos + 1, bpmEndPos - bpmPos));
                        } catch (FormatException e) { Console.WriteLine("BPM formatting error!"); break; }

                        if (bpmLow == -1 || bpm < bpmLow) { bpmLow = bpm; }
                        if (bpm > bpmHigh) { bpmHigh = bpm; }

                        bpmPos = bpmEndPos; // Start looking for the next BPM after the end of this one
                    }
                    if (bpmLow == bpmHigh) { simfile.BPM = bpmLow.ToString(); } else { simfile.BPM = bpmLow.ToString() + "-" + bpmHigh.ToString(); }
                }
                else if (line.Contains("#DISPLAYBPM:"))
                {
                    simfile.BPM = line.Substring(12, line.Length - 13);

                }
                else if (line.Contains("#NOTES:"))
                {

                    Console.WriteLine("Found notes section:");
                    string styleLine = lines[i + 1].Trim().TrimEnd(':');
                    Console.WriteLine("Style: " + styleLine);
                    string difficultyLine = lines[i + 3].Trim().TrimEnd(':'); //Trim off leading spaces and trailing colons
                    Console.WriteLine("Difficulty: " + difficultyLine);

                    StepDifficulty difficulty;
                    if (!Enum.TryParse(difficultyLine, true, out difficulty)) difficulty = StepDifficulty.Edit; //Try parsing the difficulty line - if there's a valid difficulty there, use it. If not, call it an Edit difficulty.


                    string ratingLine = lines[i + 4].Trim().TrimEnd(':'); //Trim off leading spaces and trailing colons here too
                    Console.WriteLine("Rating: " + ratingLine);

                    simfile.StepCharts.Add(new StepChart(difficulty, Int32.Parse(ratingLine), styleLine));
                }
            }
            return simfile;
        }



        //Parse a .DWI simfile for stepcharts
        public static Simfile ParseSimfileDWI(string path)
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
                else if (line.Contains("#ARTIST:"))
                {
                    simfile.Artist = line.Substring(8, line.Length - 9);
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

                    string styleLine = dwiParams[0];
                    if (styleLine == "#SINGLE") { styleLine = "dance-single"; } else if (styleLine == "#DOUBLE") { styleLine = "dance-double"; } else { styleLine = ""; }

                    simfile.StepCharts.Add(new StepChart(difficulty, Int32.Parse(ratingLine), styleLine));
                }
            }

            return simfile;
        }



        //Modify a SM format simfile with new step ratings
        public static void SaveSimfileSM(Simfile simfile)
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
        public static void SaveSimfileDWI(Simfile simfile)
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

    public class StepChart
    {
        public StepChart(StepDifficulty difficulty, int rating, string style)
        {
            Rating = rating;
            ChartDifficulty = difficulty;
            Style = style;
        }
        public int Rating { get; set; }
        public StepDifficulty ChartDifficulty { get; set; }
        public string Style { get; set; }
    }
}
