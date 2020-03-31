using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SM_Rating_Scale_Converter
{
    class SimfileReader
    {

        public static List<Simfile> OpenSimfileDir()
        {
            List<Simfile> simfiles = new List<Simfile>();
            List<string> simfileNames = new List<string>();
            int duplicateCount = 0;

            CommonOpenFileDialog dialogOpenFolder = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select a folder containing Stepmania simfiles...",
            };

            if (dialogOpenFolder.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return simfiles; //User hit cancel, let's not load any simfiles
            }

            //Get all SM simfiles in the selected directory and parse them
            string[] simfilePaths = Directory.GetFiles(dialogOpenFolder.FileName, "*.sm", SearchOption.AllDirectories);
            foreach (string simfilePath in simfilePaths)
            {
                Simfile simfile = Simfile.ParseSimfileSM(simfilePath);
                if (simfileNames.Contains(simfile.Name)) { //If a file with the same name has been loaded before, don't add it to the simfile list
                    duplicateCount++;
                    Console.WriteLine("Duplicate SM file: " + simfile.Name);
                } else { //Otherwise, add the simfile to the list
                    simfiles.Add(simfile);
                    simfileNames.Add(simfile.Name);
                }
            }

            //Get all DWI simfiles in the selected directory and parse them
            simfilePaths = Directory.GetFiles(dialogOpenFolder.FileName, "*.dwi", SearchOption.AllDirectories);
            foreach (string simfilePath in simfilePaths)
            {
                Simfile simfile = Simfile.ParseSimfileDWI(simfilePath);
                if (simfileNames.Contains(simfile.Name)) { //If a file with the same name has been loaded before, don't add it to the simfile list
                    duplicateCount++;
                    Console.WriteLine("Duplicate DWI file: " + simfile.Name);
                } else { //Otherwise, add the simfile to the list
                    simfiles.Add(simfile);
                    simfileNames.Add(simfile.Name);
                }
            }

            if (duplicateCount >= 1)
            {
                MessageBox.Show(duplicateCount + " simfiles with the same name were detected and have not been loaded.\n\nPlease ensure all simfiles have different names, and that each simfile only has *one* supported file type for note data (i.e. either a SM *or* a DWI file)", "Warning - SM Rating Scale Converter", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return simfiles;
        }

        //Apply rating changes to simfiles and save them to file
        public static void ApplyRatingChangesAndSaveSimfiles(List<Simfile> simfiles)
        {
            foreach (Simfile simfile in simfiles)
            {
                switch (simfile.ScaleChange)
                {
                    case RatingScaleChange.ToOld:
                        Console.WriteLine("To Old Scale: " + simfile.Name);
                        simfile.StepCharts = ApplyRatingChange(simfile.StepCharts, RatingScaleChange.ToOld);
                        break;
                    case RatingScaleChange.ToX:
                        Console.WriteLine("To X   Scale: " + simfile.Name);
                        simfile.StepCharts = ApplyRatingChange(simfile.StepCharts, RatingScaleChange.ToX);
                        break;
                    default:
                        Console.WriteLine("No    Change: " + simfile.Name + ". Skipping.");
                        continue;
                }

                if (simfile.Path.Substring(simfile.Path.Length - 3).Equals(".sm", StringComparison.CurrentCultureIgnoreCase))
                {
                    Simfile.SaveSimfileSM(simfile);
                } else if (simfile.Path.Substring(simfile.Path.Length - 4).Equals(".dwi", StringComparison.CurrentCultureIgnoreCase)) {
                    Simfile.SaveSimfileDWI(simfile);
                } else
                {
                    MessageBox.Show("Error: Couldn't save invalid simfile extension \"" + simfile.Path.Substring(simfile.Path.Length - 3) + "\". This shouldn't happen.", "Error - SM Rating Scale Converter", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            MessageBox.Show("Processed all simfiles!");
        }

        //Bulk-apply stepchart rating changes
        public static List<StepChart> ApplyRatingChange(List<StepChart> stepCharts, RatingScaleChange changeType)
        {
            foreach (StepChart chart in stepCharts) {
                switch (changeType)
                {
                    case RatingScaleChange.ToOld:
                        chart.Rating = Convert.ToInt32(chart.Rating / 1.5); //Ye olde "multiply or divide by 1.5" to convert ratings... not great, but better than having a simfile pack where every file uses a different rating system (thanks, ZiV)
                        break;
                    case RatingScaleChange.ToX:
                        chart.Rating = Convert.ToInt32(chart.Rating * 1.5);
                        break;
                }
            }

            return stepCharts;
        }
    }
}
