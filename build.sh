
# sudo apt install libgconf-2-4


unity=$UNITY_PATH
script_path=$(dirname "$0") 
project_path="$script_path/ProjectFiles"
build_path="$script_path/builds"
server_build_path="$build_path/server"
client_build_path="$build_path/client"
log_file="$build_path/log.txt"


mkdir -p $build_path
mkdir -p $server_build_path
mkdir -p $client_build_path
> $log_file

if [ -z "$unity" ]; then
    echo "You need to set your UNITY_PATH it might be:"
    echo "  - MacOS: /Applications/Unity/Hub/Editor/2019.3.0f3/Unity.app/Contents/MacOS/Unity"
    echo "  - Linux: $HOME/Unity/Hub/Editor/2019.3.0f3/Editor"
    echo "  - Windows: ¯\\_(ツ)_/¯"
else
    echo "¯\\_(ツ)_/¯"
    $unity -nographics -batchmode -projectPath "$project_path" -executeMethod Build.ServerBuild.Build -buildFolder "$server_build_path" -quit -logFile $log_file
    echo "Build complete" >> $log_file
fi

