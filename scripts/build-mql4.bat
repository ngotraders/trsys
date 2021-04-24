SET PATH=C:\Program Files (x86)\FXCM MetaTrader4;%PATH
pushd %~dp0
metaeditor.exe /compile:"..\mql4\TrsysPublisher.mq4"
move ..\mql\TrsysPublisher.ex4 ..\static\downloads
metaeditor.exe /compile:"..\mql4\TrsysSubscriber.mq4"
move ..\mql\TrsysSubscriber.ex4 ..\static\downloads
popd