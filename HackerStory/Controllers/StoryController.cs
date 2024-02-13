using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StoryLibrary;
using System.Text.Json;

namespace HackerStory.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoryController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public StoryController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet(Name = "GetStories")]
        public string GetStories(int numberOfStories)
        {
            if (numberOfStories >= 0)
            {
                var sortedStoriesDict = (SortedDictionary<int, Story>)_cache.Get("Stories");
                List<Story> requestedStoryList = sortedStoriesDict.TakeLast(numberOfStories).Select(storyjs => storyjs.Value).ToList();
                var serialisedStories = JsonSerializer.Serialize(requestedStoryList, new JsonSerializerOptions() { WriteIndented = true });
                return serialisedStories;
            }
            else
            {
                return "No story requested";
            }
        }
    }
}
