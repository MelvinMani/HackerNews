using System.Text.Json;
namespace StoryLibrary
{
    public class HackerNews
    {
        private bool isInitialCacheLoadingDone = false;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private SortedDictionary<int, Story> sortedStoriesDict;
        private SortedDictionary<int, Story> sortedTempStoriesDict;
        private readonly object lockObject = new object();
        private const string BestStoriesUrl = "https://hacker-news.firebaseio.com/v0/beststories.json";
        private const string StoryUrl = "https://hacker-news.firebaseio.com/v0/item/";
        private const string StoryUrlExtensionJson = ".json";
        public event EventHandler StoriesRefreshed;

        public HackerNews()
        {
            sortedStoriesDict = new SortedDictionary<int, Story>(new RepeatableKeyComparer<int>());
            sortedTempStoriesDict = new SortedDictionary<int, Story>(new RepeatableKeyComparer<int>());
        }
    
        public void LoadCaches()
        {
            LoadCache(true);
            LoadCache(false);
        }

        public SortedDictionary<int, Story> GetCache()
        {
            return sortedStoriesDict;
        }

        public void LoadCache(bool isInitialLoad)
        {
            if (isInitialLoad)
            {
                Console.WriteLine("Initial load into cache");
                RetrieveStories();
                isInitialCacheLoadingDone = true;
            }
            else
            {
                try
                {
                    Task task = Task.Run(() => PollForStories(10000, cancellationTokenSource.Token));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Poller issue. Error: " + ex.ToString());
                }
            }
        }

        public string GetStory(int numberOfStories)
        {
            if (Monitor.TryEnter(lockObject, 1000))
            {
                if (isInitialCacheLoadingDone)
                {
                    if (numberOfStories > 0)
                    {
                        if (sortedStoriesDict.Count > 0)
                        {
                            List<Story> requestedStoryList = sortedStoriesDict.TakeLast(numberOfStories).Select(storyjs => storyjs.Value).ToList();
                            var serialisedStories = JsonSerializer.Serialize(requestedStoryList, new JsonSerializerOptions() { WriteIndented = true });
                            return serialisedStories;
                        }
                        else
                        {
                            return "No Stories available";
                        }
                    }
                    else
                    {
                        return "No story requested";
                    }
                }
                else
                {
                    return "Initial Cache still loading";
                }
            }
            else
            {
                return "Stories are being refreshed try again later";
            }
        }
        private void PollForStories(int pollIntervalInMilliSeconds, CancellationToken cancelToken)
        {
            //TODO: Check the exception handling inthis method especially around cancel token operation canceled exception
            try
            {
                Task.Run(() =>
                {
                    Console.WriteLine("Polling for stories, PollInterval In MilliSeconds: " + pollIntervalInMilliSeconds);
                    while (true)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            Console.WriteLine("Cancellation Requested ");
                            cancelToken.ThrowIfCancellationRequested();
                        }
                        RetrieveStories();
                        Thread.Sleep(pollIntervalInMilliSeconds);
                    }
                }, cancellationTokenSource.Token);
            }
            catch (AggregateException aex)
            {
                Console.WriteLine("Aggregate exception caught in runloop: " + aex.Message);
            }
            catch (OperationCanceledException ocex)
            {
                Console.WriteLine("OperationCanceldException exception caught in runloop: " + ocex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception exception caught in runloop: " + ex.Message);
            }
        }

        public async Task<int[]?> RetrieveBestStoryIds()
        {
            using HttpClient client = new HttpClient();
            string response = await client.GetStringAsync(BestStoriesUrl);
            var bestStoryIds = JsonSerializer.Deserialize<int[]>(response);
            return bestStoryIds;
        }

        private async Task<Story> RetrieveStory(int id)
        {
            string url = StoryUrl + id + StoryUrlExtensionJson;
            using HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(url);
            var story = JsonSerializer.Deserialize<Story>(response);

            return story == null ? new Story() : story;
        }

        private void RetrieveBestStories(int[] bestStoryIds)
        {
            sortedTempStoriesDict.Clear();

            if (bestStoryIds != null && bestStoryIds.Length > 0)
            {
                foreach (var bestStoryId in bestStoryIds)
                {
                    if (bestStoryId != 0)
                    {
                        Task<Story> storyTask = RetrieveStory(bestStoryId);
                        while (!storyTask.IsCompleted)
                        {
                            //log information or yield return information to the caller
                        }

                        if (storyTask.IsFaulted)
                        {
                            //Log error
                            Console.WriteLine("Error retrieveing story: " + bestStoryId);
                        }
                        else
                        {
                            if (storyTask.IsCompletedSuccessfully)
                            {
                                if (storyTask.Result != null)
                                {
                                    sortedTempStoriesDict.Add(storyTask.Result.score, storyTask.Result);
                                }
                            }
                        }
                    }
                }
            }

            if (sortedTempStoriesDict.Count > 0)
            {
                lock (lockObject)
                {
                    sortedStoriesDict.Clear();
                    foreach (var story in sortedTempStoriesDict)
                    {
                        sortedStoriesDict.Add(story.Key, story.Value);
                    }
                }
                StoriesRefreshed?.Invoke(this, EventArgs.Empty);
            }

            //Log info
            //Console.WriteLine("Loaded All best Stories into cache..");
        }

        private void RetrieveStories()
        {
            Console.WriteLine("Retrieveing best stories Ids... ");
            Task<int[]?> bestStoryIdsTask = RetrieveBestStoryIds();
            while (!bestStoryIdsTask.IsCompleted)
            {
                //Log if required
                //Update on progress using yeild return
            }

            if (bestStoryIdsTask.IsFaulted)
            {
                //Log error
                //Console.WriteLine(bestStoryIdsTask.Exception.Message);
            }
            else
            {
                var bestStoryIds = bestStoryIdsTask.Result;
                //Log info
                if (bestStoryIds != null && bestStoryIds.Length > 0)
                {
                    Console.WriteLine("Best Story Ids loaded: " + bestStoryIds.Count());
                    RetrieveBestStories(bestStoryIds);
                }
                else
                {
                    //Log info
                    Console.WriteLine("No best stories available at the moment");
                }
            }
        }
    }
}