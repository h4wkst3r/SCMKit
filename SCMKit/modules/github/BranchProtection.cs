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
                string tableHeader = string.Format("{0,-30} | {1,-30} | {2,-40}", "Repo", "Branch", "Protection");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                IRepositoryBranchesClient branchesClient =
                    new RepositoryBranchesClient(new ApiConnection(client.Connection));

                SearchRepositoriesRequest searchString = new SearchRepositoriesRequest(); // adding search string in initilization
                if (!string.IsNullOrEmpty(options))
                {
                    searchString = new SearchRepositoriesRequest(options); // adding search string in initilization
                }
                searchString.Stars = Range.GreaterThanOrEquals(0); // using this as a hack to be able to get all repos listed

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
                                string protectionSettings = $"- Protected: {branch.Protected.ToString(),-58}";
                                if (branch.Protected)
                                {
                                    protectionSettings = $"+ Protected: {branch.Protected.ToString(),-58}";
                                    BranchProtectionSettings branchProtection = new BranchProtectionSettings();
                                    try
                                    {
                                        branchProtection = await branchesClient.GetBranchProtection(repo.Id, branch.Name);
                                    }
                                    catch (NotFoundException ex)
                                    {
                                        protectionSettings +=
                                            $"\n{" + ",58}Insufficient privileges to list branch protection rules";
                                    }
                                    if (branchProtection != null)
                                    {
                                        // List branch protection rules in the order they are displayed in the GitHub web console
                                        if (branchProtection.RequiredPullRequestReviews != null)
                                        {
                                            if (branchProtection.RequiredPullRequestReviews.RequiredApprovingReviewCount !=
                                                0)
                                                protectionSettings +=
                                                    $"\n{" + ",58}Approvals required before merge: {branchProtection.RequiredPullRequestReviews.RequiredApprovingReviewCount}";
                                            if (branchProtection.RequiredPullRequestReviews.RequireCodeOwnerReviews)
                                                protectionSettings +=
                                                    $"\n{" + ",58}Owner review required before merge: {branchProtection.RequiredPullRequestReviews.RequireCodeOwnerReviews}";
                                            // Add "Allow specified actors to bypass required pull requests" setting when added to Octokit (not supported as of 8/30/22, but supported by GitHub API bypass_pull_request_allowances property)
                                        }
                                        
                                        if (branchProtection.RequiredStatusChecks != null)
                                        {
                                            protectionSettings +=
                                                $"\n{" + ",58}Status checks must pass before merge:";
                                            if (branchProtection.RequiredStatusChecks.Strict)
                                                protectionSettings +=
                                                    $"\n{" + ",58}  Branch must be up-to-date before merge: {branchProtection.RequiredStatusChecks.Strict}";
                                            if (branchProtection.RequiredStatusChecks.Contexts != null)
                                            {
                                                // Context is deprecated, replace with Checks when added to Octokit (not supported as of 8/30/22, but supported by GitHub API)
                                                foreach (string statusCheck in branchProtection.RequiredStatusChecks.Contexts)
                                                    protectionSettings +=
                                                        $"\n{" + ",58}\n  {statusCheck}";
                                            }
                                        }

                                        if (branchProtection.RequiredConversationResolution != null)
                                            protectionSettings += $"\n{" + ",58}Require conversation resolution: {branchProtection.RequiredConversationResolution.Enabled}";
                                        
                                        if (branchProtection.RequiredSignatures != null)
                                            protectionSettings += $"\n{" + ",58}Require signed commits: {branchProtection.RequiredSignatures.Enabled}";
                                        
                                        if (branchProtection.RequiredLinearHistory != null)
                                            protectionSettings += $"\n{" + ",58}Require linear history: {branchProtection.RequiredLinearHistory.Enabled}";
                                        
                                        // Add "Require deployments to succeed before merging" setting when added to GitHub API and Octokit (not supported as of 8/30/22)

                                        if (branchProtection.EnforceAdmins != null)
                                        {
                                            if (branchProtection.EnforceAdmins.Enabled)
                                                protectionSettings +=
                                                    $"\n{" + ",58}Protections apply to repo admins: {branchProtection.EnforceAdmins.Enabled}";
                                        }
                                        
                                        if (branchProtection.Restrictions != null)
                                        {
                                            protectionSettings +=
                                                $"\n{" + ",58}Push restricted to:";
                                            if (branchProtection.Restrictions.Users.Count > 0)
                                            {
                                                protectionSettings +=
                                                    $"\n{" + ",58}  Users:";
                                                foreach (var user in branchProtection.Restrictions.Users)
                                                    protectionSettings +=
                                                        $"\n{" + ",58}    {user.Login}";
                                            }

                                            if (branchProtection.Restrictions.Teams.Count > 0)
                                            {
                                                protectionSettings +=
                                                    $"\n{" + ",58}  Teams:";
                                                foreach (var team in branchProtection.Restrictions.Teams)
                                                    protectionSettings +=
                                                        $"\n{" + ",58}    {team.Name}";
                                            }
                                            // Add Apps when supported by Octokit (not supported as of 8/30/22, but supported by GitHub API)
                                        }
                                        
                                        if (branchProtection.AllowForcePushes != null)
                                            protectionSettings += $"\n{" + ",58}Allow force pushes: {branchProtection.AllowForcePushes.Enabled}";
                                        
                                        if (branchProtection.AllowDeletions != null)
                                            protectionSettings += $"\n{" + ",58}Allow deletions: {branchProtection.AllowDeletions.Enabled}";
                                        
                                        if (branchProtection.BlockCreations != null)
                                            protectionSettings += $"\n{" + ",58}Block creations: {branchProtection.BlockCreations.Enabled}";
                                    }
                                }

                                Console.WriteLine("{0,-30} | {1,-30} | {2,-40}", repo.Name, branch.Name, protectionSettings);
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
                        var rLen = repo.Name.Length;
                        var branches = branchesClient.GetAll(repo.Id);
                        foreach (Branch branch in branches.Result)
                        {
                            var bLen = branch.Name.Length;
                            string protectionSettings = $"- Protected: {branch.Protected.ToString(),-58}";
                            if (branch.Protected)
                            {
                                protectionSettings = $"+ Protected: {branch.Protected.ToString(),-58}";
                                BranchProtectionSettings branchProtection = new BranchProtectionSettings();
                                try
                                {
                                    branchProtection = await branchesClient.GetBranchProtection(repo.Id, branch.Name);
                                }
                                catch (NotFoundException ex)
                                {
                                    protectionSettings +=
                                        $"\n{" + ",58}Insufficient privileges to list branch protection rules";
                                }
                                if (branchProtection != null)
                                {
                                    // List branch protection rules in the order they are displayed in the GitHub web console
                                    if (branchProtection.RequiredPullRequestReviews != null)
                                    {
                                        if (branchProtection.RequiredPullRequestReviews.RequiredApprovingReviewCount !=
                                            0)
                                            protectionSettings +=
                                                $"\n{" + ",58}Approvals required before merge: {branchProtection.RequiredPullRequestReviews.RequiredApprovingReviewCount}";
                                        if (branchProtection.RequiredPullRequestReviews.RequireCodeOwnerReviews)
                                            protectionSettings +=
                                                $"\n{" + ",58}Owner review required before merge: {branchProtection.RequiredPullRequestReviews.RequireCodeOwnerReviews}";
                                        // Add "Allow specified actors to bypass required pull requests" setting when added to Octokit (not supported as of 8/30/22, but supported by GitHub API bypass_pull_request_allowances property)
                                    }
                                    
                                    if (branchProtection.RequiredStatusChecks != null)
                                    {
                                        protectionSettings +=
                                            $"\n{" + ",58}Status checks must pass before merge:";
                                        if (branchProtection.RequiredStatusChecks.Strict)
                                            protectionSettings +=
                                                $"\n{" + ",58}  Branch must be up-to-date before merge: {branchProtection.RequiredStatusChecks.Strict}";
                                        if (branchProtection.RequiredStatusChecks.Contexts != null)
                                        {
                                            // Context is deprecated, replace with Checks when added to Octokit (not supported as of 8/30/22, but supported by GitHub API)
                                            foreach (string statusCheck in branchProtection.RequiredStatusChecks.Contexts)
                                                protectionSettings +=
                                                    $"\n{" + ",58}\n  {statusCheck}";
                                        }
                                    }

                                    if (branchProtection.RequiredConversationResolution != null)
                                        protectionSettings += $"\n{" + ",58}Require conversation resolution: {branchProtection.RequiredConversationResolution.Enabled}";
                                    
                                    if (branchProtection.RequiredSignatures != null)
                                        protectionSettings += $"\n{" + ",58}Require signed commits: {branchProtection.RequiredSignatures.Enabled}";
                                    
                                    if (branchProtection.RequiredLinearHistory != null)
                                        protectionSettings += $"\n{" + ",58}Require linear history: {branchProtection.RequiredLinearHistory.Enabled}";
                                    
                                    // Add "Require deployments to succeed before merging" setting when added to GitHub API and Octokit (not supported as of 8/30/22)

                                    if (branchProtection.EnforceAdmins != null)
                                    {
                                        if (branchProtection.EnforceAdmins.Enabled)
                                            protectionSettings +=
                                                $"\n{" + ",58}Protections apply to repo admins: {branchProtection.EnforceAdmins.Enabled}";
                                    }
                                    
                                    if (branchProtection.Restrictions != null)
                                    {
                                        protectionSettings +=
                                            $"\n{" + ",58}Push restricted to:";
                                        if (branchProtection.Restrictions.Users.Count > 0)
                                        {
                                            protectionSettings +=
                                                $"\n{" + ",58}  Users:";
                                            foreach (var user in branchProtection.Restrictions.Users)
                                                protectionSettings +=
                                                    $"\n{" + ",58}    {user.Login}";
                                        }

                                        if (branchProtection.Restrictions.Teams.Count > 0)
                                        {
                                            protectionSettings +=
                                                $"\n{" + ",58}  Teams:";
                                            foreach (var team in branchProtection.Restrictions.Teams)
                                                protectionSettings +=
                                                    $"\n{" + ",58}    {team.Name}";
                                        }
                                        // Add Apps when supported by Octokit (not supported as of 8/30/22, but supported by GitHub API)
                                    }
                                    
                                    if (branchProtection.AllowForcePushes != null)
                                        protectionSettings += $"\n{" + ",58}Allow force pushes: {branchProtection.AllowForcePushes.Enabled}";
                                    
                                    if (branchProtection.AllowDeletions != null)
                                        protectionSettings += $"\n{" + ",58}Allow deletions: {branchProtection.AllowDeletions.Enabled}";
                                    
                                    if (branchProtection.BlockCreations != null)
                                        protectionSettings += $"\n{" + ",58}Block creations: {branchProtection.BlockCreations.Enabled}";
                                }
                            }

                            Console.WriteLine("{0,-30} | {1,-30} | {2,-40}", repo.Name, branch.Name, protectionSettings);
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
