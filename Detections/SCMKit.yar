rule SCMKit_Signatures
{
    meta:
        description = "Static signatures for the SCMKit tool."
        md5 = "9b4b2a06aa840afcbbfe2d412f99b4a8"
        rev = 1
        author = "Brett Hawkins"
    strings:
        $typelibguid = "266c644a-69b1-426b-a47c-1cf32b211f80" ascii nocase wide
        $gitlabModules = "SCMKit.modules.gitlab" ascii nocase wide
        $githubModules = "SCMKit.modules.github" ascii nocase wide
        $bitbucketModules = "SCMKit.modules.bitbucket" ascii nocase wide
    condition:
        uint16(0) == 0x5A4D and $typelibguid and $gitlabModules and $githubModules and $bitbucketModules
}
