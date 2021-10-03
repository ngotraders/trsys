SET PATH=C:\Program Files\OANDA MetaTrader 5;%PATH
pushd %~dp0
metaeditor64.exe /compile:"..\mql5\TrsysPublisher2.mq5"
move ..\mql5\TrsysPublisher2.ex5 ..\static\downloads
metaeditor64.exe /compile:"..\mql5\TrsysSubscriber2.mq5"
move ..\mql5\TrsysSubscriber2.ex5 ..\static\downloads
popd