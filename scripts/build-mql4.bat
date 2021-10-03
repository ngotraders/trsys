SET PATH=C:\Program Files (x86)\FXCM MetaTrader4;%PATH
pushd %~dp0
metaeditor.exe /compile:"..\mql4\TrsysPublisher2.mq4"
move ..\mql4\TrsysPublisher2.ex4 ..\static\downloads
metaeditor.exe /compile:"..\mql4\TrsysSubscriber2.mq4"
move ..\mql4\TrsysSubscriber2.ex4 ..\static\downloads
popd