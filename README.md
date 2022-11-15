# SM-Rating-Scale-Converter
 
This is a simple tool that does what the name implies... it convert Stepmania simfiles between different chart rating scales (namely Old/ITG and New/X scale). It was written over the span of a day and a half in C# using WPF for an *extremely debatably* snazzy UI.



## Features:
 * Current supports SSC, SM, and DWI format simfiles
 * Supports conversions to and from the Old/ITG and New/X chart rating scales
 * Supports batch conversions or conversions on a simfile-by-simfile basis



## Download
[Downloads for pre-compiled binaries that *probably* work are available in bin/Debug/](https://github.com/48productions/SM-Rating-Scale-Converter/tree/master/bin/Debug) - *Download the whole folder to get all the required files!*

Source code is provided, so feel free to modify/compile it yourself, too! I built it in Visual Studio 2017 on Windows 10, but your guess on how well it'll compile on other systems is as good as mine.





## Usage instructions:
1. [Download the folder with the precompiled binary](https://github.com/48productions/SM-Rating-Scale-Converter/tree/master/bin/Debug), or try compiling it yourself if you feel brave.
2. Run the exe, open a folder with simfiles. It doesn't need to be a group folder or a song folder, anything that contains a simfile somewhere within a subdirectory or a hundred will do. You can just have a mess of .sm files in a single folder for all the program cares, it'll just open everything in finds in all subdirectories.
  * Note: Each .ssc/.sm/.dwi file is treated as a separate simfile. If multiple "simfiles" have the same `#TITLE` field (like if a single song has both a SSC and SM file), only the first file found (in the order of SSC, SM, then DWI) will be loaded. This should probably be fixed.
3. Each simfile's `#TITLE` field and up to 5 chart difficulty values are displayed in a table. Click "Convert all to Old/X" at the top to convert all simfiles found to a certain scale, or alternatively set each simfile's conversion individually. Click "Keep all scales" to revert changes you've made.
  * "To Old" converts a file from the X scale to the Old scale (divides chart ratings by 1.5)
  * "Keep" doesn't change a file's rating.
  * "To X" converts a file from the Old scale to the X scale (multiplies chart ratings by 1.5)
4. When done, click "Save Changes" to save the new ratings for any modified charts.





## Disclaimer:
I take no responsibility for what this code does or for the ratings it produces (it just multiplies/divides ratings by 1.5, lmao). It's not going to be as accurate as rerating charts by hand, but it'll at least be better than using a pack of charts where the pack maintainer used a different rating scale for each song. I don't make any warranties or guarantees behind this code - everything works as far as I know, but that's about all I can say about it.


Feel free to fork/modify/etc, and enjoy!
