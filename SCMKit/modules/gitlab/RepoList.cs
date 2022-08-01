using System;
using System.Threading.Tasks;
using GitLabApiClient;

namespace SCMKit.modules.gitlab
{
    class RepoList
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listrepo", credential, url, options, system));

            try
            {

                await library.Utils.HeartbeatRequest(url);

                // auth to GitLab and get list of all projects
                Task<GitLabClient> authTask = library.GitLabUtils.AuthToGitLabAsync(credential, url);
                GitLabClient client = authTask.Result;
                var projects = await client.Projects.GetAsync();

                // create table header
                string tableHeader = string.Format("{0,40} | {1,10} | {2,50}", "Name", "Visibility", "URL");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                foreach (var project in projects)
                {

                    if (options.ToLower().Equals("private") && project.Visibility.ToString().Equals("Private"))
                    {
                        Console.WriteLine("{0,40} | {1,10} | {2,50}", project.Name, project.Visibility, project.WebUrl);

                    }
                    else if (options.ToLower().Equals("public") && project.Visibility.ToString().Equals("Public"))
                    {
                        Console.WriteLine("{0,40} | {1,10} | {2,50}", project.Name, project.Visibility, project.WebUrl);
                    }
                    else if (options.ToLower().Equals("internal") && project.Visibility.ToString().Equals("Internal"))
                    {
                        Console.WriteLine("{0,40} | {1,10} | {2,50}", project.Name, project.Visibility, project.WebUrl);
                    }
                    else if (options.Equals(""))
                    {
                        Console.WriteLine("{0,40} | {1,10} | {2,50}", project.Name, project.Visibility, project.WebUrl);
                    }

                }

            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not retrieve listing of repos. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }


    }

}