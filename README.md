# HackerNews
Worked exercise for Santander
This sample uses a separate library to 
  1. Initial loading of best story ids and then loading all the stories for the ids from Hacker News into a cache
  2. These are loaded into a SortedDictionary collection
  3. The key is score and the value is the JSon of the story (although commentCount seems to be unavailable at HackerNews)
  4. A background task keeps polling for new story ids and loads it in the background and loads into a temp collection - this is to ensure the cache is not stale
  5. Temp collection is then offloaded into the collection that serves requests (concurrency handled)
  6. There is an event that is invoked once the cache is refreshed

If I had more time, I would look into
  1. Exception handling - at the moment, not upto par - this can be improved
  3. Logging - moderately, to ensure requests are handled with no lag
  4. Introduce custom caching instead of memory cache and hance improve concurrency when loading the cache
