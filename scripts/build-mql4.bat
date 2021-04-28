SET PATH=C:\Program Files (x86)\FXCM MetaTrader4;%PATH
pushd %~dp0
metaeditor.exe /compile:"..\mql4\TrsysPublisher.mq4"
move ..\mql4\TrsysPublisher.ex4 ..\static\downloads
metaeditor.exe /compile:"..\mql4\TrsysSubscriber.mq4"
move ..\mql4\TrsysSubscriber.ex4 ..\static\downloads
popd