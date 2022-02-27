using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SM_Rating_Scale_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<Simfile> simfiles;

        List<RadioButton> rbsToOld;
        List<RadioButton> rbsKeep;
        List<RadioButton> rbsToX;

        bool saveLocked = true;

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            loadGroup();
        }

        private void loadGroup()
        {
            gridSimfiles.Children.Clear(); //Clear out previous simfiles in the grid
            gridSimfiles.RowDefinitions.Clear();
            rbsToOld = new List<RadioButton>();
            rbsKeep = new List<RadioButton>();
            rbsToX = new List<RadioButton>();
            simfiles = SimfileReader.OpenSimfileDir();

            /*List<StepChart> stepCharts = new List<StepChart>();
            stepCharts.Add(new StepChart(StepDifficulty.Beginner, 2));
            stepCharts.Add(new StepChart(StepDifficulty.Easy, 4));
            Simfile test = new Simfile
            {
                Name = "Derp derp test thing",
                StepCharts = stepCharts,
            };
            addSimfileToGrid(test);
            */

            bool useDarkLine = false;
            foreach (Simfile simfile in simfiles) //Add a row to the grid for each simfile
            {
                useDarkLine = !useDarkLine; //Alternate between light and dark backgrounds for rows
                addSimfileToGrid(simfile, useDarkLine);
            }
            gridSimfiles.RowDefinitions.Add(new RowDefinition //Add another dummy row to the grid to prevent an issue with the last two simfiles overlapping in the grid
            {
                Height = GridLength.Auto
            });

            saveLocked = false;
        }

        private void rbSetScale_Click(object sender, RoutedEventArgs e, Simfile simfile, RatingScaleChange scaleChange)
        {
            if (saveLocked) { promptForGroupReload(); return; }

            simfile.ScaleChange = scaleChange;
        }


        private void addSimfileToGrid(Simfile simfile, bool useDarkLine)
        {
            gridSimfiles.RowDefinitions.Add(new RowDefinition { //Add a new row to the grid for this simfile
                Height = GridLength.Auto
            });

            Label labelTitle = new Label { //Then add the label for the file's title
                Content = simfile.Name,
                Background = Simfile.GetBGColor(useDarkLine), 
            };
            Grid.SetColumn(labelTitle, 0);
            Grid.SetRow(labelTitle, simfiles.IndexOf(simfile) + 1);
            gridSimfiles.Children.Add(labelTitle);

            foreach (StepChart chart in simfile.StepCharts) { //Iterate through charts to add to the grid
                if (simfile.StepCharts.IndexOf(chart) + 1 > 5) continue; //Only display up to 5 charts in the grid
                Label labelDifficulty = new Label //Add the difficulty rating/color to the grid
                {
                    Content = chart.Rating,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Foreground = Simfile.GetDifficultyColor(chart.ChartDifficulty),
                    Background = Simfile.GetBGColor(useDarkLine),
                };
                Grid.SetColumn(labelDifficulty, simfile.StepCharts.IndexOf(chart) + 1);
                Grid.SetRow(labelDifficulty, simfiles.IndexOf(simfile) + 1);
                gridSimfiles.Children.Add(labelDifficulty);
            }

            //Now add the radio buttons for what scale to set a chart to
            RadioButton setScaleOld = new RadioButton
            {
                Content = "To Old",
                GroupName = simfile.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Simfile.GetBGColor(useDarkLine),
            };
            setScaleOld.Click += (sender, EventArgs) => { rbSetScale_Click(sender, EventArgs, simfile, RatingScaleChange.ToOld); };
            rbsToOld.Add(setScaleOld);
            Grid.SetColumn(setScaleOld, 6);
            Grid.SetRow(setScaleOld, simfiles.IndexOf(simfile) + 1);
            gridSimfiles.Children.Add(setScaleOld);

            RadioButton setScaleKeep = new RadioButton
            {
                Content = "Keep",
                GroupName = simfile.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Simfile.GetBGColor(useDarkLine),
            };
            setScaleKeep.Click += (sender, EventArgs) => { rbSetScale_Click(sender, EventArgs, simfile, RatingScaleChange.NoChange); };
            rbsKeep.Add(setScaleKeep);
            Grid.SetColumn(setScaleKeep, 7);
            Grid.SetRow(setScaleKeep, simfiles.IndexOf(simfile) + 1);
            gridSimfiles.Children.Add(setScaleKeep);

            RadioButton setScaleX = new RadioButton
            {
                Content = "To X",
                GroupName = simfile.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Simfile.GetBGColor(useDarkLine),
            };
            setScaleX.Click += (sender, EventArgs) => { rbSetScale_Click(sender, EventArgs, simfile, RatingScaleChange.ToX); };
            rbsToX.Add(setScaleX);
            Grid.SetColumn(setScaleX, 8);
            Grid.SetRow(setScaleX, simfiles.IndexOf(simfile) + 1);
            gridSimfiles.Children.Add(setScaleX);

            setScaleKeep.IsChecked = true; //Set the current row's "keep" button as checked
        }

        private void buttonConvertAllOld_Click(object sender, RoutedEventArgs e)
        {
            if (saveLocked) { promptForGroupReload(); return; }

            foreach (RadioButton radiobutton in rbsToOld)
            {
                radiobutton.IsChecked = true;
            }
        }

        private void buttonConvertAllKeep_Click(object sender, RoutedEventArgs e)
        {
            if (saveLocked) { promptForGroupReload(); return; }

            foreach (RadioButton radiobutton in rbsKeep)
            {
                radiobutton.IsChecked = true;
            }
        }

        private void buttonConvertAllX_Click(object sender, RoutedEventArgs e)
        {
            if (saveLocked) { promptForGroupReload(); return; }

            foreach (RadioButton radiobutton in rbsToX)
            {
                radiobutton.IsChecked = true;
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (saveLocked) { promptForGroupReload(); return; }

            int toOldCount = 0;
            int keepCount = 0;
            int toXCount = 0;

            foreach (Simfile simfile in simfiles)
            {
                switch (simfile.ScaleChange)
                {
                    case RatingScaleChange.ToOld:
                        toOldCount++;
                        break;
                    case RatingScaleChange.ToX:
                        toXCount++;
                        break;
                    default:
                        keepCount++;
                        break;
                }
            }

            MessageBoxResult result = MessageBox.Show("Processing " + (toOldCount + keepCount + toXCount) + " total simfiles:\n\n - " + toOldCount + " to Old Scale\n - " + keepCount + " keeping scale (no change)\n - " + toXCount + " to X Scale\n\nProceed?", "Saving - SM Rating Scale Converter", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK) {
                SimfileReader.ApplyRatingChangesAndSaveSimfiles(simfiles);
                saveLocked = true;
            }
        }

        private void promptForGroupReload()
        {
            MessageBoxResult result = MessageBox.Show("You must reload the current group or load a new group to make any new changes.\n\nReload now?", "Reload? - SM Rating Scale Converter", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) { loadGroup(); }
        }

        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            CommonSaveFileDialog dialogExportData = new CommonSaveFileDialog
            {
                Title = "Save exported data to...",
                DefaultFileName = "songdata.json",
            };

            if (dialogExportData.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return; // It's TIME to STOP! NO MORE!
            }

            List<DDRCardDrawDataSong> songs = new List<DDRCardDrawDataSong>();
            foreach (Simfile file in simfiles)
            {
                songs.Add(DDRCardDrawData.simfileToCardDrawSong(file));
            }

            string data = JsonConvert.SerializeObject(songs);
            File.WriteAllText(dialogExportData.FileName, data);

        }
    }
}
