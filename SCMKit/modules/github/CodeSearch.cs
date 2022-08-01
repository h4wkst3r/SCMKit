using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;

namespace SCMKit.modules.github
{
    class CodeSearch
    {


        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("searchcode", credential, url, options, system));

            try
            {
                await library.Utils.HeartbeatRequest(url);

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);

                SearchCodeRequest s = new SearchCodeRequest(options); // adding search string in initilization

                var sa = await client.Search.SearchCode(s);
                int totalNumItems = sa.TotalCount;


                // if total number of results is past 100, then iterate through all pages
                if (totalNumItems >= 100)
                {

                    for (int i = 1; i < (totalNumItems / 100) + 2; i++)
                    {
                        SearchCodeRequest searchString = new SearchCodeRequest(options); // adding search string in initilization
                        searchString.Page = i;

                        var search = await client.Search.SearchCode(searchString);
                        IReadOnlyList<SearchCode> currentSearch = search.Items;

                        foreach (SearchCode item in currentSearch)
                        {
                            Console.WriteLine("\n[>] URL: " + item.HtmlUrl.Replace(" ", ""));
                            await getLinesInFile(credential, item.GitUrl, options);
                        }
                    }
                }

                // if number of results is less than 100, just display the 1 page
                else
                {
                    SearchCodeRequest searchString = new SearchCodeRequest(options); // adding search string in initilization

                    var search = await client.Search.SearchCode(searchString);
                    IReadOnlyList<SearchCode> currentSearch = search.Items;



                    foreach (SearchCode item in currentSearch)
                    {
                        Console.WriteLine("\n[>] URL: " + item.HtmlUrl.Replace(" ", ""));
                        await getLinesInFile(credential, item.GitUrl, options);
                    }
                }


                Console.WriteLine("");
                Console.WriteLine("Total number of items matching search: " + totalNumItems);

            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not results of search. Exception: " + ex.ToString());
                Console.WriteLine("");
            }
        }

        public static async Task getLinesInFile(String credential, String url, String sTerm)
        {
            try
            {
                library.WebUtils.IgnoreSSL();
                var webRequest = library.WebUtils.GenerateRawFileWebRequest(credential, url);
                var versionResponse = await library.WebUtils.GetRequestResponseString(webRequest);

                String sPattern = "(.+|)" + sTerm + "(.+|)";
                MatchCollection match = Regex.Matches(versionResponse, sPattern, RegexOptions.IgnoreCase);

                if (match.Count > 0)
                {
                    Console.WriteLine("[*] Match count : " + match.Count);
                    foreach (Match m in match)
                    {
                        Console.WriteLine("    |_ " + m.Value.Trim());
                    }
                }
                else
                {
                    Console.WriteLine("[+] Regex did not match file content..");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not retrieve file(s) for parsing. Exception: " + ex.ToString());
                Console.WriteLine("");
            }
        }
    }
}
