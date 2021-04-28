SET PATH=C:\Program Files\OANDA MetaTrader 5;%PATH
pushd %~dp0
metaeditor64.exe /compile:"..\mql5\TrsysPublisher.mq5"
move ..\mql5\TrsysPublisher.ex5 ..\static\downloads
metaeditor64.exe /compile:"..\mql5\TrsysSubscriber.mq5"
move ..\mql5\TrsysSubscriber.ex5 ..\static\downloads
popd