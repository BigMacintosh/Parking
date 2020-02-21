
# sudo apt install libgconf-2-4


unity=$UNITY_PATH
script_path=$(dirname "$0") 
echo "$script_path"
project_path="$script_path/ProjectFiles"
build_path="$script_path/builds"
server_build_path="$build_path/server"
client_build_path="$build_path/client"
log_file="$build_path/log.txt"
server_log_file="/tmp/parking-server-build.log"
client_log_file="/tmp/parking-client-build.log"


mkdir -p $build_path
mkdir -p $server_build_path
mkdir -p $client_build_path
> $log_file


if [ -z "$unity" ]; then
    echo "You need to set your UNITY_PATH it might be:"
    echo "  - MacOS: /Applications/Unity/Hub/Editor/2019.3.0f3/Unity.app/Contents/MacOS/Unity"
    echo "  - Linux: $HOME/Unity/Hub/Editor/2019.3.0f3/Editor/Unity"
    echo "  - Windows: ¯\\_(ツ)_/¯"
else
    echo "Starting Server build"
    echo "STARTING BUILD PROCESS" >> $log_file
    echo "----------------------" >> $log_file
    echo "Server Build Unity Logs:" >> $log_file
    echo "\n\n\n" >> $log_file
    if $unity -nographics -batchmode -projectPath "$project_path" -executeMethod ServerBuild.BuildServer -quit -logFile $server_log_file
    then
        cat $server_log_file >> $log_file
        rm $server_log_file
        echo "\n\n\n" >> $log_file
        echo "---------------------" >> $log_file
        echo "Server Build Complete" >> $log_file
        echo "---------------------" >> $log_file
        echo "Client Build Unity Logs:" >> $log_file
        echo "\n\n\n" >> $log_file

        echo "Server build complete"
        echo "Build saved at: ${server_build_path}"
        echo " ---"
        echo "Starting Client Build"

        if $unity -nographics -batchmode -projectPath "$project_path" -executeMethod ServerBuild.BuildClient -quit -logFile $client_log_file
        then 
            cat $client_log_file >> $log_file
            rm $client_log_file
            echo "\n\n\n" >> $log_file
            echo "---------------------" >> $log_file
            echo "Server Build Complete" >> $log_file
            echo "---------------------" >> $log_file
            echo "All builds complete" >> $log_file
            echo "Client build complete"
            echo "Build saved at: ${client_build_path}"
        else
            echo "Client build failed"
            echo "Detailed logs can be found at: ${log_file}"
            echo "\n\n\n" >> $log_file
            echo "-------------------" >> $log_file
            echo "Server Build Failed" >> $log_file
            echo "-------------------" >> $log_file
        fi
    else
        echo "Server build failed"
        echo "Detailed logs can be found at: ${log_file}"
        echo "\n\n\n" >> $log_file
        echo "-------------------" >> $log_file
        echo "Server Build Failed" >> $log_file
        echo "-------------------" >> $log_file
        
    fi
fi

