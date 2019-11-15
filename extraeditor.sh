#! /bin/bash

currdirectory=$(pwd)
newfolder=$currdirectory/../Parking3

mkdir -p $newfolder

# Project files
cp -a $currdirectory/ProjectFiles $newfolder/

rm -rf $newfolder/ProjectFiles/Assets $newfolder/ProjectFiles/ProjectSettings

ln -s $currdirectory/ProjectFiles/Assets $newfolder/ProjectFiles/Assets
ln -s $currdirectory/ProjectFiles/ProjectSettings $newfolder/ProjectFiles/ProjectSetings


# multiplayer
cp -a $currdirectory/multiplayer $newfolder
