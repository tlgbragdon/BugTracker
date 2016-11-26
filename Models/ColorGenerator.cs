using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;


namespace BugTracker.Models
{
    public class ColorGenerator
    {

        //Generate a random color
        public static string GenerateHexColor()
            {
                var random = new Random();  
                var hue = random.Next(0, 360);  // get random hue
                // maintain 75% saturation and 50% brightness value for all hues
                return string.Format("hsl({0},{1},{2})", hue, "75%", "50%");

            }
    }
}