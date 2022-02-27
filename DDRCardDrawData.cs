using System;
using System.Collections.Generic;

namespace SM_Rating_Scale_Converter
{
	public class DDRCardDrawData
	{
		public static DDRCardDrawDataSong simfileToCardDrawSong(Simfile simfile)
		{
			DDRCardDrawDataSong song = new DDRCardDrawDataSong();
			song.charts = new List<DDRCardDrawDataChart>();
			foreach (StepChart chart in simfile.StepCharts)
			{
				song.charts.Add(new DDRCardDrawDataChart(chart.Rating, chart.Style, chart.ChartDifficulty));
			}
			song.name = simfile.Name;
			song.artist = simfile.Artist;
			song.jacket = simfile.NameTranslit != null ? simfile.NameTranslit + ".png" : simfile.Name + ".png"; // Hard code for jackets already present in the stock app lol
			song.folder = ""; // What is this even used for?
			song.bpm = simfile.BPM;
			song.name_translation = simfile.NameTranslit;
			song.artist_translation = simfile.ArtistTranslit;
			song.genre = ""; // No idea if this is actually used for anything either...

			return song;
		}
	}

	public class DDRCardDrawDataSong
	{
		public string name { get; set; }
		public string artist { get; set; }
		public string jacket { get; set; }
		public string folder { get; set; }
		public string bpm { get; set; }
		public string name_translation { get; set; }
		public string artist_translation { get; set; }
		public string genre { get; set; }
		public List<DDRCardDrawDataChart> charts { get; set; }
	}

	public class DDRCardDrawDataChart
	{
		public int lvl { get; set; }
		public string style { get; set; }
		public string diffClass { get; set; }
		public DDRCardDrawDataChart(int lvl, string style, StepDifficulty difficulty)
        {
			this.lvl = lvl;
			this.style = style;
			diffClass = difficulty.ToString();
        }
	}
}