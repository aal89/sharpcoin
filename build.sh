#!/bin/sh

BASEDIR=$(dirname "$0")
VERSION=`cat $BASEDIR/Core.sln | grep version | tail -c 7 | rev | cut -c 2- | rev`

dotnet publish -c Release -r win-x86 --self-contained true /p:PublishSingleFile=true
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true
dotnet restore

# win-x86

cd $BASEDIR/Core/bin/Release/netcoreapp3.0/win-x86
rm -r 'sharpcoin-core-v'"$VERSION"'_win-x86'
rm 'sharpcoin-core-v'"$VERSION"'_win-x86.zip'

mv publish 'sharpcoin-core-v'"$VERSION"'_win-x86'
zip -r 'sharpcoin-core-v'"$VERSION"'_win-x86.zip' 'sharpcoin-core-v'"$VERSION"'_win-x86'

# win-x64

cd ../win-x64
rm -r 'sharpcoin-core-v'"$VERSION"'_win-x64'
rm 'sharpcoin-core-v'"$VERSION"'_win-x64.zip'

mv publish 'sharpcoin-core-v'"$VERSION"'_win-x64'
zip -r 'sharpcoin-core-v'"$VERSION"'_win-x64.zip' 'sharpcoin-core-v'"$VERSION"'_win-x64'

# osx-x64

cd ../osx-x64
rm -r 'sharpcoin-core-v'"$VERSION"'_osx-x64'
rm 'sharpcoin-core-v'"$VERSION"'_osx-x64.zip'

mv publish 'sharpcoin-core-v'"$VERSION"'_osx-x64'
zip -r 'sharpcoin-core-v'"$VERSION"'_osx-x64.zip' 'sharpcoin-core-v'"$VERSION"'_osx-x64'
