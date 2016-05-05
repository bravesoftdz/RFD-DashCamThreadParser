for ($i=1407; $i -le 1594; $i++) {
  Write-Host "Downloading page $($i)"
  $source = "http://forums.redflagdeals.com/merged-post-videos-daily-events-your-dash-cam-here-your-dash-cam-only-1106658/$($i)/"
  $destination = "Page$($i).html"
  Invoke-WebRequest $source -OutFile $destination
  Start-Sleep -s 10
}