using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace SCMKit.modules.github
{

    // TODO 

    class FileSearch
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("searchfile", credential, url, options, system));



            try
            {
                await library.Utils.HeartbeatRequest(url);

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);

                SearchCodeRequest s = new SearchCodeRequest(options); // adding search string in initilization
                s.In = new[] { CodeInQualifier.Path };

                var sa = await client.Search.SearchCode(s);
                int totalNumItems = sa.TotalCount;
                int matchCount = 0;

                // if total number of results is past 100, then iterate through all pages
                if (totalNumItems >= 100)
                {

                    for (int i = 1; i < (totalNumItems / 100) + 2; i++)
                    {
                        SearchCodeRequest searchString = new SearchCodeRequest(options); // adding search string in initilization
                        searchString.In = new[] { CodeInQualifier.Path };
                        searchString.Page = i;

                        var search = await client.Search.SearchCode(searchString);
                        IReadOnlyList<SearchCode> currentSearch = search.Items;

                        foreach (SearchCode item in currentSearch)
                        {
                            if (item.Name.ToLower().Contains(options.ToLower()))
                            {
                                Console.WriteLine("\n[>] REPO: " + item.Repository.HtmlUrl);
                                Console.WriteLine("    [>] FILE: " + item.HtmlUrl.Replace(" ", ""));
                                matchCount++;
                            }
                        }
                    }
                }

                // if number of results is less than 100, just display the 1 page
                else
                {
                    SearchCodeRequest searchString = new SearchCodeRequest(options); // adding search string in initilization
                    searchString.In = new[] { CodeInQualifier.Path };


                    var search = await client.Search.SearchCode(searchString);
                    IReadOnlyList<SearchCode> currentSearch = search.Items;



                    foreach (SearchCode item in currentSearch)
                    {

                        if (item.Name.ToLower().Contains(options.ToLower()))
                        {
                            Console.WriteLine("\n[>] REPO: " + item.Repository.HtmlUrl);
                            Console.WriteLine("    [>] FILE: " + item.HtmlUrl.Replace(" ", ""));
                            matchCount++;
                        }
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("Total number of items matching search: " + matchCount);

            }

            catch (Exception ex)
            {

                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not perform file search for search term given. Exception: " + ex.ToString());
                Console.WriteLine("");


            }

        }
    }
}
