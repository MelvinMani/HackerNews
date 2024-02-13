using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryLibrary
{
    public class Story
    {
        public string? title { get; set; }
        public string? by { get; set; }
        public string? url { get; set; }
        public int score { get; set; }
        public int commentCount { get; set; }
        //public string time { get; set; }
    }
}
