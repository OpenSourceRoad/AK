PowerShell -ExecutionPolicy Unrestricted .\CreatePerformanceCounters.ps1 -CategoryName "Akurates Performance" -CounterFilePath CounterCreateData.xml >> "%TEMP%\StartupLog.txt" 2>&1

EXIT /B 0