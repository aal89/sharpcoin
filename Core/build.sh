dotnet publish -c Release -r win-x86 --self-contained true /p:PublishSingleFile=true
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
dotnet restore

cd bin/Release/netcoreapp3.0/win-x86
zip -r sharpcoin-core-v0.1_win-x86.zip publish

cd ../win-x64
zip -r sharpcoin-core-v0.1_win-x64.zip publish

cd ../linux-x64
zip -r sharpcoin-core-v0.1_linux-x64.zip publish

cd ../osx-x64
zip -r sharpcoin-core-v0.1_osx-x64.zip publish
