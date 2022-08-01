using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace SCMKit.modules.github
{
    class RepoSearch
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("searchrepo", credential, url, options, system));



            try
            {

                await library.Utils.HeartbeatRequest(url);

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);

                // create table header
                string tableHeader = string.Format("{0,40} | {1,10} | {2,50}", "Name", "Visibility", "URL");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));


                SearchRepositoriesRequest s = new SearchRepositoriesRequest(options); // adding search string in initilization


                var sa = await client.Search.SearchRepo(s);
                int totalNumItems = sa.TotalCount;


                // if total number of results is past 100, then iterate through all pages
                if (totalNumItems >= 100)
                {

                    for (int i = 1; i < (totalNumItems / 100) + 2; i++)
                    {
                        SearchRepositoriesRequest searchString = new SearchRepositoriesRequest(options); // adding search string in initilization
                        searchString.Page = i;

                        var search = await client.Search.SearchRepo(searchString);
                        IReadOnlyList<Repository> currentSearch = search.Items;

                        foreach (Repository item in currentSearch)
                        {

                            string name = "";
                            string visibility = "Not Found";
                            string itemURL = "";

                            if (item.Name != null)
                            {
                                name = item.Name;
                            }
                            if (item.Visibility.HasValue)
                            {
                                visibility = item.Visibility.Value.ToString();
                            }

                            if (item.HtmlUrl != null)
                            {
                                itemURL = item.HtmlUrl.Replace(" ", "");
                            }

                            Console.WriteLine("{0,40} | {1,10} | {2,50}", name, visibility, itemURL);

                        }
                    }
                }

                // if number of results is less than 100, just display the 1 page
                else
                {

                    IReadOnlyList<Repository> currentSearch = sa.Items;


                    foreach (Repository item in currentSearch)
                    {

                        string name = "";
                        string visibility = "Unknown";
                        string itemURL = "";

                        if (item.Name != null)
                        {
                            name = item.Name;
                        }
                        if (item.Visibility.HasValue)
                        {
                            visibility = item.Visibility.Value.ToString();
                        }

                        if (item.HtmlUrl != null)
                        {
                            itemURL = item.HtmlUrl.Replace(" ", "");
                        }

                        Console.WriteLine("{0,40} | {1,10} | {2,50}", name, visibility, itemURL);
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("Total number of items matching search: " + totalNumItems);


            }

            catch (Exception ex)
            {

                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not perform repo search for search term given. Exception: " + ex.ToString());
                Console.WriteLine("");


            }

        }
    }
}
