using PumpjackPipeOptimizer.Data;
using PumpjackPipeOptimizer.Steps;

namespace PumpjackPipeOptimizer;

internal partial class Program
{
    private static void Main(string[] args)
    {
        /*
        var options = new Options
        {
            ElectricPoleEntityName = EntityNames.Vanilla.Substation,
            ElectricPoleSupplyWidth = 18,
            ElectricPoleSupplyHeight = 18,
            ElectricPoleWireReach = 18,
            ElectricPoleWidth = 2,
            ElectricPoleHeight = 2,
        };
        */
        /*
        var options = new Options
        {
            ElectricPoleEntityName = EntityNames.SpaceExploration.SmallIronElectricPole,
            ElectricPoleSupplyWidth = 5,
            ElectricPoleSupplyHeight = 5,
            ElectricPoleWireReach = 7.5,
            ElectricPoleWidth = 1,
            ElectricPoleHeight = 1,
        };
        */
        /*
        var options = new Options
        {
            ElectricPoleEntityName = EntityNames.Vanilla.MediumElectricPole,
            ElectricPoleSupplyWidth = 7,
            ElectricPoleSupplyHeight = 7,
            ElectricPoleWireReach = 9,
            ElectricPoleWidth = 1,
            ElectricPoleHeight = 1,
        };
        */
        var options = new Options
        {
            ElectricPoleEntityName = EntityNames.Vanilla.BigElectricPole,
            ElectricPoleSupplyWidth = 4,
            ElectricPoleSupplyHeight = 4,
            ElectricPoleWireReach = 30,
            ElectricPoleWidth = 2,
            ElectricPoleHeight = 2,
        };

        // var blueprint = "0eNqVlNtqhDAQht9lrkMxbpJRX6WU4rqhpF2jeCgVybs3Udos3Wx39kIwMvn855/DCsfzrPvB2AmqFUzT2RGq5xVG82brc/g2Lb2GCsykW2Bg6zac+rnt3+vmAxwDY0/6CyruXhhoO5nJ6J2xHZZXO7dHPfiA69sM+m70Fzob/uQhqiieJIPFR6Pwr86xK05O4ZTylyOzNOdA4GCWRz1lmiMoeqSIekSaI0n+yLscReGo6LOUaQ6S9ER/pEpzCpKe6I8K9WJwMoNu9hCVoJYUKmJUd6N6PCOlqf6Rl6ewpG4Pon6w/Ia+nNSm2X3QgeTYIYLE30RFCitI+i6wiuSffDTtgtI1nDQcGIcMBUktaVYwzlxozGSRigfTxq2t/ebd9nN1sc4ZfOph3CUXXGCZo5L+4ejcN5pB4x0=";
        // var blueprint = "0eNqtmdtum0AQht9lr3HFHmYPfpXKqhxnk9LaYAGOGkV+9y6xVAjZyTDUF5FiBN8Mu/8cdngTD8dLPLdV3Yvtm6gOTd2J7fc30VXP9f44XOtfz1FsRdXHkyhEvT8Nv86X0/nX/vBbXAtR1Y/xj9jKa0E/VqWL4yPquitErPuqr+LN7PuP1x/15fQQ28T8bLAQ56ZLDzT1YCVBtNbfoBCv6e7g0r8J/1i18XC7xRTvLnTDzfHpabj8kixsTs3j5Rg3enBicHxmWC0xDGE0bOaG7TrD+p/h7rQ/HjdV29SbeEyPt9Vhc26OMbcC5dyROlbPPx+aSzssq1K7jCXzcVe+XldI1AwDSIYiGZZkGJLhSAaQDE8yLMkIJMORDFmSEE9DJAkJNERREFPSEE1CJA0hxWpooUlSrYZWvCTlami5SlKvhtarJAVrFoiNVKyhVa/KNUkL3NdJK+SSliJ1DfRLK7UqyU5k6j77qwsDBeis05qTJi3itOFAHAKBRQV1khr8neqashy1YmtAx02gIXTceBoSVkn+awmlOIJsqdZkSZiSEZc1HTolDSFLAmgaQsYD0PGgyXgAoCFkSQBDQywnJ2EQUtpgaYhfEt5qEiNhHt5qZdtKdz7yY1LJdaQlpyUNCERyIJgnipNsMYjmdKXY6xhOe4xBYFXBs/MyMDtVFCCzBwvL6aQxlx2nlcYgnqEFXyKQwIFI5KBULiq847p4dafCC3LV3vuPrzTb+yQoB7m9h1Wt1aTbzVlLxRaxtuq0PMl/Xmd17bO6Bs6J+X3/chDgQDQC4ZyZUYhbIslJ++/vNeOAZWVqjG9v71SmgIzlqTiwgQVdpkoaIhkFE4UojpgMAtEcCOYJ53SCQoAjawxiGVUXhThGCUMhnlHCUEhgTIMwiCsZ0yAUIhmDHBTCGSmhEM0Y5KAQzkgJhaxqrCYTIp+ZfKSKnT8GOs7sCXXZcbIXctBwngNBhhEucCBIk+1ZyRjpE738zz4ilJlt1EUwuW30iuFyQBpKrzkQpLX1ZlEhHhNMMHcqxB447iO9lLccCNIG+TEeur5p989x0+/r7DqMwRWwbwicuEAhnLgISFUPZFxM+quAhHkgU76yNEQxei4UQsp9qlIMYlaF+QScGdrlzwqBpW/MX5a+kSwbFrX5032810woeM7GI/k9BM7Ge+yTGqdK3Ci720unJ8Yv5IV4iW13WxMvjQvKWUh/0l2vfwHRKsnH";
        // var blueprint = "0eNqtmttum0AQht9lr6FiD7OHvEplVY5NXFobLIyjRpHfvYstFUx2PAz1RaQYwTfD7j+HHftTvO7P5bGt6k68fIpq09Qn8fL9U5yqXb3e99e6j2MpXkTVlQeRiXp96D8dz4fjr/Xmt7hkoqq35R/xIi8Z/VgVLw6PqMsqE2XdVV1V3sxeP3z8qM+H17KNzK8GM3FsTvGBpu6tRIjW+htk4iPeHVz8N+K3VVtubreY7OrCqb+5fHvrL79HC/mh2Z73Za57J3rHJ4bVHMMQBsNmatguM6z/GT4d1vt9XrVNnZf7+HhbbfJjsy9TK1BMHanLavfztTm3/bIqtUpYMve78nhdIVITDCAZimRYkmFIhiMZQDI8ybAkI5AMRzJkQUI8DZEkJNAQRUFMQUM0CZE0hBSroYUmSbUaWvGSlKuh5SpJvRpar5IUrJkhNlKxhla9KpYkLXCPk1ZIJS1F6hrol1ZqUZIdydR99VdnBjLQSac1J01axGnDgTgEArMK6ig1+CfVNWU5asXWgI6bQEPouPE0JCyS/GMJxTiCZKnWZEkYkxGXNR06BQ0hSwJoGkLGA9DxoMl4AKAhZEkAQ0MsJydhEFLaYGmInxPeahQjYRreamHbSnc+8j6ppDrSgtOSBgQiORDME8VJthhEc7pS7HUMpz3GILCo4NlpGZicKjKQyYOF5XTSmMuO00pjEM/Qgi8QSOBAJHJQKmYV3mFdvHpS4QW5aO/9/StN9j4KykFq72FRazXqdlPWYrFFrC06LY/yn9dJXfukroFzYr7uXwoCHIhGIJwzMwpxcyQ5av/9s2YcMK9MDfHt7ZPKFJCxPBYHNrCgy1RBQySjYKIQxRGTQSCaA8E84ZxOUAhwZI1BLKPqohDHKGEoxDNKGAoJjGkQBnEFYxqEQiRjkINCOCMlFKIZgxwUwhkpoZBFjdVoQuQTkw/AGivHmT2hLjtO9kIOGs5zIMgwwgUOBGmyPSsZI32il//ZR4QisY06Cy61jV4xXA5IQ+k1B4K0tt7MKsRDggnmSYXYA8d9pJfylgNB2iA/xMOpa9r1rsy7dZ1chyG4AvYdAicuUAgnLgJS1cN9XORdk+/a5lxvH1fVq0/3PV4KLhlwoH1VDJx94KtKwclAGTWaAcl3gSwWytIQYDSfKITU/DhcMYhblO9G4MT0Mn1oCqyIwPxlRYTDvlcrlqn2y+HDJOmLashoqJdaU1nYVdLYrO+nx5p81qBPFpqjYo/theHIGKWwqghKsYt2Tk8Xd7JzMRxWtzWO4OFXFpl4L9vTbQu8NC4oZyH+SXe5/AUrKYLb";
        // var blueprint = "0eNqV1NtugzAMANB/8XOomhu3X6mqidJ0ygYBQaiGUP59hErNtAbNe+Ti48SOs8ClmVQ/aGOhXEDXnRmhPC0w6ndTNf6dnXsFJWirWiBgqtY/9VPbf1T1JzgC2lzVF5TUnQkoY7XV6mFsD/ObmdqLGtYfXqMJ9N24BnTGZ1qRhAt6kARmKJlk2UE6R14ghoIof0KcxSGOgngeIB6HBG5rxwDlcUiiIBZWJPzWyNad0X9Xt5uqrb6vaNJ216lRCfe9ieRKcWWUIdfO7jNcGdMAyTiUoyAZTogo4lCB6wd7QnLnhNDjfxviV/d3Q1g0GXJCQiGl2Fk2bkTkD2ln1ihuRliQUroj4YaE0V/S+VHMNSxcVQTuahi3MJZTkRUsS9frIqWZc9/GK4Yy";
        var blueprintString = "0eNqV1O1qgzAUBuB7Ob+z0nxrbmWMYbswstUY/BgVyb1XHTRlTeDsZ8TzmPN6kgVOl8mG3vkRzALu3PkBzOsCg/v0zWV7Ns7BggE32hYI+KbdVmFqw1dz/oZIwPkPewVD4xsB60c3Ovtr7Iv53U/tyfbrC8/VBEI3rAWd3760Ii9c0IMkMINhkumDjJE8QQwFUX6HOMtDHAXxKkE8Dwlca8cEVXlIoiCWdiQKrSlcRjJBhdY0LiOVIJmHKhQk0+8XdR6qcWGzOyQLGdHjf9PedpeVkLOdUpKiIOGGWz5IhVNCcdPNkqRK3eHGm9E/0nof7LeGebhkCPzYftjLWEWFrplW60FXVMd4AxsSbeE=";
        // var blueprintString = "0eNpt0MtuwyAQBdB/mTWOZBzilF+prAg7o3RaXgJc1bL494CrPuMFi0HMueiuMOoZfSCbQK5Ak7MR5PMKkW5W6XqXFo8ggRIaYGCVqZOfjX9V0xtkBmSv+AGyzQMDtIkS4aexDcvFzmbEUB48bjPwLpYFZ2tSQZqOnw+CwQKSiyM/iJzZA8S/oWiU1g0FZxvUOKVAU+Odxl1Y/IULgnR7Gd0c6ne7YSep+0max5jUBu7Z3Zd8+u/yofaytSd/lc3gHUPcDH5uj/0T70+inLbP+Q5xpYON";

        var inputBlueprint = ParseBlueprint.Execute(blueprintString);

        var context = InitializeContext.Execute(options, inputBlueprint);

        if (context.Centers.Count < 2)
        {
            throw new InvalidOperationException("The must be at least two pumpjacks in the blueprint.");
        }

        // Use Dijksta's algorithm to add good connecting pipes to the grid.
        var pipes = AddPipes.Execute(context);

        // Find pipe "squares" (four pipes forming a square) and try to remove one from the square.
        PruneSquares.Execute(context, pipes);

        if (context.Options.UseUndergroundPipes)
        {
            // Substitute long stretches of pipes for underground pipes
            UseUndergroundPipes.Execute(context, pipes);
        }

        // Add electric poles to the grid.
        AddElectricPoles.Execute(context);

        Console.WriteLine();
        context.Grid.WriteTo(Console.Out);

        var newBlueprint = GridToBlueprintString.Execute(context);
        Console.WriteLine();
        Console.WriteLine(newBlueprint);
    }
}
