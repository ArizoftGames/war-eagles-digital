# update_version.ps1
$git_version = git describe --tags --abbrev=0 --always
if ($LASTEXITCODE -ne 0) {
    $git_version = "0.1.0"  # Fallback SemVer
}
$project_file = "project.godot"
$new_content = Get-Content $project_file -Raw

# Update SemVer
$new_content = $new_content -replace 'config/version=".*?"', "config/version=`"$git_version`""

# Increment build number
$build_number = [regex]::Match($new_content, 'config/build_number=(\d+)').Groups[1].Value
if (-not $build_number) {
    $build_number = 1000000  # Fallback if not found
    $new_content += "`nconfig/build_number=$build_number"
} else {
    $build_number = [int]$build_number + 1
    $new_content = $new_content -replace 'config/build_number=\d+', "config/build_number=$build_number"
}

Set-Content $project_file $new_content
Write-Output "Updated project.godot with version: $git_version, build: $build_number"