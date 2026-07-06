
$packagesSource = "C:\Users\nikky\dev\vrchat\projects\Astral Plane\Packages"
$packages = @(
    'moe.nikky.common',
    'moe.nikky.kinetic-controls',
    'moe.nikky.kinetic-controls.audiolink'
    )

# rsync -avu --delete "/home/user/A/" "/home/user/B"
foreach($package in $packages) {
    $source = Join-Path $packagesSource $package -Resolve
    $target = Join-Path $PSScriptRoot $package -Resolve
    echo "processing $package copying $source -> $target"
    ROBOCOPY $source $target /MIR
}