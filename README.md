# SCMKit

## Description
**S**ource **C**ode **M**anagement Attack Tool**kit** - SCMKit is a toolkit that can be used to attack SCM systems. SCMKit allows the user to specify the SCM system and attack module to use, along with specifying valid credentials (username/password or API key) to the respective SCM system. Currently, the SCM systems that SCMKit supports are GitHub Enterprise, GitLab Enterprise and Bitbucket Server. The attack modules supported include reconnaissance, privilege escalation and persistence. SCMKit was built in a modular approach, so that new modules and SCM systems can be added in the future by the information security community.

## Release
* Version 1.1 of SCMKit can be found in Releases

## Table of Contents

- [SCMKit](#scmkit)
- [Table of Contents](#table-of-contents)
- [Installation/Building](#installationbuilding)
  - [Libraries Used](#libraries-used)
  - [Pre-Compiled](#pre-compiled)
  - [Building Yourself](#building-yourself)
- [Usage](#usage)
  - [Arguments/Options](#argumentsoptions)
  - [Systems](#systems--s--system)
  - [Modules](#modules--m--module)
  - [Module Details Table](#Module-Details-Table)
- [Examples](#examples)
  - [List Repos](#List-repos)
  - [Search Repos](#Search-repos)
  - [Search Code](#Search-code)
  - [Search Files](#Search-files)
  - [List Snippets](#List-snippets)
  - [List Runners](#List-runners)
  - [List Gists](#List-gists)
  - [List Orgs](#List-orgs)
  - [Get Privileges of API Key](#Get-privileges-of-api-token)
  - [Add Admin](#Add-admin)
  - [Remove Admin](#Remove-admin)
  - [Create Access Token](#Create-access-token)
  - [List Access Tokens](#List-access-tokens)
  - [Remove Access Token](#Remove-access-token)
  - [Create SSH Key](#Create-ssh-key)
  - [List SSH Keys](#List-ssh-keys)
  - [Remove SSH Key](#Remove-ssh-key)
  - [List Admin Stats](#list-admin-stats)
  - [List Branch Protection](#list-branch-protection)
- [Detection](#detection)
- [References](#references)


## Installation/Building

### Libraries Used
The below 3rd party libraries are used in this project.

| Library | URL | License |
| ------------- | ------------- | ------------- |
| Octokit  | [https://github.com/octokit/octokit.net](https://github.com/octokit/octokit.net) | MIT License  |
| Fody  | [https://github.com/Fody/Fody](https://github.com/Fody/Fody) | MIT License  |
| GitLabApiClient  | [https://github.com/nmklotas/GitLabApiClient](https://github.com/nmklotas/GitLabApiClient) | MIT License  |
| Newtonsoft.Json  | [https://github.com/JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) | MIT License  |

### Pre-Compiled 

* Use the pre-compiled binary in Releases

### Building Yourself

Take the below steps to setup Visual Studio in order to compile the project yourself. This requires a .NET library that can be installed from the NuGet package manager.


* Load the Visual Studio project up and go to "Tools" --> "NuGet Package Manager" --> "Package Manager Settings"
* Go to "NuGet Package Manager" --> "Package Sources"
* Add a package source with the URL `https://api.nuget.org/v3/index.json`
* Install the below NuGet packages
  * `Install-Package Costura.Fody -Version 3.3.3`
  * `Install-Package Octokit`
  * `Install-Package GitLabApiClient`
  * `Install-Package Newtonsoft.Json`
* You can now build the project yourself!

## Usage

### Arguments/Options

* <b>-c, -credential </b> - credential for authentication (username:password or apiKey)
* <b>-s, -system </b> - system to attack (github,gitlab,bitbucket)
* <b>-u, -url </b> - URL for GitHub Enterprise, GitLab Enterprise or Bitbucket Server
* <b>-m, -module </b> - module to run
* <b>-o, -option </b> - options (when applicable)

### Systems (-s, -system)
* <b>github:</b> GitHub Enterprise
* <b>gitlab:</b> GitLab Enterprise
* <b>bitbucket:</b> Bitbucket Server

### Modules (-m, -module)
* <b>listrepo:</b> list all repos the current user can see
* <b>searchrepo:</b> search for a given repo
* <b>searchcode:</b> search for code containing keyword search term
* <b>searchfile:</b> search for filename containing keyword search term
* <b>listsnippet:</b> list all snippets of current user
* <b>listrunner:</b> list all GitLab runners available to current user
* <b>listgist:</b> list all gists of current user
* <b>listorg:</b> list all orgs current user belongs to
* <b>privs:</b> get privs of current API token
* <b>addadmin:</b> promote given user to admin role
* <b>removeadmin:</b> demote given user from admin role
* <b>createpat:</b> create personal access token for target user
* <b>listpat:</b> list personal access tokens for a target user
* <b>removepat:</b> remove personal access token for a target user
* <b>createsshkey:</b> create SSH key for current user
* <b>listsshkey:</b> list SSH keys for current user
* <b>removesshkey:</b> remove SSH key for current user
* <b>adminstats:</b> get admin stats (users, repos, orgs, gists)
* <b>protection:</b> get branch protection settings



### Module Details Table
The below table shows where each module is supported

Attack Scenario | Module  | Requires Admin? | GitHub Enterprise | GitLab Enterprise | Bitbucket Server
:---: |:---: | :---: | :---: | :---: | :---:
Reconnaissance | `listrepo` |  No | X | X | X
Reconnaissance |`searchrepo` |  No | X | X | X
Reconnaissance |`searchcode` |  No | X | X | X
Reconnaissance |`searchfile` |  No | X | X | X
Reconnaissance |`listsnippet` |  No |  | X | 
Reconnaissance |`listrunner` |  No |  | X | 
Reconnaissance |`listgist` |  No | X |  | 
Reconnaissance |`listorg` |  No | X |  | 
Reconnaissance |`privs` |  No | X | X | 
Reconnaissance |`protection` |  No | X |  | 
Persistence | `listsshkey` |  No | X | X | X
Persistence | `removesshkey` |  No | X | X | X
Persistence | `createsshkey` |  No | X | X | X
Persistence | `listpat` |  No |  | X | X
Persistence | `removepat` |  No |  | X | X
Persistence | `createpat` |  Yes (GitLab Enterprise only) |  | X | X
Privilege Escalation | `addadmin` |  Yes | X | X | X
Privilege Escalation | `removeadmin` |  Yes | X | X | X
Reconnaissance | `adminstats` |  Yes | X |  |


## Examples

### List Repos

#### Use Case

> *Discover repositories being used in a particular SCM system*

#### Syntax

Provide the `listrepo` module, along with any relevant authentication information and URL. This will output the repository name and URL.

##### GitHub Enterprise

This will list all repositories that a user can see.

`SCMKit.exe -s github -m listrepo -c userName:password -u https://github.something.local`

`SCMKit.exe -s github -m listrepo -c apiKey -u https://github.something.local`

##### GitLab Enterprise

This will list all repositories that a user can see.

`SCMKit.exe -s gitlab -m listrepo -c userName:password -u https://gitlab.something.local`

`SCMKit.exe -s gitlab -m listrepo -c apiKey -u https://gitlab.something.local`

##### Bitbucket Server

This will list all repositories that a user can see.

`SCMKit.exe -s bitbucket -m listrepo -c userName:password -u https://bitbucket.something.local`

`SCMKit.exe -s bitbucket -m listrepo -c apiKey -u https://bitbucket.something.local`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m listrepo -c username:password -u https://gitlab.hogwarts.local

==================================================
Module:         listrepo
System:         gitlab
Auth Type:      Username/Password
Options:
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/14/2022 8:30:47 PM
==================================================

                                    Name | Visibility |                                                URL
----------------------------------------------------------------------------------------------------------
                            MaraudersMap |    Private | https://gitlab.hogwarts.local/hpotter/maraudersmap
                            testingStuff |   Internal | https://gitlab.hogwarts.local/adumbledore/testingstuff
                               Spellbook |   Internal |    https://gitlab.hogwarts.local/hpotter/spellbook
       findShortestPathToGryffindorSword |   Internal | https://gitlab.hogwarts.local/hpotter/findShortestPathToGryffindorSword
                                  charms |     Public |      https://gitlab.hogwarts.local/hgranger/charms
                           Secret-Spells |   Internal | https://gitlab.hogwarts.local/adumbledore/secret-spells
                              Monitoring |   Internal | https://gitlab.hogwarts.local/gitlab-instance-10590c85/Monitoring
```

### Search Repos

#### Use Case

> *Search for repositories by repository name in a particular SCM system*

#### Syntax

Provide the `searchrepo` module and your search criteria in the `-o` command-line switch, along with any relevant authentication information and URL. This will output the matching repository name and URL.

##### GitHub Enterprise

The GitHub repo search is a "contains" search where the string you enter it will search for repos with names that contain your search term.

`SCMKit.exe -s github -m searchrepo -c userName:password -u https://github.something.local -o "some search term"`

`SCMKit.exe -s github -m searchrepo -c apikey -u https://github.something.local -o "some search term"`

##### GitLab Enterprise

The GitLab repo search is a "contains" search where the string you enter it will search for repos with names that contain your search term.

`SCMKit.exe -s gitlab -m searchrepo -c userName:password -u https://gitlab.something.local -o "some search term"`

`SCMKit.exe -s gitlab -m searchrepo -c apikey -u https://gitlab.something.local -o "some search term"`

##### Bitbucket Server

The Bitbucket repo search is a "starts with" search where the string you enter it will search for repos with names that start with your search term.

`SCMKit.exe -s bitbucket -m searchrepo -c userName:password -u https://bitbucket.something.local -o "some search term"`

`SCMKit.exe -s bitbucket -m searchrepo -c apikey -u https://bitbucket.something.local -o "some search term"`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m searchrepo -c apiKey -u https://gitlab.hogwarts.local -o "spell"

==================================================
Module:         searchrepo
System:         gitlab
Auth Type:      API Key
Options:        spell
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/14/2022 8:32:30 PM
==================================================

                                    Name | Visibility |                                                URL
----------------------------------------------------------------------------------------------------------
                               Spellbook |   Internal |    https://gitlab.hogwarts.local/hpotter/spellbook
                           Secret-Spells |   Internal | https://gitlab.hogwarts.local/adumbledore/secret-spells
```

### Search Code

#### Use Case

> *Search for code containing a given keyword in a particular SCM system*

#### Syntax

Provide the `searchcode` module and your search criteria in the `-o` command-line switch, along with any relevant authentication information and URL. This will output the URL to the matching code file, along with the line in the code that matched.

##### GitHub Enterprise

The GitHub code search is a "contains" search where the string you enter it will search for code that contains your search term in any line.

`SCMKit.exe -s github -m searchcode -c userName:password -u https://github.something.local -o "some search term"`

`SCMKit.exe -s github -m searchcode -c apikey -u https://github.something.local -o "some search term"`

##### GitLab Enterprise

The GitLab code search is a "contains" search where the string you enter it will search for code that contains your search term in any line.

`SCMKit.exe -s gitlab -m searchcode -c userName:password -u https://gitlab.something.local -o "some search term"`

`SCMKit.exe -s gitlab -m searchcode -c apikey -u https://gitlab.something.local -o "some search term"`

##### Bitbucket Server

The Bitbucket code search is a "contains" search where the string you enter it will search for code that contains your search term in any line.

`SCMKit.exe -s bitbucket -m searchcode -c userName:password -u https://bitbucket.something.local -o "some search term"`

`SCMKit.exe -s bitbucket -m searchcode -c apikey -u https://bitbucket.something.local -o "some search term"`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m searchcode -c username:password -u https://gitlab.hogwarts.local -o "api_key"

==================================================
Module:         searchcode
System:         gitlab
Auth Type:      Username/Password
Options:        api_key
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/14/2022 8:34:14 PM
==================================================


[>] URL: https://gitlab.hogwarts.local/adumbledore/secret-spells/stuff.txt
    |_ API_KEY=abc123

Total number of items matching code search: 1

```

### Search Files

#### Use Case

> *Search for files in repositories containing a given keyword in the file name in a particular SCM system*

#### Syntax

Provide the `searchfile` module and your search criteria in the `-o` command-line switch, along with any relevant authentication information and URL. This will output the URL to the matching file in its respective repository.

##### GitHub Enterprise

The GitLab file search is a "contains" search where the string you enter it will search for files that contains your search term in the file name.

`SCMKit.exe -s github -m searchfile -c userName:password -u https://github.something.local -o "some search term"`

`SCMKit.exe -s github -m searchfile -c apikey -u https://github.something.local -o "some search term"`

##### GitLab Enterprise

The GitLab file search is a "contains" search where the string you enter it will search for files that contains your search term in the file name.

`SCMKit.exe -s gitlab -m searchfile -c userName:password -u https://gitlab.something.local -o "some search term"`

`SCMKit.exe -s gitlab -m searchfile -c apikey -u https://gitlab.something.local -o "some search term"`

##### Bitbucket Server

The Bitbucket file search is a "contains" search where the string you enter it will search for files that contains your search term in the file name.

`SCMKit.exe -s bitbucket -m searchfile -c userName:password -u https://bitbucket.something.local -o "some search term"`

`SCMKit.exe -s bitbucket -m searchfile -c apikey -u https://bitbucket.something.local -o "some search term"`

#### Example Output

```

C:\source\SCMKit\SCMKit\bin\Release>SCMKit.exe -s bitbucket -m searchfile -c apikey -u http://bitbucket.hogwarts.local:7990 -o jenkinsfile

==================================================
Module:         searchfile
System:         bitbucket
Auth Type:      API Key
Options:        jenkinsfile
Target URL:     http://bitbucket.hogwarts.local:7990

Timestamp:      1/14/2022 10:17:59 PM
==================================================


[>] REPO: http://bitbucket.hogwarts.local:7990/scm/~HPOTTER/hpotter
    [>] FILE: Jenkinsfile

[>] REPO: http://bitbucket.hogwarts.local:7990/scm/STUD/cred-decryption
    [>] FILE: subDir/Jenkinsfile

Total matching results: 2

```

### List Snippets

#### Use Case

> *List snippets owned by the current user in GitLab*

#### Syntax

Provide the `listsnippet` module, along with any relevant authentication information and URL.

##### GitLab Enterprise

`SCMKit.exe -s gitlab -m listsnippet -c userName:password -u https://gitlab.something.local`

`SCMKit.exe -s gitlab -m listsnippet -c apikey -u https://gitlab.something.local`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m listsnippet -c username:password -u https://gitlab.hogwarts.local

==================================================
Module:         listsnippet
System:         gitlab
Auth Type:      Username/Password
Options:
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/14/2022 9:17:36 PM
==================================================

               Title |                                                                Raw URL
---------------------------------------------------------------------------------------------
        spell-script |                         https://gitlab.hogwarts.local/-/snippets/2/raw
```

### List Runners

#### Use Case

> *List all GitLab runners available to the current user in GitLab*

#### Syntax

Provide the `listrunner` module, along with any relevant authentication information and URL.

##### GitLab Enterprise

`SCMKit.exe -s gitlab -m listrunner -c userName:password -u https://gitlab.something.local`

`SCMKit.exe -s gitlab -m listrunner -c apikey -u https://gitlab.something.local`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m listrunner -c username:password -u https://gitlab.hogwarts.local

==================================================
Module:         listrunner
System:         gitlab
Auth Type:      Username/Password
Options:
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/25/2022 11:40:08 AM
==================================================

   ID |                 Name |                                      Repo Assigned
---------------------------------------------------------------------------------
    2 |        gitlab-runner | https://gitlab.hogwarts.local/hpotter/spellbook.git
    3 |        gitlab-runner | https://gitlab.hogwarts.local/hpotter/maraudersmap.git
    
```


### List Gists

#### Use Case

> *List gists owned by the current user in GitHub*

#### Syntax

Provide the `listgist` module, along with any relevant authentication information and URL.

##### GitHub Enterprise

`SCMKit.exe -s github -m listgist -c userName:password -u https://github.something.local`

`SCMKit.exe -s github -m listgist -c apikey -u https://github.something.local`

#### Example Output

```

C:\>SCMKit.exe -s github -m listgist -c username:password -u https://github-enterprise.hogwarts.local

==================================================
Module:         listgist
System:         github
Auth Type:      Username/Password
Options:
Target URL:     https://github-enterprise.hogwarts.local

Timestamp:      1/14/2022 9:43:23 PM
==================================================

                             Description | Visibility |                                                URL
----------------------------------------------------------------------------------------------------------
            Shell Script to Decode Spell |     public | https://github-enterprise.hogwarts.local/gist/c11c6bb3f47fe67183d5bc9f048412a1
            
```

### List Orgs

#### Use Case

> *List all organizations the current user belongs to in GitHub*

#### Syntax

Provide the `listorg` module, along with any relevant authentication information and URL.

##### GitHub Enterprise

`SCMKit.exe -s github -m listorg -c userName:password -u https://github.something.local`

`SCMKit.exe -s github -m listorg -c apiKey -u https://github.something.local`

#### Example Output

```

C:\>SCMKit.exe -s github -m listorg -c username:password -u https://github-enterprise.hogwarts.local

==================================================
Module:         listorg
System:         github
Auth Type:      Username/Password
Options:
Target URL:     https://github-enterprise.hogwarts.local

Timestamp:      1/14/2022 9:44:48 PM
==================================================

                          Name |                                                URL
-----------------------------------------------------------------------------------
                      Hogwarts | https://github-enterprise.hogwarts.local/api/v3/orgs/Hogwarts/repos
                      
```

### Get Privileges of API Token

#### Use Case

> *Get the assigned privileges to an access token being used in a particular SCM system*

#### Syntax

Provide the `privs` module, along with an API key and URL.

##### GitHub Enterprise

`SCMKit.exe -s github -m privs -c apiKey -u https://github.something.local`

##### GitLab Enterprise

`SCMKit.exe -s gitlab -m privs -c apiKey -u https://gitlab.something.local`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m privs -c apikey -u https://gitlab.hogwarts.local

==================================================
Module:         privs
System:         gitlab
Auth Type:      API Key
Options:
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/14/2022 9:18:27 PM
==================================================

          Token Name |    Active? |            Privilege |                                                            Description
---------------------------------------------------------------------------------------------------------------------------------
  hgranger-api-token |       True |                  api | Read-write for the complete API, including all groups and projects, the Container Registry, and the Package Registry.
  hgranger-api-token |       True |            read_user | Read-only for endpoints under /users. Essentially, access to any of the GET requests in the Users API.
  hgranger-api-token |       True |             read_api | Read-only for the complete API, including all groups and projects, the Container Registry, and the Package Registry.
  hgranger-api-token |       True |      read_repository |                      Read-only (pull) for the repository through git clone.
  hgranger-api-token |       True |     write_repository | Read-write (pull, push) for the repository through git clone. Required for accessing Git repositories over HTTP when 2FA is enabled.
  
```

### Add Admin

#### Use Case

> *Promote a normal user to an administrative role in a particular SCM system*

#### Syntax

Provide the `addadmin` module, along with any relevant authentication information and URL. Additionally, provide the target user you would like to add an administrative role to.

##### GitHub Enterprise

`SCMKit.exe -s github -m addadmin -c userName:password -u https://github.something.local -o targetUserName`

`SCMKit.exe -s github -m addadmin -c apikey -u https://github.something.local -o targetUserName`

##### GitLab Enterprise

`SCMKit.exe -s gitlab -m addadmin -c userName:password -u https://gitlab.something.local -o targetUserName`

`SCMKit.exe -s gitlab -m addadmin -c apikey -u https://gitlab.something.local -o targetUserName`

##### Bitbucket Server

Only username/password auth is supported to perform actions not related to repos or projects in Bitbucket.

`SCMKit.exe -s bitbucket -m addadmin -c userName:password -u https://bitbucket.something.local -o targetUserName`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m addadmin -c apikey -u https://gitlab.hogwarts.local -o hgranger

==================================================
Module:         addadmin
System:         gitlab
Auth Type:      API Key
Options:        hgranger
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/14/2022 9:19:32 PM
==================================================


[+] SUCCESS: The hgranger user was successfully added to the admin role.

```

### Remove Admin

#### Use Case

> *Demote an administrative user to a normal user role in a particular SCM system*

#### Syntax

Provide the `removeadmin` module, along with any relevant authentication information and URL. Additionally, provide the target user you would like to remove an administrative role from.

##### GitHub Enterprise

`SCMKit.exe -s github -m removeadmin -c userName:password -u https://github.something.local -o targetUserName`

`SCMKit.exe -s github -m removeadmin -c apikey -u https://github.something.local -o targetUserName`

##### GitLab Enterprise

`SCMKit.exe -s gitlab -m removeadmin -c userName:password -u https://gitlab.something.local -o targetUserName`

`SCMKit.exe -s gitlab -m removeadmin -c apikey -u https://gitlab.something.local -o targetUserName`

##### Bitbucket Server

Only username/password auth is supported to perform actions not related to repos or projects in Bitbucket.

`SCMKit.exe -s bitbucket -m removeadmin -c userName:password -u https://bitbucket.something.local -o targetUserName`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m removeadmin -c username:password -u https://gitlab.hogwarts.local -o hgranger

==================================================
Module:         removeadmin
System:         gitlab
Auth Type:      Username/Password
Options:        hgranger
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/14/2022 9:20:12 PM
==================================================


[+] SUCCESS: The hgranger user was successfully removed from the admin role.

```

### Create Access Token

#### Use Case

> *Create an access token to be used in a particular SCM system*

#### Syntax

Provide the `createpat` module, along with any relevant authentication information and URL. Additionally, provide the target user you would like to create an access token for.

##### GitLab Enterprise

This can only be performed as an administrator. You will provide the username that you would like to create a PAT for.

`SCMKit.exe -s gitlab -m createpat -c userName:password -u https://gitlab.something.local -o targetUserName`

`SCMKit.exe -s gitlab -m createpat -c apikey -u https://gitlab.something.local -o targetUserName`

##### Bitbucket Server

Creates PAT for the current user authenticating as. In Bitbucket you cannot create a PAT for another user, even as an admin. Only username/password auth is supported to perform actions not related to repos or projects in Bitbucket. Take note of the PAT ID that is shown after being created. You will need this when you need to remove the PAT in the future.

`SCMKit.exe -s bitbucket -m createpat -c userName:password -u https://bitbucket.something.local `

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m createpat -c username:password -u https://gitlab.hogwarts.local -o hgranger

==================================================
Module:         createpat
System:         gitlab
Auth Type:      Username/Password
Options:        hgranger
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/20/2022 1:51:23 PM
==================================================

   ID |         Name |                          Token
-----------------------------------------------------
   59 | SCMKIT-AaCND |           R3ySx_8HUn6UQ_6onETx

[+] SUCCESS: The hgranger user personal access token was successfully added.


```

### List Access Tokens

#### Use Case

> *List access tokens for a user on a particular SCM system*

#### Syntax

Provide the `listpat` module, along with any relevant authentication information and URL. 

##### GitLab Enterprise

Only requires admin if you want to list another user's PAT's. A regular user can list their own PAT's.

`SCMKit.exe -s gitlab -m listpat -c userName:password -u https://gitlab.something.local -o targetUser`

`SCMKit.exe -s gitlab -m listpat -c apikey -u https://gitlab.something.local -o targetUser`

##### Bitbucket Server

List access tokens for current user. Only username/password auth is supported to perform actions not related to repos or projects in Bitbucket.

`SCMKit.exe -s bitbucket -m listpat -c userName:password -u https://bitbucket.something.local`

List access tokens for another user (requires admin). Only username/password auth is supported to perform actions not related to repos or projects in Bitbucket.

`SCMKit.exe -s bitbucket -m listpat -c userName:password -u https://bitbucket.something.local -o targetUser`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m listpat -c username:password -u https://gitlab.hogwarts.local -o hgranger

==================================================
Module:         listpat
System:         gitlab
Auth Type:      Username/Password
Options:        hgranger
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/20/2022 1:54:41 PM
==================================================

   ID |                 Name |    Active? |                                             Scopes
----------------------------------------------------------------------------------------------
   59 |         SCMKIT-AaCND |       True |             api, read_repository, write_repository
    
```

### Remove Access Token

#### Use Case

> *Remove an access token for a user in a particular SCM system*

#### Syntax

Provide the `removepat` module, along with any relevant authentication information and URL. Additionally, provide the target user PAT ID you would like to remove an access token for.

##### GitLab Enterprise

Only requires admin if you want to remove another user's PAT. A regular user can remove their own PAT. You have to provide the PAT ID to remove. This ID was shown whenever you created the PAT and also when you listed the PAT.

`SCMKit.exe -s gitlab -m removepat -c userName:password -u https://gitlab.something.local -o patID`

`SCMKit.exe -s gitlab -m removepat -c apikey -u https://gitlab.something.local -o patID`

##### Bitbucket Server

Only username/password auth is supported to perform actions not related to repos or projects in Bitbucket. You have to provide the PAT ID to remove. This ID was shown whenever you created the PAT.

`SCMKit.exe -s bitbucket -m removepat -c userName:password -u https://bitbucket.something.local -o patID`

#### Example Output

```

C:\>SCMKit.exe -s gitlab -m removepat -c apikey -u https://gitlab.hogwarts.local -o 58

==================================================
Module:         removepat
System:         gitlab
Auth Type:      API Key
Options:        59
Target URL:     https://gitlab.hogwarts.local

Timestamp:      1/20/2022 1:56:47 PM
==================================================



[*] INFO: Revoking personal access token of ID: 59


[+] SUCCESS: The personal access token of ID 59 was successfully revoked.

```


### Create SSH Key

#### Use Case

> *Create an SSH key to be used in a particular SCM system*

#### Syntax

Provide the `createsshkey` module, along with any relevant authentication information and URL.

##### GitHub Enterprise

Creates SSH key for the current user authenticating as.

`SCMKit.exe -s github -m createsshkey -c userName:password -u https://github.something.local -o "ssh public key"`

`SCMKit.exe -s github -m createsshkey -c apiToken -u https://github.something.local -o "ssh public key"`

##### GitLab Enterprise

Creates SSH key for the current user authenticating as. Take note of the SSH key ID that is shown after being created. You will need this when you need to remove the SSH key in the future.

`SCMKit.exe -s gitlab -m createsshkey -c userName:password -u https://gitlab.something.local -o "ssh public key"`

`SCMKit.exe -s gitlab -m createsshkey -c apiToken -u https://gitlab.something.local -o "ssh public key"`


##### Bitbucket Server

Creates SSH key for the current user authenticating as. Only username/password auth is supported to perform actions not related to repos or projects in Bitbucket. Take note of the SSH key ID that is shown after being created. You will need this when you need to remove the SSH key in the future.

`SCMKit.exe -s bitbucket -m createsshkey -c userName:password -u https://bitbucket.something.local -o "ssh public key"`

#### Example Output

```

C:\>SCMKit.exe -s bitbucket -m createsshkey -c username:password -u https://bitbucket.hogwarts.local -o "ssh-rsa..."

==================================================
Module:         createsshkey
System:         bitbucket
Auth Type:      Username/Password
Options:        ssh-rsa ...
Target URL:     http://bitbucket.hogwarts.local:7990

Timestamp:      2/7/2022 1:02:31 PM
==================================================

  SSH Key ID
------------
          16

[+] SUCCESS: The hpotter user SSH key was successfully added.


```

### List SSH Keys

#### Use Case

> *List SSH keys for a user on a particular SCM system*

#### Syntax

Provide the `listsshkey` module, along with any relevant authentication information and URL. 

##### GitHub Enterprise

List SSH keys for current user. This will include SSH key ID's, which is needed when you would want to remove an SSH key.

`SCMKit.exe -s github -m listsshkey -c userName:password -u https://github.something.local`

`SCMKit.exe -s github -m listsshkey -c apiToken -u https://github.something.local`

##### GitLab Enterprise

List SSH keys for current user.

`SCMKit.exe -s gitlab -m listsshkey -c userName:password -u https://gitlab.something.local`

`SCMKit.exe -s gitlab -m listsshkey -c apiToken -u https://gitlab.something.local`


##### Bitbucket Server

List SSH keys for current user. Only username/password auth is supported to perform actions not related to repos or projects in Bitbucket.

`SCMKit.exe -s bitbucket -m listsshkey -c userName:password -u https://bitbucket.something.local`


#### Example Output

```

C:\>SCMKit.exe -s gitlab -m listsshkey -u http://gitlab.hogwarts.local -c apiToken

==================================================
Module:         listsshkey
System:         gitlab
Auth Type:      API Key
Options:
Target URL:     https://gitlab.hogwarts.local

Timestamp:      2/7/2022 4:09:40 PM
==================================================

  SSH Key ID |             SSH Key Value |                Title
---------------------------------------------------------------
           9 | .....p50edigBAF4lipVZkAM= |         SCMKIT-RLzie
          10 | .....vGJLPGHiTwIxW9i+xAs= |         SCMKIT-muFGU
    
```

### Remove SSH Key

#### Use Case

> *Remove an SSH key for a user in a particular SCM system*

#### Syntax

Provide the `removesshkey` module, along with any relevant authentication information and URL. Additionally, provide the target user SSH key ID to remove.

##### GitHub Enterprise

You have to provide the SSH key ID to remove. This ID was shown whenever you list SSH keys.

`SCMKit.exe -s github -m removesshkey -c userName:password -u https://github.something.local -o sshKeyID`

`SCMKit.exe -s github -m removesshkey -c apiToken -u https://github.something.local -o sshKeyID`

##### GitLab Enterprise

 You have to provide the SSH key ID to remove. This ID was shown whenever you created the SSH key and is also shown when listing SSH keys.

`SCMKit.exe -s gitlab -m removesshkey -c userName:password -u https://gitlab.something.local -o sshKeyID`

`SCMKit.exe -s gitlab -m removesshkey -c apiToken -u https://gitlab.something.local -o sshKeyID`

##### Bitbucket Server

Only username/password auth is supported to perform actions not related to repos or projects in Bitbucket. You have to provide the SSH key ID to remove. This ID was shown whenever you created the SSH key and is also shown when listing SSH keys.

`SCMKit.exe -s bitbucket -m removesshkey -c userName:password -u https://bitbucket.something.local -o sshKeyID`

#### Example Output

```

C:\>SCMKit.exe -s bitbucket -m removesshkey -u http://bitbucket.hogwarts.local:7990 -c username:password -o 16

==================================================
Module:         removesshkey
System:         bitbucket
Auth Type:      Username/Password
Options:        16
Target URL:     http://bitbucket.hogwarts.local:7990

Timestamp:      2/7/2022 1:48:03 PM
==================================================


[+] SUCCESS: The SSH key of ID 16 was successfully revoked.

```


### List Admin Stats

#### Use Case

> *List admin stats in GitHub Enterprise*

#### Syntax

Provide the `adminstats` module, along with any relevant authentication information and URL. Site admin access in GitHub Enterprise is required to use this module

##### GitHub Enterprise

`SCMKit.exe -s github -m adminstats -c userName:password -u https://github.something.local`

`SCMKit.exe -s github -m adminstats -c apikey -u https://github.something.local`

#### Example Output

```

C:\>SCMKit.exe -s github -m adminstats -c username:password -u https://github-enterprise.hogwarts.local

==================================================
Module:         adminstats
System:         github
Auth Type:      Username/Password
Options:
Target URL:     https://github-enterprise.hogwarts.local

Timestamp:      1/14/2022 9:45:50 PM
==================================================

     Admin Users |  Suspended Users |      Total Users
------------------------------------------------------
               1 |                0 |                5


     Total Repos |      Total Wikis
-----------------------------------
               4 |                0


      Total Orgs |   Total Team Members |      Total Teams
----------------------------------------------------------
               1 |                    0 |                0


   Private Gists |     Public Gists
-----------------------------------
               0 |                1
               
```

### List Branch Protection

#### Use Case

> *List branch protections in GitHub Enterprise*

#### Syntax

Provide the `protection` module, along with any relevant authentication information and URL. Optionally, supply a string in the options parameter to return matching results contained in repo names

##### GitHub Enterprise

`SCMKit.exe -s github -m protection -c userName:password -u https://github.something.local`

`SCMKit.exe -s github -m protection -c apikey -u https://github.something.local`

`SCMKit.exe -s github -m protection -c apikey -u https://github.something.local -o reponame`

#### Example Output

```
C:\>.\SCMKit.exe -u http://github.hogwarts.local -s github -c apiToken -m protection -o public-r

==================================================
Module:         protection
System:         github
Auth Type:      API Key
Options:        public-r
Target URL:     http://github.hogwarts.local

Timestamp:      8/29/2022 2:02:42 PM
==================================================

                     Repo |                    Branch |                                         Protection
----------------------------------------------------------------------------------------------------------
              public-repo |                       dev | Protected: True
                                                        Status checks must pass before merge:
                                                          Branch must be up-to-date before merge: True
                                                        Owner review required before merge: True
                                                        Approvals required before merge: 2
                                                        Protections apply to repo admins: True
              public-repo |                      main | Protected: False
```

## Detection

Below are static signatures for the specific usage of this tool in its default state:

* Project GUID - `{266C644A-69B1-426B-A47C-1CF32B211F80}`
  * See [SCMKit Yara Rule](Detections/SCMKit.yar) in this repo.
* User Agent String - `SCMKIT-5dc493ada400c79dd318abbe770dac7c`
  * See [SCMKit Snort Rule](Detections/SCMKit.rules) in this repo.
* Access Token & SSH Key Names - Access tokens and SSH keys that are created using the tool are prepended with `SCMKIT-` for the name.

For detection guidance of the techniques used by the tool, see the X-Force Red [blog post](https://securityintelligence.com/posts/abusing-source-code-management-systems).

## References
* Bitbucket API Documentation 
  * https://developer.atlassian.com/server/bitbucket/reference/rest-api/
* Octokit Documentation
  * https://octokitnet.readthedocs.io/en/latest/
  * https://github.com/octokit/octokit.net
* GitHub API Documentation
  * https://docs.github.com/en/rest/overview
* GitLab API Documentation
  * https://docs.gitlab.com/ee/api/api_resources.html
* GitLabApiClient Nuget Package Documentation
  * https://github.com/nmklotas/GitLabApiClient
