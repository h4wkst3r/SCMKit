using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace SCMKit.modules.github
{
    class BranchProtection
    {



        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("protection", credential, url, options, system));

            try
            {
                await library.Utils.HeartbeatRequest(url);

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);

                // create table header
                string tableHeader = string.Format("{0,25} | {1,25} | {2,50}", "Repo", "Branch", "Protection");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                IRepositoryBranchesClient branchesClient =
                    new RepositoryBranchesClient(new ApiConnection(client.Connection));

                SearchRepositoriesRequest
                    searchString = new SearchRepositoriesRequest(); // adding search string in initilization
                searchString.Stars =
                    Range.GreaterThanOrEquals(0); // using this as a hack to be able to get all repos listed

                var sa = await client.Search.SearchRepo(searchString);
                int totalNumItems = sa.TotalCount;


                // if total number of results is past 100, then iterate through all pages
                if (totalNumItems >= 100)
                {

                    for (int i = 1; i < (totalNumItems / 100) + 2; i++)
                    {
                        searchString.Page = i;

                        var search = await client.Search.SearchRepo(searchString);
                        IReadOnlyList<Repository> currentSearch = search.Items;

                        foreach (Repository repo in currentSearch)
                        {
                            var branches = branchesClient.GetAll(repo.Id);
                            foreach (Branch branch in branches.Result)
                            {
                                string protectionSettings = $"Protected: {branch.Protected.ToString(),-56}";
                                if (branch.Protected)
                                {
                                    var branchProtection =
                                        await branchesClient.GetBranchProtection(repo.Id, branch.Name);
                                    if (branchProtection != null)
                                    {
                                        if (branchProtection.Restrictions != null)
                                        {
                                            protectionSettings +=
                                                $"\n{"",56}Push restricted to:";
                                            if (branchProtection.Restrictions.Users.Count > 0)
                                            {
                                                protectionSettings +=
                                                    $"\n{"",56}  Users:";
                                                foreach (var user in branchProtection.Restrictions.Users)
                                                    protectionSettings +=
                                                        $"\n{"",56}    {user.Login}";
                                            }

                                            if (branchProtection.Restrictions.Teams.Count > 0)
                                            {
                                                protectionSettings +=
                                                    $"\n{"",56}  Teams:";
                                                foreach (var team in branchProtection.Restrictions.Teams)
                                                    protectionSettings +=
                                                        $"\n{"",56}    {team.Name}";
                                            }
                                        }

                                        if (branchProtection.RequiredStatusChecks != null)
                                        {
                                            protectionSettings +=
                                                $"\n{"",56}Status checks must pass before merge:";
                                            if (branchProtection.RequiredStatusChecks.Strict)
                                                protectionSettings +=
                                                    $"\n{"",56}  Branch must be up-to-date before merge: {branchProtection.RequiredStatusChecks.Strict}";
                                            if (branchProtection.RequiredStatusChecks.Contexts != null)
                                            {
                                                foreach (string statusCheck in branchProtection.RequiredStatusChecks
                                                             .Contexts)
                                                    protectionSettings +=
                                                        $"\n{"",56}\n  {statusCheck}";
                                            }
                                        }

                                        if (branchProtection.RequiredPullRequestReviews != null)
                                        {
                                            if (branchProtection.RequiredPullRequestReviews.RequireCodeOwnerReviews)
                                                protectionSettings +=
                                                    $"\n{"",56}Owner review required before merge: {branchProtection.RequiredPullRequestReviews.RequireCodeOwnerReviews}";
                                            if (branchProtection.RequiredPullRequestReviews
                                                    .RequiredApprovingReviewCount != 0)
                                                protectionSettings +=
                                                    $"\n{"",56}Approvals required before merge: {branchProtection.RequiredPullRequestReviews.RequiredApprovingReviewCount}";
                                        }

                                        if (branchProtection.EnforceAdmins.Enabled)
                                            protectionSettings +=
                                                $"\n{"",56}Protections apply to repo admins: {branchProtection.EnforceAdmins.Enabled}";
                                    }
                                }

                                Console.WriteLine("{0,25} | {1,25} | {2,50}", repo.Name, branch.Name,
                                    protectionSettings);
                            }
                        }
                    }
                }

                // if number of results is less than 100, just display the 1 page
                else
                {
                    var search = await client.Search.SearchRepo(searchString);
                    IReadOnlyList<Repository> currentSearch = search.Items;

                    foreach (Repository repo in currentSearch)
                    {
                        var branches = branchesClient.GetAll(repo.Id);
                        foreach (Branch branch in branches.Result)
                        {
                            string protectionSettings = $"Protected: {branch.Protected.ToString(),-56}";
                            if (branch.Protected)
                            {
                                var branchProtection = await branchesClient.GetBranchProtection(repo.Id, branch.Name);
                                if (branchProtection != null)
                                {
                                    if (branchProtection.Restrictions != null)
                                    {
                                        protectionSettings +=
                                            $"\n{"",56}Push restricted to:";
                                        if (branchProtection.Restrictions.Users.Count > 0)
                                        {
                                            protectionSettings +=
                                                $"\n{"",56}  Users:";
                                            foreach (var user in branchProtection.Restrictions.Users)
                                                protectionSettings +=
                                                    $"\n{"",56}    {user.Login}";
                                        }

                                        if (branchProtection.Restrictions.Teams.Count > 0)
                                        {
                                            protectionSettings +=
                                                $"\n{"",56}  Teams:";
                                            foreach (var team in branchProtection.Restrictions.Teams)
                                                protectionSettings +=
                                                    $"\n{"",56}    {team.Name}";
                                        }
                                    }

                                    if (branchProtection.RequiredStatusChecks != null)
                                    {
                                        protectionSettings +=
                                            $"\n{"",56}Status checks must pass before merge:";
                                        if (branchProtection.RequiredStatusChecks.Strict)
                                            protectionSettings +=
                                                $"\n{"",56}  Branch must be up-to-date before merge: {branchProtection.RequiredStatusChecks.Strict}";
                                        if (branchProtection.RequiredStatusChecks.Contexts != null)
                                        {
                                            foreach (string statusCheck in branchProtection.RequiredStatusChecks
                                                         .Contexts)
                                                protectionSettings +=
                                                    $"\n{"",56}\n  {statusCheck}";
                                        }
                                    }

                                    if (branchProtection.RequiredPullRequestReviews != null)
                                    {
                                        if (branchProtection.RequiredPullRequestReviews.RequireCodeOwnerReviews)
                                            protectionSettings +=
                                                $"\n{"",56}Owner review required before merge: {branchProtection.RequiredPullRequestReviews.RequireCodeOwnerReviews}";
                                        if (branchProtection.RequiredPullRequestReviews.RequiredApprovingReviewCount !=
                                            0)
                                            protectionSettings +=
                                                $"\n{"",56}Approvals required before merge: {branchProtection.RequiredPullRequestReviews.RequiredApprovingReviewCount}";
                                    }

                                    if (branchProtection.EnforceAdmins.Enabled)
                                        protectionSettings +=
                                            $"\n{"",56}Protections apply to repo admins: {branchProtection.EnforceAdmins.Enabled}";
                                }
                            }

                            Console.WriteLine("{0,25} | {1,25} | {2,50}", repo.Name, branch.Name, protectionSettings);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Exception: " + ex.ToString());
                Console.WriteLine("");
            }
        }
    }
}
