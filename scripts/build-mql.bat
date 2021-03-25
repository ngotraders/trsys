SET PATH=C:\Program Files (x86)\FXCM MetaTrader4;%PATH
pushd %~dp0
metaeditor.exe /compile:"..\mql\TrsysPublisher.mq4"
move ..\mql\TrsysPublisher.ex4 ..\static\downloads
metaeditor.exe /compile:"..\mql\TrsysSubscriber.mq4"
move ..\mql\TrsysSubscriber.ex4 ..\static\downloads
popd