$uri = "https://xref.docs.microsoft.com/autocomplete?text=" + [System.Web.HTTPUtility]::UrlEncode($Args[0])
Invoke-WebRequest $uri | ConvertFrom-Json | ForEach-Object -Process { Write-Output $_.Uid }