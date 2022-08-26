using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCMKit
{
    class SCMKit
    {

        // variables to be used
        private static string module = "";
        private static string credential = "";
        private static string system = "";
        private static string url = "";
        private static string option = "";
        private static List<string> approvedModules = new List<string> { "listrepo", "repolist", "reposearch", "codesearch", "snippetlist", "gistlist", "orglist", "searchrepo", "searchcode", "searchfile", "listsnippet", "listgist", "listorg", "privs", "addadmin", "removeadmin", "createpat", "removepat", "listpat", "adminstats", "listrunner", "runnerlist", "createsshkey", "removesshkey", "listsshkey", "protection" };


        static async Task Main(string[] args)
        {

            try
            {

                Dictionary<string, string> argDict = library.Utils.ParseTheArguments(args); // dictionary to hold arguments

                // if no arguments given, display help and return
                if ((args.Length > 0 && argDict.Count == 0) || argDict.ContainsKey("h") || argDict.ContainsKey("help"))
                {
                    library.Utils.HelpMe();
                    return;
                }

                // if url is not set, display message and exit
                if ((!argDict.ContainsKey("u") && !argDict.ContainsKey("url")))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Must supply a URL. See the README.");
                    return;
                }

                // if system is not set, display message and exit
                if ((!argDict.ContainsKey("s") && !argDict.ContainsKey("system")))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Must supply a system. See the README.");
                    return;
                }

                // if both module and credential are not given, display message and exit
                if ((!argDict.ContainsKey("m") && !argDict.ContainsKey("module")) && (!argDict.ContainsKey("c") && !argDict.ContainsKey("credential")))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Must supply both a module and credential. See the README.");
                    return;
                }

                // initialize variables

                // module
                if (argDict.ContainsKey("m") || argDict.ContainsKey("module"))
                {
                    if (argDict.ContainsKey("m"))
                    {
                        module = argDict["m"];
                    }
                    else
                    {
                        module = argDict["module"];
                    }
                }

                // options
                if (argDict.ContainsKey("o") || argDict.ContainsKey("options"))
                {
                    if (argDict.ContainsKey("o"))
                    {
                        option = argDict["o"];
                    }
                    else
                    {
                        option = argDict["options"];
                    }
                }


                // credential
                if (argDict.ContainsKey("c") || argDict.ContainsKey("credential"))
                {
                    if (argDict.ContainsKey("c"))
                    {
                        credential = argDict["c"];
                    }
                    else
                    {
                        credential = argDict["credential"];
                    }

                }


                // url
                if (argDict.ContainsKey("u") || argDict.ContainsKey("url"))
                {
                    if (argDict.ContainsKey("u"))
                    {
                        url = argDict["u"];
                    }
                    else
                    {
                        url = argDict["url"];
                    }

                }


                // system
                if (argDict.ContainsKey("s") || argDict.ContainsKey("system"))
                {
                    if (argDict.ContainsKey("s"))
                    {
                        system = argDict["s"];
                    }
                    else
                    {
                        system = argDict["system"];
                    }

                }

                // determine if invalid module was given
                if (!approvedModules.Contains(module.ToLower()))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Invalid module given. Please see the README for approved modules.");
                    return;
                }


                // system is github
                if (system.ToLower().Equals("github"))
                {

                    // get to the appropriate module that user specified
                    switch (module.ToLower())
                    {
                        case "listrepo":
                            await modules.github.RepoList.execute(credential, url, option, system);
                            break;
                        case "searchrepo":
                            await modules.github.RepoSearch.execute(credential, url, option, system);
                            break;
                        case "searchcode":
                            await modules.github.CodeSearch.execute(credential, url, option, system);
                            break;
                        case "searchfile":
                            await modules.github.FileSearch.execute(credential, url, option, system);
                            break;
                        case "listgist":
                            await modules.github.GistList.execute(credential, url, option, system);
                            break;
                        case "listorg":
                            await modules.github.OrgList.execute(credential, url, option, system);
                            break;
                        case "repolist":
                            await modules.github.RepoList.execute(credential, url, option, system);
                            break;
                        case "reposearch":
                            await modules.github.RepoSearch.execute(credential, url, option, system);
                            break;
                        case "codesearch":
                            await modules.github.CodeSearch.execute(credential, url, option, system);
                            break;
                        case "filesearch":
                            await modules.github.FileSearch.execute(credential, url, option, system);
                            break;
                        case "gistlist":
                            await modules.github.GistList.execute(credential, url, option, system);
                            break;
                        case "orglist":
                            await modules.github.OrgList.execute(credential, url, option, system);
                            break;
                        case "privs":
                            await modules.github.Privs.execute(credential, url, option, system);
                            break;
                        case "addadmin":
                            await modules.github.AddAdmin.execute(credential, url, option, system);
                            break;
                        case "removeadmin":
                            await modules.github.RemoveAdmin.execute(credential, url, option, system);
                            break;
                        case "adminstats":
                            await modules.github.AdminStats.execute(credential, url, option, system);
                            break;
                        case "createsshkey":
                            await modules.github.CreateSSHKey.execute(credential, url, option, system);
                            break;
                        case "listsshkey":
                            await modules.github.ListSSHKeys.execute(credential, url, option, system);
                            break;
                        case "removesshkey":
                            await modules.github.RemoveSSHKey.execute(credential, url, option, system);
                            break;
                        case "protection":
                            await modules.github.BranchProtection.execute(credential, url, option, system);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: That module is not supported for " + system + ". Please see README");
                            Console.WriteLine("");
                            Environment.Exit(1);
                            break;
                    }

                }

                // system is gitlab
                else if (system.ToLower().Equals("gitlab"))
                {
                    // get to the appropriate module that user specified
                    switch (module.ToLower())
                    {
                        case "listrepo":
                            await modules.gitlab.RepoList.execute(credential, url, option, system);
                            break;
                        case "searchrepo":
                            await modules.gitlab.RepoSearch.execute(credential, url, option, system);
                            break;
                        case "searchcode":
                            await modules.gitlab.CodeSearch.execute(credential, url, option, system);
                            break;
                        case "searchfile":
                            await modules.gitlab.FileSearch.execute(credential, url, option, system);
                            break;
                        case "listsnippet":
                            await modules.gitlab.SnippetList.execute(credential, url, option, system);
                            break;
                        case "repolist":
                            await modules.gitlab.RepoList.execute(credential, url, option, system);
                            break;
                        case "reposearch":
                            await modules.gitlab.RepoSearch.execute(credential, url, option, system);
                            break;
                        case "codesearch":
                            await modules.gitlab.CodeSearch.execute(credential, url, option, system);
                            break;
                        case "filesearch":
                            await modules.gitlab.FileSearch.execute(credential, url, option, system);
                            break;
                        case "snippetlist":
                            await modules.gitlab.SnippetList.execute(credential, url, option, system);
                            break;
                        case "privs":
                            await modules.gitlab.Privs.execute(credential, url, option, system);
                            break;
                        case "createpat":
                            await modules.gitlab.CreatePAT.execute(credential, url, option, system);
                            break;
                        case "listpat":
                            await modules.gitlab.ListPAT.execute(credential, url, option, system);
                            break;
                        case "removepat":
                            await modules.gitlab.RemovePAT.execute(credential, url, option, system);
                            break;
                        case "addadmin":
                            await modules.gitlab.AddAdmin.execute(credential, url, option, system);
                            break;
                        case "removeadmin":
                            await modules.gitlab.RemoveAdmin.execute(credential, url, option, system);
                            break;
                        case "listrunner":
                            await modules.gitlab.RunnerList.execute(credential, url, option, system);
                            break;
                        case "runnerlist":
                            await modules.gitlab.RunnerList.execute(credential, url, option, system);
                            break;
                        case "createsshkey":
                            await modules.gitlab.CreateSSHKey.execute(credential, url, option, system);
                            break;
                        case "removesshkey":
                            await modules.gitlab.RemoveSSHKey.execute(credential, url, option, system);
                            break;
                        case "listsshkey":
                            await modules.gitlab.ListSSHKeys.execute(credential, url, option, system);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: That module is not supported for " + system + ". Please see README");
                            Console.WriteLine("");
                            Environment.Exit(1);
                            break;
                    }
                }

                // system is bitbucket
                else if (system.ToLower().Equals("bitbucket"))
                {
                    // get to the appropriate module that user specified
                    switch (module.ToLower())
                    {
                        case "listrepo":
                            await modules.bitbucket.RepoList.execute(credential, url, option, system);
                            break;
                        case "searchrepo":
                            await modules.bitbucket.RepoSearch.execute(credential, url, option, system);
                            break;
                        case "searchcode":
                            await modules.bitbucket.CodeSearch.execute(credential, url, option, system);
                            break;
                        case "searchfile":
                            await modules.bitbucket.FileSearch.execute(credential, url, option, system);
                            break;
                        case "repolist":
                            await modules.bitbucket.RepoList.execute(credential, url, option, system);
                            break;
                        case "reposearch":
                            await modules.bitbucket.RepoSearch.execute(credential, url, option, system);
                            break;
                        case "codesearch":
                            await modules.bitbucket.CodeSearch.execute(credential, url, option, system);
                            break;
                        case "filesearch":
                            await modules.bitbucket.FileSearch.execute(credential, url, option, system);
                            break;
                        case "createpat":
                            await modules.bitbucket.CreatePAT.execute(credential, url, option, system);
                            break;
                        case "listpat":
                            await modules.bitbucket.ListPAT.execute(credential, url, option, system);
                            break;
                        case "removepat":
                            await modules.bitbucket.RemovePAT.execute(credential, url, option, system);
                            break;
                        case "addadmin":
                            await modules.bitbucket.AddAdmin.execute(credential, url, option, system);
                            break;
                        case "removeadmin":
                            await modules.bitbucket.RemoveAdmin.execute(credential, url, option, system);
                            break;
                        case "createsshkey":
                            await modules.bitbucket.CreateSSHKey.execute(credential, url, option, system);
                            break;
                        case "removesshkey":
                            await modules.bitbucket.RemoveSSHKey.execute(credential, url, option, system);
                            break;
                        case "listsshkey":
                            await modules.bitbucket.ListSSHKeys.execute(credential, url, option, system);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: That module is not supported for " + system + ". Please see README");
                            Console.WriteLine("");
                            break;
                    }
                }

                // invalid system given
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Invalid system given. Please see the README for approved modules.");
                    return;
                }

            } // end try

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR : {0}", ex.Message);
            }


        } // end main

    } // end class

} // end namespace