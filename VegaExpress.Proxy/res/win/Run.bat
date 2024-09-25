@echo off
where  VegaExpressProxy >VegaExpressProxy.path
set /p PortFusion=<VegaExpressProxy.path
dir %PortFusion%

echo Signing ...
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\signtool.exe" sign /d "VegaExpressProxy Native Binary" /du "https://liiksoft.com" /f "c:\liiksoft\certificates\vegaexpress-proxy.pfx" /t http://timestamp.verisign.com/scripts/timestamp.dll "%VegaExpressProxy%"