# Push all nuget packages

$nugetexe = "..\..\.nuget\nuget.exe"

$files=get-childitem("bin\release\*.nupkg")

foreach ($file in $files) 
{
	$fileName = $file.Name
	# exclude symbols packages because they are automatically pushed when pushing the standard package
	if (!$fileName.contains("symbols"))
	{
		
		$cmd = "$nugetexe push ""$file"""
		write $cmd
		iex $cmd
	}
}